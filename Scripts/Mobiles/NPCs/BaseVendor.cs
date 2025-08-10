using System;
using System.Collections;
using System.Collections.Generic;
using Server.Accounting;
using Server.ContextMenus;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Network.Packets;
using Server.Regions;

namespace Server.Mobiles
{
    public interface IVendor
    {
        bool OnBuyItems(Mobile from, List<BuyItemResponse> list);
        bool OnSellItems(Mobile from, List<SellItemResponse> list);

        DateTime LastRestock { get; set; }
        TimeSpan RestockDelay { get; }
        void Restock();
    }

    public class BuyItemStateComparer : IComparer<BuyItemState>
	{
		public int Compare(BuyItemState l, BuyItemState r)
		{
			if (l == null && r == null)
			{
				return 0;
			}
			if (l == null)
			{
				return -1;
			}
			if (r == null)
			{
				return 1;
			}

			return l.MySerial.CompareTo(r.MySerial);
		}
	}

	public class BuyItemResponse
	{
		private readonly Serial m_Serial;
		private readonly int m_Amount;

		public BuyItemResponse(Serial serial, int amount)
		{
			m_Serial = serial;
			m_Amount = amount;
		}

		public Serial Serial => m_Serial;

		public int Amount => m_Amount;
	}

	public class SellItemResponse
	{
		private readonly Item m_Item;
		private readonly int m_Amount;

		public SellItemResponse(Item i, int amount)
		{
			m_Item = i;
			m_Amount = amount;
		}

		public Item Item => m_Item;

		public int Amount => m_Amount;
	}

	public class SellItemState
	{
		private readonly Item m_Item;
		private readonly int m_Price;
		private readonly string m_Name;

		public SellItemState(Item item, int price, string name)
		{
			m_Item = item;
			m_Price = price;
			m_Name = name;
		}

		public Item Item => m_Item;

		public int Price => m_Price;

		public string Name => m_Name;
	}

	public class BuyItemState
	{
		private readonly Serial m_ContSer;
		private readonly Serial m_MySer;
		private readonly int m_ItemID;
		private readonly int m_Amount;
		private readonly int m_Hue;
		private readonly int m_Price;
		private readonly string m_Desc;

		public BuyItemState(string name, Serial cont, Serial serial, int price, int amount, int itemID, int hue)
		{
			m_Desc = name;
			m_ContSer = cont;
			m_MySer = serial;
			m_Price = price;
			m_Amount = amount;
			m_ItemID = itemID;
			m_Hue = hue;
		}

		public int Price => m_Price;

		public Serial MySerial => m_MySer;

		public Serial ContainerSerial => m_ContSer;

		public int ItemID => m_ItemID;

		public int Amount => m_Amount;

		public int Hue => m_Hue;

		public string Description => m_Desc;
	}

    public enum VendorShoeType
    {
        None,
        Shoes,
        Boots,
        Sandals,
        ThighBoots
    }

    public abstract class BaseVendor : BaseCreature, IVendor
    {
        public static bool UseVendorEconomy = true;
        public static int BuyItemChange = Config.Get("Vendors.BuyItemChange", 1000);
        public static int SellItemChange = Config.Get("Vendors.SellItemChange", 1000);
        public static int EconomyStockAmount = Config.Get("Vendors.EconomyStockAmount", 500);
        public static TimeSpan DelayRestock = TimeSpan.FromMinutes(Config.Get("Vendors.RestockDelay", 60));
        public static int MaxSell = Config.Get("Vendors.MaxSell", 500);

        public static List<BaseVendor> AllVendors { get; private set; }

        static BaseVendor()
        {
            AllVendors = new List<BaseVendor>(0x4000);
        }

        protected abstract List<SBInfo> SBInfos { get; }

        private readonly ArrayList m_ArmorBuyInfo = new ArrayList();
        private readonly ArrayList m_ArmorSellInfo = new ArrayList();

        private DateTime m_LastRestock;

        public override bool BardImmune => true;

        public override bool PlayerRangeSensitive => true;

        public override bool AlwaysInnocent => true;

        public virtual bool IsActiveVendor => true;
        public virtual bool IsActiveBuyer => IsActiveVendor; // response to vendor SELL
        public virtual bool IsActiveSeller => IsActiveVendor; // repsonse to vendor BUY

        public virtual NpcGuild NpcGuild => NpcGuild.None;

        public override bool IsInvulnerable => true;

        public virtual DateTime NextTrickOrTreat { get; set; }
        public virtual double GetMoveDelay => Utility.RandomMinMax(30, 120);

        public override bool ShowFameTitle => false;

        public virtual int GetPriceScalar()
        {
            return 100;
        }

        public void UpdateBuyInfo()
        {
            int priceScalar = GetPriceScalar();

            IBuyItemInfo[] buyinfo = (IBuyItemInfo[])m_ArmorBuyInfo.ToArray(typeof(IBuyItemInfo));

            for (int index = 0; index < buyinfo.Length; index++)
            {
                IBuyItemInfo info = buyinfo[index];

                info.PriceScalar = priceScalar;
            }
        }

        public BaseVendor(string title)
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 5)
        {
            AllVendors.Add(this);

            LoadSBInfo();

            Title = title;

            InitBody();
            InitOutfit();

            Container pack;
            //these packs MUST exist, or the client will crash when the packets are sent
            pack = new Backpack
            {
                Layer = Layer.ShopBuy,
                Movable = false,
                Visible = false
            };
            AddItem(pack);

            pack = new Backpack
            {
                Layer = Layer.ShopResale,
                Movable = false,
                Visible = false
            };
            AddItem(pack);

            m_LastRestock = DateTime.UtcNow;
        }

        public BaseVendor(Serial serial)
            : base(serial)
        {
            AllVendors.Add(this);
        }

        public override void OnDelete()
        {
            base.OnDelete();

            AllVendors.Remove(this);
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            AllVendors.Remove(this);
        }

        public DateTime LastRestock { get => m_LastRestock; set => m_LastRestock = value; }

        public virtual TimeSpan RestockDelay => DelayRestock;

        public Container BuyPack
        {
            get
            {
                Container pack = FindItemOnLayer(Layer.ShopBuy) as Container;

                if (pack == null)
                {
                    pack = new Backpack
                    {
                        Layer = Layer.ShopBuy,
                        Visible = false
                    };
                    AddItem(pack);
                }

                return pack;
            }
        }

        public abstract void InitSBInfo();

        protected void LoadSBInfo()
        {
            if (SBInfos == null)
            {
                return;
            }

            m_LastRestock = DateTime.UtcNow;

            for (int i = 0; i < m_ArmorBuyInfo.Count; ++i)
            {
                if (m_ArmorBuyInfo[i] is GenericBuyInfo buy)
                {
                    buy.DeleteDisplayEntity();
                }
            }

            SBInfos.Clear();

            InitSBInfo();

            m_ArmorBuyInfo.Clear();
            m_ArmorSellInfo.Clear();

            for (int i = 0; i < SBInfos.Count; i++)
            {
                SBInfo sbInfo = SBInfos[i];
                m_ArmorBuyInfo.AddRange(sbInfo.BuyInfo);
                m_ArmorSellInfo.Add(sbInfo.SellInfo);
            }
        }

        public virtual bool GetGender()
        {
            return Utility.RandomBool();
        }

        public virtual void InitBody()
        {
            InitStats(100, 100, 25);

            SpeechHue = Utility.RandomDyedHue();
            Hue = Utility.RandomSkinHue();
            Female = GetGender();

            if (Female)
            {
                Body = 0x191;
                Name = NameList.RandomName("female");
            }
            else
            {
                Body = 0x190;
                Name = NameList.RandomName("male");
            }
        }

        public virtual int GetRandomHue()
        {
            switch (Utility.Random(5))
            {
                default:
                case 0:
                {
                    return Utility.RandomBlueHue();
                }
                case 1:
                {
                    return Utility.RandomGreenHue();
                }
                case 2:
                {
                    return Utility.RandomRedHue();
                }
                case 3:
                {
                    return Utility.RandomYellowHue();
                }
                case 4:
                {
                    return Utility.RandomNeutralHue();
                }
            }
        }

        public virtual int GetShoeHue()
        {
            if (0.1 > Utility.RandomDouble())
            {
                return 0;
            }

            return Utility.RandomNeutralHue();
        }

        public virtual VendorShoeType ShoeType => VendorShoeType.Shoes;

        protected override void OnMapChange(Map oldMap)
        {
            base.OnMapChange(oldMap);

            LoadSBInfo();
        }

        public virtual int GetHairHue()
        {
            return Utility.RandomHairHue();
        }

        public virtual void InitOutfit()
        {
            if (Backpack == null)
            {
                Item backpack = new Backpack
                {
                    Movable = false
                };
                AddItem(backpack);
            }

            switch (Utility.Random(3))
            {
                case 0:
                {
                    SetWearable(new FancyShirt(GetRandomHue()));
                    break;
                }
                case 1:
                {
                    SetWearable(new Doublet(GetRandomHue()));
                    break;
                }
                case 2:
                {
                    SetWearable(new Shirt(GetRandomHue()));
                    break;
                }
            }

            switch (ShoeType)
            {
                case VendorShoeType.Shoes:
                {
                    SetWearable(new Shoes(GetShoeHue()));
                    break;
                }
                case VendorShoeType.Boots:
                {
                    SetWearable(new Boots(GetShoeHue()));
                    break;
                }
                case VendorShoeType.Sandals:
                {
                    SetWearable(new Sandals(GetShoeHue()));
                    break;
                }
                case VendorShoeType.ThighBoots:
                {
                    SetWearable(new ThighBoots(GetShoeHue()));
                    break;
                }
            }

            int hairHue = GetHairHue();

            Utility.AssignRandomHair(this, hairHue);
            Utility.AssignRandomFacialHair(this, hairHue);

            if (Body == 0x191)
            {
                FacialHairItemID = 0;
            }

            if (Body == 0x191)
            {
                switch (Utility.Random(6))
                {
                    case 0:
                    {
                        SetWearable(new ShortPants(GetRandomHue()));
                        break;
                    }
                    case 1:
                    case 2:
                    {
                        SetWearable(new Kilt(GetRandomHue()));
                        break;
                    }
                    case 3:
                    case 4:
                    case 5:
                    {
                        SetWearable(new Skirt(GetRandomHue()));
                        break;
                    }
                }
            }
            else
            {
                switch (Utility.Random(2))
                {
                    case 0:
                    {
                        SetWearable(new LongPants(GetRandomHue()));
                        break;
                    }
                    case 1:
                    {
                        SetWearable(new ShortPants(GetRandomHue()));
                        break;
                    }
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ForceRestock
        {
            get => false;
            set
            {
                if (value)
                {
                    Restock();
                    Say("Restocked!");
                }
            }
        }

        public virtual void Restock()
        {
            m_LastRestock = DateTime.UtcNow;

            IBuyItemInfo[] buyInfo = GetBuyInfo();

            for (int index = 0; index < buyInfo.Length; index++)
            {
                IBuyItemInfo bii = buyInfo[index];

                bii.OnRestock();
            }
        }

        private static readonly TimeSpan InventoryDecayTime = TimeSpan.FromHours(1.0);

        public virtual void VendorBuy(Mobile from)
        {
            if (!IsActiveSeller)
            {
                return;
            }

            if (!from.CheckAlive())
            {
                return;
            }

            if (!CheckVendorAccess(from))
            {
                Say(501522); // I shall not treat with scum like thee!
                return;
            }

            if (DateTime.UtcNow - m_LastRestock > RestockDelay)
            {
                Restock();
            }

            UpdateBuyInfo();

            int count = 0;
            List<BuyItemState> list;
            IBuyItemInfo[] buyInfo = GetBuyInfo();
            IShopSellInfo[] sellInfo = GetSellInfo();

            list = new List<BuyItemState>(buyInfo.Length);
            Container cont = BuyPack;

            List<ObjectPropertyListPacket> opls = null;

            for (int idx = 0; idx < buyInfo.Length; idx++)
            {
                IBuyItemInfo buyItem = buyInfo[idx];

                if (buyItem.Amount <= 0 || list.Count >= 250)
                {
                    continue;
                }

                // NOTE: Only GBI supported; if you use another implementation of IBuyItemInfo, this will crash
                GenericBuyInfo gbi = (GenericBuyInfo)buyItem;
                IEntity disp = gbi.GetDisplayEntity();

                list.Add(
                    new BuyItemState(
                        buyItem.Name,
                        cont.Serial,
                        disp == null ? (Serial)0x7FC0FFEE : disp.Serial,
                        buyItem.Price,
                        buyItem.Amount,
                        buyItem.ItemID,
                        buyItem.Hue));
                count++;

                if (opls == null)
                {
                    opls = new List<ObjectPropertyListPacket>();
                }

                if (disp is Item item)
                {
                    opls.Add(item.PropertyList);
                }
                else if (disp is Mobile mobile)
                {
                    opls.Add(mobile.PropertyList);
                }
            }

            List<Item> playerItems = cont.Items;

            for (int i = playerItems.Count - 1; i >= 0; --i)
            {
                if (i >= playerItems.Count)
                {
                    continue;
                }

                Item item = playerItems[i];

                if (item.LastMoved + InventoryDecayTime <= DateTime.UtcNow)
                {
                    item.Delete();
                }
            }

            for (int i = 0; i < playerItems.Count; ++i)
            {
                Item item = playerItems[i];

                int price = 0;
                string name = null;

                for (int index = 0; index < sellInfo.Length; index++)
                {
                    IShopSellInfo ssi = sellInfo[index];

                    if (ssi.IsSellable(item))
                    {
                        price = ssi.GetBuyPriceFor(item, this);
                        name = ssi.GetNameFor(item);
                        break;
                    }
                }

                if (name != null && list.Count < 250)
                {
                    list.Add(new BuyItemState(name, cont.Serial, item.Serial, price, item.Amount, item.ItemID, item.Hue));
                    count++;

                    if (opls == null)
                    {
                        opls = new List<ObjectPropertyListPacket>();
                    }

                    opls.Add(item.PropertyList);
                }
            }

            if (list.Count > 0)
            {
                list.Sort(new BuyItemStateComparer());

                SendPacksTo(from);

                NetState ns = from.NetState;

                if (ns == null)
                {
                    return;
                }

                from.Send(new VendorBuyContentPacket(list));

                from.Send(new VendorBuyListPacket(this, list));

                from.Send(new DisplayBuyListPacket(this));

                from.Send(new MobileStatus(from)); //make sure their gold amount is sent

                if (opls != null)
                {
                    for (int i = 0; i < opls.Count; ++i)
                    {
                        from.Send(opls[i]);
                    }
                }

                SayTo(from, 500186, 0x3B2); // Greetings.  Have a look around.
            }
        }

        public virtual void SendPacksTo(Mobile from)
        {
            Item pack = FindItemOnLayer(Layer.ShopBuy);

            if (pack == null)
            {
                pack = new Backpack
                {
                    Layer = Layer.ShopBuy,
                    Movable = false,
                    Visible = false
                };
                SetWearable(pack);
            }

            from.Send(new EquipUpdate(pack));

            pack = FindItemOnLayer(Layer.ShopSell);

            if (pack != null)
            {
                from.Send(new EquipUpdate(pack));
            }

            pack = FindItemOnLayer(Layer.ShopResale);

            if (pack == null)
            {
                pack = new Backpack
                {
                    Layer = Layer.ShopResale,
                    Movable = false,
                    Visible = false
                };
                SetWearable(pack);
            }

            from.Send(new EquipUpdate(pack));
        }

        public virtual void VendorSell(Mobile from)
        {
            if (!IsActiveBuyer)
            {
                return;
            }

            if (!from.CheckAlive())
            {
                return;
            }

            if (!CheckVendorAccess(from))
            {
                Say(501522); // I shall not treat with scum like thee!
                return;
            }

            Container cont = BuyPack;

            List<Item> packItems = cont.Items;

            for (int i = packItems.Count - 1; i >= 0; --i)
            {
                if (i >= packItems.Count)
                {
                    continue;
                }

                Item item = packItems[i];

                if (item.LastMoved + InventoryDecayTime <= DateTime.UtcNow)
                {
                    item.Delete();
                }
            }

            Container pack = from.Backpack;

            if (pack != null)
            {
                IShopSellInfo[] info = GetSellInfo();

                Dictionary<Item, SellItemState> table = new Dictionary<Item, SellItemState>();

                for (int index = 0; index < info.Length; index++)
                {
                    IShopSellInfo ssi = info[index];

                    Item[] items = pack.FindItemsByType(ssi.Types);

                    for (int i = 0; i < items.Length; i++)
                    {
                        Item item = items[i];

                        if (item is Container && item.Items.Count != 0)
                        {
                            continue;
                        }

                        if (item.ParentEntity is LockableContainer lockable && lockable.Locked)
                        {
                            continue;
                        }

                        if (item.IsStandardLoot() && item.Movable && ssi.IsSellable(item))
                        {
                            table[item] = new SellItemState(item, ssi.GetSellPriceFor(item, this), ssi.GetNameFor(item));
                        }
                    }
                }

                if (table.Count > 0)
                {
                    SendPacksTo(from);

                    from.Send(new VendorSellListPacket(this, table.Values));
                }
                else
                {
                    Say(true, "You have nothing I would be interested in.");
                }
            }
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (AcceptsGift(from, dropped))
            {
                dropped.Delete();
            }

            return base.OnDragDrop(from, dropped);
        }

        public bool AcceptsGift(Mobile from, Item dropped)
        {
            string name;

            if (dropped.Name != null)
            {
                if (dropped.Amount > 0)
                {
                    name = $"{dropped.Amount} {dropped.Name}";
                }
                else
                {
                    name = dropped.Name;
                }
            }
            else
            {
                name = Engines.VendorSearching.VendorSearch.GetItemName(dropped);
            }

            if (!string.IsNullOrEmpty(name))
            {
                PrivateOverheadMessage(MessageType.Regular, 0x3B2, true, $"Thou art giving me {name}.", from.NetState);
            }
            else
            {
                SayTo(from, 1071971, $"#{dropped.LabelNumber}", 0x3B2); // Thou art giving me ~1_VAL~?
            }

            if (dropped is Gold)
            {
                SayTo(from, 501548, 0x3B2); // I thank thee.
                Titles.AwardFame(from, dropped.Amount / 100, true);

                return true;
            }

            IShopSellInfo[] info = GetSellInfo();

            for (int index = 0; index < info.Length; index++)
            {
                IShopSellInfo ssi = info[index];

                if (ssi.IsSellable(dropped))
                {
                    SayTo(from, 501548, 0x3B2); // I thank thee.
                    Titles.AwardFame(from, ssi.GetSellPriceFor(dropped, this) * dropped.Amount, true);

                    return true;
                }
            }

            SayTo(from, 501550, 0x3B2); // I am not interested in this.

            return false;
        }

        private GenericBuyInfo LookupDisplayObject(object obj)
        {
            IBuyItemInfo[] buyInfo = GetBuyInfo();

            for (int i = 0; i < buyInfo.Length; ++i)
            {
                GenericBuyInfo gbi = (GenericBuyInfo)buyInfo[i];

                if (gbi.GetDisplayEntity() == obj)
                {
                    return gbi;
                }
            }

            return null;
        }

        private void ProcessSinglePurchase(
            BuyItemResponse buy,
            IBuyItemInfo bii,
            List<BuyItemResponse> validBuy,
            ref int controlSlots,
            ref bool fullPurchase,
            ref double cost)
        {
            int amount = buy.Amount;

            if (amount > bii.Amount)
            {
                amount = bii.Amount;
            }

            if (amount <= 0)
            {
                return;
            }

            int slots = bii.ControlSlots * amount;

            if (controlSlots >= slots)
            {
                controlSlots -= slots;
            }
            else
            {
                fullPurchase = false;
                return;
            }

            cost = (double)bii.Price * amount;
            validBuy.Add(buy);
        }

        private void ProcessValidPurchase(int amount, IBuyItemInfo bii, Mobile buyer, Container cont)
        {
            if (amount > bii.Amount)
            {
                amount = bii.Amount;
            }

            if (amount < 1)
            {
                return;
            }

            bii.Amount -= amount;

            IEntity o = bii.GetEntity();

            if (o is Item item)
            {
                if (item.Stackable)
                {
                    item.Amount = amount;

                    if (cont == null || !cont.TryDropItem(buyer, item, false))
                    {
                        item.MoveToWorld(buyer.Location, buyer.Map);
                    }
                }
                else
                {
                    item.Amount = 1;

                    if (cont == null || !cont.TryDropItem(buyer, item, false))
                    {
                        item.MoveToWorld(buyer.Location, buyer.Map);
                    }

                    for (int i = 1; i < amount; i++)
                    {
                        item = bii.GetEntity() as Item;

                        if (item != null)
                        {
                            item.Amount = 1;

                            if (cont == null || !cont.TryDropItem(buyer, item, false))
                            {
                                item.MoveToWorld(buyer.Location, buyer.Map);
                            }
                        }
                    }
                }

                bii.OnBought(buyer, this, item, amount);
            }
            else if (o is Mobile m)
            {
                bii.OnBought(buyer, this, m, amount);

                m.Direction = (Direction)Utility.Random(8);
                m.MoveToWorld(buyer.Location, buyer.Map);
                m.PlaySound(m.GetIdleSound());

                if (m is BaseCreature creature)
                {
                    creature.SetControlMaster(buyer);
                }

                for (int i = 1; i < amount; ++i)
                {
                    m = bii.GetEntity() as Mobile;

                    if (m != null)
                    {
                        m.Direction = (Direction)Utility.Random(8);
                        m.MoveToWorld(buyer.Location, buyer.Map);

                        if (m is BaseCreature baseCreature)
                        {
                            baseCreature.SetControlMaster(buyer);
                        }
                    }
                }
            }
        }

        public virtual bool OnBuyItems(Mobile buyer, List<BuyItemResponse> list)
        {
            if (!IsActiveSeller)
            {
                return false;
            }

            if (!buyer.CheckAlive())
            {
                return false;
            }

            if (!CheckVendorAccess(buyer))
            {
                Say(501522); // I shall not treat with scum like thee!
                return false;
            }

            UpdateBuyInfo();

            IShopSellInfo[] info = GetSellInfo();

            double totalCost = 0.0;

            List<BuyItemResponse> validBuy = new List<BuyItemResponse>(list.Count);

            Container cont;

            bool bought = false;
            bool fromBank = false;
            bool fullPurchase = true;

            int controlSlots = buyer.FollowersMax - buyer.Followers;

            for (int index = 0; index < list.Count; index++)
            {
                BuyItemResponse buy = list[index];
                Serial ser = buy.Serial;

                int amount = buy.Amount;

                double cost = 0;

                if (ser.IsItem)
                {
                    Item item = World.FindItem(ser);

                    if (item == null)
                    {
                        continue;
                    }

                    GenericBuyInfo gbi = LookupDisplayObject(item);

                    if (gbi != null)
                    {
                        ProcessSinglePurchase(buy, gbi, validBuy, ref controlSlots, ref fullPurchase, ref cost);
                    }
                    else if (item != BuyPack && item.IsChildOf(BuyPack))
                    {
                        if (amount > item.Amount)
                        {
                            amount = item.Amount;
                        }

                        if (amount <= 0)
                        {
                            continue;
                        }

                        for (int i = 0; i < info.Length; i++)
                        {
                            IShopSellInfo ssi = info[i];

                            if (ssi.IsSellable(item))
                            {
                                if (ssi.IsResellable(item))
                                {
                                    cost = (double) ssi.GetBuyPriceFor(item, this) * amount;
                                    validBuy.Add(buy);
                                    break;
                                }
                            }
                        }
                    }

                    if (validBuy.Contains(buy))
                    {
                        if (ValidateBought(buyer, item))
                        {
                            totalCost += cost;
                        }
                        else
                        {
                            validBuy.Remove(buy);
                        }
                    }
                }
                else if (ser.IsMobile)
                {
                    Mobile mob = World.FindMobile(ser);

                    if (mob == null)
                    {
                        continue;
                    }

                    GenericBuyInfo gbi = LookupDisplayObject(mob);

                    if (gbi != null)
                    {
                        ProcessSinglePurchase(buy, gbi, validBuy, ref controlSlots, ref fullPurchase, ref cost);
                    }

                    if (validBuy.Contains(buy))
                    {
                        if (ValidateBought(buyer, mob))
                        {
                            totalCost += cost;
                        }
                        else
                        {
                            validBuy.Remove(buy);
                        }
                    }
                }
            }

            if (fullPurchase && validBuy.Count == 0)
            {
                SayTo(buyer, 500190, 0x3B2); // Thou hast bought nothing!
            }
            else if (validBuy.Count == 0)
            {
                SayTo(buyer, 500187, 0x3B2); // Your order cannot be fulfilled, please try again.
            }

            if (validBuy.Count == 0)
            {
                return false;
            }

            bought = buyer.AccessLevel >= AccessLevel.GameMaster;
            cont = buyer.Backpack;

            if (!bought && cont != null && ConsumeGold(cont, totalCost))
            {
                bought = true;
            }

            if (!bought)
            {
                if (totalCost <= int.MaxValue)
                {
                    if (Banker.Withdraw(buyer, (int)totalCost))
                    {
                        bought = true;
                        fromBank = true;
                    }
                }
                else if (buyer.Account != null && buyer.Account.WithdrawCurrency(totalCost / AccountGold.CurrencyThreshold))
                {
                    bought = true;
                    fromBank = true;
                }
            }

            if (!bought)
            {
                cont = buyer.FindBankNoCreate();

                if (cont != null && ConsumeGold(cont, totalCost))
                {
                    bought = true;
                    fromBank = true;
                }
            }

            if (!bought)
            {
                // ? Begging thy pardon, but thy bank account lacks these funds. 
                // : Begging thy pardon, but thou casnt afford that.
                SayTo(buyer, totalCost >= 2000 ? 500191 : 500192, 0x3B2);

                return false;
            }

            buyer.PlaySound(0x32);

            cont = buyer.Backpack ?? buyer.BankBox;

            for (int index = 0; index < validBuy.Count; index++)
            {
                BuyItemResponse buy = validBuy[index];
                Serial ser = buy.Serial;

                int amount = buy.Amount;

                if (amount < 1)
                {
                    continue;
                }

                if (ser.IsItem)
                {
                    Item item = World.FindItem(ser);

                    if (item == null)
                    {
                        continue;
                    }

                    GenericBuyInfo gbi = LookupDisplayObject(item);

                    if (gbi != null)
                    {
                        ProcessValidPurchase(amount, gbi, buyer, cont);
                    }
                    else
                    {
                        if (amount > item.Amount)
                        {
                            amount = item.Amount;
                        }

                        for (int i = 0; i < info.Length; i++)
                        {
                            IShopSellInfo ssi = info[i];

                            if (ssi.IsSellable(item))
                            {
                                if (ssi.IsResellable(item))
                                {
                                    Item buyItem;

                                    if (amount >= item.Amount)
                                    {
                                        buyItem = item;
                                    }
                                    else
                                    {
                                        buyItem = LiftItemDupe(item, item.Amount - amount);

                                        if (buyItem == null)
                                        {
                                            buyItem = item;
                                        }
                                    }

                                    if (cont == null || !cont.TryDropItem(buyer, buyItem, false))
                                    {
                                        buyItem.MoveToWorld(buyer.Location, buyer.Map);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
                else if (ser.IsMobile)
                {
                    Mobile mob = World.FindMobile(ser);

                    if (mob == null)
                    {
                        continue;
                    }

                    GenericBuyInfo gbi = LookupDisplayObject(mob);

                    if (gbi != null)
                    {
                        ProcessValidPurchase(amount, gbi, buyer, cont);
                    }
                }
            }

            if (fullPurchase)
            {
                if (buyer.AccessLevel >= AccessLevel.GameMaster)
                {
                    SayTo(
                        buyer,
                        0x3B2,
                        "I would not presume to charge thee anything.  Here are the goods you requested.",
                        null,
                        true);
                }
                else if (fromBank)
                {
                    SayTo(
                        buyer,
                        0x3B2,
                        "The total of thy purchase is {0} gold, which has been withdrawn from your bank account.  My thanks for the patronage.",
                        totalCost.ToString(),
                        true);
                }
                else
                {
                    SayTo(buyer, $"The total of thy purchase is {totalCost} gold.  My thanks for the patronage.", 0x3B2, true);
                }
            }
            else
            {
                if (buyer.AccessLevel >= AccessLevel.GameMaster)
                {
                    SayTo(
                        buyer,
                        0x3B2,
                        "I would not presume to charge thee anything.  Unfortunately, I could not sell you all the goods you requested.",
                        null,
                        true);
                }
                else if (fromBank)
                {
                    SayTo(
                        buyer,
                        0x3B2,
                        "The total of thy purchase is {0} gold, which has been withdrawn from your bank account.  My thanks for the patronage.  Unfortunately, I could not sell you all the goods you requested.",
                        totalCost.ToString(),
                        true);
                }
                else
                {
                    SayTo(
                        buyer,
                        0x3B2,
                        "The total of thy purchase is {0} gold.  My thanks for the patronage.  Unfortunately, I could not sell you all the goods you requested.",
                        totalCost.ToString(),
                        true);
                }
            }

            return true;
        }

        public virtual bool ValidateBought(Mobile buyer, Item item)
        {
            return true;
        }

        public virtual bool ValidateBought(Mobile buyer, Mobile m)
        {
            return true;
        }

        public static bool ConsumeGold(Container cont, double amount)
        {
            return ConsumeGold(cont, amount, true);
        }

        public static bool ConsumeGold(Container cont, double amount, bool recurse)
        {
            Queue<Gold> gold = new Queue<Gold>(FindGold(cont, recurse));

            double total = 0.0;

            foreach (Gold gold1 in gold)
            {
                total = total + gold1.Amount;
            }

            if (total < amount)
            {
                gold.Clear();

                return false;
            }

            double consume = amount;

            while (consume > 0)
            {
                Gold g = gold.Dequeue();

                if (g.Amount > consume)
                {
                    g.Consume((int)consume);

                    consume = 0;
                }
                else
                {
                    consume -= g.Amount;

                    g.Delete();
                }
            }

            gold.Clear();

            return true;
        }

        private static IEnumerable<Gold> FindGold(Container cont, bool recurse)
        {
            if (cont == null || cont.Items.Count == 0)
            {
                yield break;
            }

            if (cont is ILockable lockable && lockable.Locked)
            {
                yield break;
            }

            if (cont is TrapableContainer container && container.TrapType != TrapType.None)
            {
                yield break;
            }

            int count = cont.Items.Count;

            while (--count >= 0)
            {
                if (count >= cont.Items.Count)
                {
                    continue;
                }

                Item item = cont.Items[count];

                if (item is Container containerItem)
                {
                    if (!recurse)
                    {
                        continue;
                    }

                    foreach (Gold gold in FindGold(containerItem, true))
                    {
                        yield return gold;
                    }
                }
                else if (item is Gold gold)
                {
                    yield return gold;
                }
            }
        }

        public virtual bool CheckVendorAccess(Mobile from)
        {
            GuardedRegion reg = (GuardedRegion)Region.GetRegion(typeof(GuardedRegion));

            if (reg != null && !reg.CheckVendorAccess(this, from))
            {
                return false;
            }

            if (Region != from.Region)
            {
                reg = (GuardedRegion)from.Region.GetRegion(typeof(GuardedRegion));

                if (reg != null && !reg.CheckVendorAccess(this, from))
                {
                    return false;
                }
            }

            return true;
        }

        public virtual bool OnSellItems(Mobile seller, List<SellItemResponse> list)
        {
            if (!IsActiveBuyer)
            {
                return false;
            }

            if (!seller.CheckAlive())
            {
                return false;
            }

            if (!CheckVendorAccess(seller))
            {
                Say(501522); // I shall not treat with scum like thee!
                return false;
            }

            seller.PlaySound(0x32);

            IShopSellInfo[] info = GetSellInfo();
            IBuyItemInfo[] buyInfo = GetBuyInfo();
            int GiveGold = 0;
            int Sold = 0;
            Container cont;

            for (int index = 0; index < list.Count; index++)
            {
                SellItemResponse resp = list[index];

                if (resp.Item.RootParent != seller || resp.Amount <= 0 || !resp.Item.IsStandardLoot() || !resp.Item.Movable || resp.Item is Container && resp.Item.Items.Count != 0)
                {
                    continue;
                }

                for (int i = 0; i < info.Length; i++)
                {
                    IShopSellInfo ssi = info[i];

                    if (ssi.IsSellable(resp.Item))
                    {
                        Sold++;
                        break;
                    }
                }
            }

            if (Sold > MaxSell)
            {
                SayTo(seller, "You may only sell {0} items at a time!", MaxSell, 0x3B2, true);
                return false;
            }

            if (Sold == 0)
            {
                return true;
            }

            for (int index = 0; index < list.Count; index++)
            {
                SellItemResponse resp = list[index];

                if (resp.Item.RootParent != seller || resp.Amount <= 0 || !resp.Item.IsStandardLoot() || !resp.Item.Movable || resp.Item is Container && resp.Item.Items.Count != 0)
                {
                    continue;
                }

                for (int i = 0; i < info.Length; i++)
                {
                    IShopSellInfo ssi = info[i];

                    if (ssi.IsSellable(resp.Item))
                    {
                        int amount = resp.Amount;

                        if (amount > resp.Item.Amount)
                        {
                            amount = resp.Item.Amount;
                        }

                        if (ssi.IsResellable(resp.Item))
                        {
                            bool found = false;

                            for (int index1 = 0; index1 < buyInfo.Length; index1++)
                            {
                                IBuyItemInfo bii = buyInfo[index1];

                                if (bii.Restock(resp.Item, amount))
                                {
                                    bii.OnSold(this, amount);

                                    resp.Item.Consume(amount);
                                    found = true;

                                    break;
                                }
                            }

                            if (!found)
                            {
                                cont = BuyPack;

                                if (amount < resp.Item.Amount)
                                {
                                    Item item = LiftItemDupe(resp.Item, resp.Item.Amount - amount);

                                    if (item != null)
                                    {
                                        item.SetLastMoved();
                                        cont.DropItem(item);
                                    }
                                    else
                                    {
                                        resp.Item.SetLastMoved();
                                        cont.DropItem(resp.Item);
                                    }
                                }
                                else
                                {
                                    resp.Item.SetLastMoved();
                                    cont.DropItem(resp.Item);
                                }
                            }
                        }
                        else
                        {
                            if (amount < resp.Item.Amount)
                            {
                                resp.Item.Amount -= amount;
                            }
                            else
                            {
                                resp.Item.Delete();
                            }
                        }

                        int singlePrice = ssi.GetSellPriceFor(resp.Item, this);
                        GiveGold += singlePrice * amount;

                        break;
                    }
                }
            }

            if (GiveGold > 0)
            {
                while (GiveGold > 60000)
                {
                    seller.AddToBackpack(new Gold(60000));
                    GiveGold -= 60000;
                }

                seller.AddToBackpack(new Gold(GiveGold));

                seller.PlaySound(0x0037); //Gold dropping sound
            }

            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(4); // version

            List<SBInfo> sbInfos = SBInfos;

            for (int i = 0; sbInfos != null && i < sbInfos.Count; ++i)
            {
                SBInfo sbInfo = sbInfos[i];
                List<GenericBuyInfo> buyInfo = sbInfo.BuyInfo;

                for (int j = 0; buyInfo != null && j < buyInfo.Count; ++j)
                {
                    GenericBuyInfo gbi = buyInfo[j];

                    int maxAmount = gbi.MaxAmount;
                    int doubled = 0;

                    switch (maxAmount)
                    {
                        case 40:
                        {
                            doubled = 1;
                            break;
                        }
                        case 80:
                        {
                            doubled = 2;
                            break;
                        }
                        case 160:
                        {
                            doubled = 3;
                            break;
                        }
                        case 320:
                        {
                            doubled = 4;
                            break;
                        }
                        case 640:
                        {
                            doubled = 5;
                            break;
                        }
                        case 999:
                        {
                            doubled = 6;
                            break;
                        }
                    }

                    if (doubled > 0)
                    {
                        writer.WriteEncodedInt(1 + j * sbInfos.Count + i);
                        writer.WriteEncodedInt(doubled);
                    }
                }
            }

            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            LoadSBInfo();

            List<SBInfo> sbInfos = SBInfos;

            switch (version)
            {
                case 4:
                case 3:
                case 2:
                case 1:
                    {
                        int index;

                        while ((index = reader.ReadEncodedInt()) > 0)
                        {
                            int doubled = reader.ReadEncodedInt();

                            if (sbInfos != null)
                            {
                                index -= 1;
                                int sbInfoIndex = index % sbInfos.Count;
                                int buyInfoIndex = index / sbInfos.Count;

                                if (sbInfoIndex >= 0 && sbInfoIndex < sbInfos.Count)
                                {
                                    SBInfo sbInfo = sbInfos[sbInfoIndex];
                                    List<GenericBuyInfo> buyInfo = sbInfo.BuyInfo;

                                    if (buyInfo != null && buyInfoIndex >= 0 && buyInfoIndex < buyInfo.Count)
                                    {
                                        GenericBuyInfo gbi = buyInfo[buyInfoIndex];

                                        int amount = 20;

                                        switch (doubled)
                                        {
                                            case 0:
                                            {
                                                break;
                                            }
                                            case 1:
                                            {
                                                amount = 40;
                                                break;
                                            }
                                            case 2:
                                            {
                                                amount = 80;
                                                break;
                                            }
                                            case 3:
                                            {
                                                amount = 160;
                                                break;
                                            }
                                            case 4:
                                            {
                                                amount = 320;
                                                break;
                                            }
                                            case 5:
                                            {
                                                amount = 640;
                                                break;
                                            }
                                            case 6:
                                            {
                                                amount = 999;
                                                break;
                                            }
                                        }

                                        if (version == 2 && gbi.Stackable)
                                        {
                                            gbi.Amount = gbi.MaxAmount = EconomyStockAmount;
                                        }
                                        else
                                        {
                                            gbi.Amount = gbi.MaxAmount = amount;
                                        }

                                        gbi.TotalBought = 0;
                                        gbi.TotalSold = 0;
                                    }
                                }
                            }
                        }

                        break;
                    }
            }
        }

        public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
        {
            if (from.Alive && IsActiveVendor)
            {
                if (IsActiveSeller)
                {
                    list.Add(new VendorBuyEntry(from, this));
                }

                if (IsActiveBuyer)
                {
                    list.Add(new VendorSellEntry(from, this));
                }
            }

            base.AddCustomContextEntries(from, list);
        }

        public virtual IShopSellInfo[] GetSellInfo()
        {
            return (IShopSellInfo[])m_ArmorSellInfo.ToArray(typeof(IShopSellInfo));
        }

        public virtual IBuyItemInfo[] GetBuyInfo()
        {
            return (IBuyItemInfo[])m_ArmorBuyInfo.ToArray(typeof(IBuyItemInfo));
        }
    }
}

namespace Server.ContextMenus
{
    public class VendorBuyEntry : ContextMenuEntry
    {
        private readonly BaseVendor m_Vendor;

        public VendorBuyEntry(Mobile from, BaseVendor vendor)
            : base(6103, 8)
        {
            m_Vendor = vendor;
            Enabled = vendor.CheckVendorAccess(from);
        }

        public override void OnClick()
        {
            m_Vendor.VendorBuy(Owner.From);
        }
    }

    public class VendorSellEntry : ContextMenuEntry
    {
        private readonly BaseVendor m_Vendor;

        public VendorSellEntry(Mobile from, BaseVendor vendor)
            : base(6104, 8)
        {
            m_Vendor = vendor;
            Enabled = vendor.CheckVendorAccess(from);
        }

        public override void OnClick()
        {
            m_Vendor.VendorSell(Owner.From);
        }
    }
}

namespace Server
{
    public interface IShopSellInfo
    {
        //get display name for an item
        string GetNameFor(Item item);

        //get price for an item which the player is selling
        int GetSellPriceFor(Item item);
        int GetSellPriceFor(Item item, BaseVendor vendor);

        //get price for an item which the player is buying
        int GetBuyPriceFor(Item item);
        int GetBuyPriceFor(Item item, BaseVendor vendor);

        //can we sell this item to this vendor?
        bool IsSellable(Item item);

        //What do we sell?
        Type[] Types { get; }

        //does the vendor resell this item?
        bool IsResellable(Item item);
    }

    public interface IBuyItemInfo
    {
        //get a new instance of an object (we just bought it)
        IEntity GetEntity();

        int ControlSlots { get; }

        int PriceScalar { get; set; }

        bool Stackable { get; set; }
        int TotalBought { get; set; }
        int TotalSold { get; set; }

        void OnBought(Mobile buyer, BaseVendor vendor, IEntity entity, int amount);
        void OnSold(BaseVendor vendor, int amount);

        //display price of the item
        int Price { get; }

        //display name of the item
        string Name { get; }

        //display hue
        int Hue { get; }

        //display id
        int ItemID { get; }

        //amount in stock
        int Amount { get; set; }

        //max amount in stock
        int MaxAmount { get; }

        //Attempt to restock with item, (return true if restock sucessful)
        bool Restock(Item item, int amount);

        //called when its time for the whole shop to restock
        void OnRestock();
    }
}
