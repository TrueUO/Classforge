using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.VvV
{
    public class SilverTrader : BaseVendor
    {
        public override bool IsActiveVendor => false;
        public override bool DisallowAllMoves => true;
        public override bool ClickTitle => true;

        protected List<SBInfo> m_SBInfos = new List<SBInfo>();
        protected override List<SBInfo> SBInfos => m_SBInfos;
        public override void InitSBInfo() { }

        [Constructable]
        public SilverTrader() : base("the Silver Trader")
        {
        }

        public override void InitBody()
        {
            base.InitBody();

            Name = NameList.RandomName("male");

            SpeechHue = 0x3B2;
            Hue = Utility.RandomSkinHue();
            Body = 0x190;
        }

        public override void InitOutfit()
        {
            Robe robe = new Robe
            {
                ItemID = 0x2684,
                Name = "a robe"
            };

            SetWearable(robe, 1109);

            Timer.DelayCall(TimeSpan.FromSeconds(10), StockInventory);
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add(1155513); // Vice vs Virtue Reward Vendor
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (ViceVsVirtueSystem.Enabled && m is PlayerMobile pm && InRange(pm.Location, 3))
            {
                if (ViceVsVirtueSystem.IsVvV(pm))
                {
                    pm.SendGump(new VvVRewardGump(this, pm));
                }
                else
                {
                    SayTo(pm, 1155585); // You have no silver to trade with. Join Vice vs Virtue and return to me.
                }
            }
        }

        public void StockInventory()
        {
            if (Backpack == null)
            {
                AddItem(new Backpack());
            }

            for (int index = 0; index < VvVRewards.Rewards.Count; index++)
            {
                CollectionItem item = VvVRewards.Rewards[index];

                if (item.Tooltip == 0)
                {
                    if (Backpack.GetAmount(item.Type) > 0)
                    {
                        Item itm = Backpack.FindItemByType(item.Type);

                        if (itm is IVvVItem vItem)
                        {
                            vItem.IsVvVItem = true;
                        }

                        continue;
                    }

                    if (Activator.CreateInstance(item.Type) is Item i)
                    {
                        if (i is IOwnerRestricted restricted)
                        {
                            restricted.OwnerName = "Your Player Name";
                        }

                        if (i is IVvVItem vItem)
                        {
                            vItem.IsVvVItem = true;
                        }

                        NegativeAttributes neg = RunicReforging.GetNegativeAttributes(i);

                        if (neg != null)
                        {
                            neg.Antique = 1;

                            if (i is IDurability durability && durability.MaxHitPoints == 0)
                            {
                                durability.MaxHitPoints = 255;
                                durability.HitPoints = 255;
                            }
                        }

                        ViceVsVirtueSystem.Instance.AddVvVItem(i, true);

                        Backpack.DropItem(i);
                    }
                }
            }
        }

        public SilverTrader(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            Timer.DelayCall(TimeSpan.FromSeconds(5), StockInventory);
        }
    }
}
