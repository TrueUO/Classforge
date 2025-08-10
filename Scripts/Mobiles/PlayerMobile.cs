using Server.Accounting;
using Server.Commands;
using Server.ContextMenus;
using Server.Engines.ArenaSystem;
using Server.Engines.CannedEvil;
using Server.Engines.Chat;
using Server.Engines.CityLoyalty;
using Server.Engines.Craft;
using Server.Engines.Doom;
using Server.Engines.Events;
using Server.Engines.InstancedPeerless;
using Server.Engines.NewMagincia;
using Server.Engines.PartySystem;
using Server.Engines.Plants;
using Server.Engines.Points;
using Server.Engines.Quests;
using Server.Engines.Shadowguard;
using Server.Engines.SphynxFortune;
using Server.Engines.VendorSearching;
using Server.Engines.VoidPool;
using Server.Engines.VvV;
using Server.Events.Halloween;
using Server.Guilds;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Multis;
using Server.Network;
using Server.Network.Packets;
using Server.Regions;
using Server.Services.ClassSystem;
using Server.Services.TownCryer;
using Server.SkillHandlers;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Fifth;
using Server.Spells.First;
using Server.Spells.Fourth;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;
using Server.Spells.Sixth;
using Server.Spells.SkillMasteries;
using Server.Spells.Spellweaving;
using System;
using System.Collections.Generic;
using Aggression = Server.Misc.Aggression;
using RankDefinition = Server.Guilds.RankDefinition;

namespace Server.Mobiles
{
    [Flags]
    public enum PlayerFlag
    {
        None = 0x00000000,
        Glassblowing = 0x00000001,
        Masonry = 0x00000002,
        SandMining = 0x00000004,
        StoneMining = 0x00000008,
        ToggleMiningStone = 0x00000010,
        KarmaLocked = 0x00000020,
        UseOwnFilter = 0x00000040,
        AcceptGuildInvites = 0x00000080,
        DisplayChampionTitle = 0x00000100,
        HasStatReward = 0x00000200,
        GemMining = 0x00000400,
        ToggleMiningGem = 0x00000800,
        AbyssEntry = 0x00001000,
        ToggleStoneOnly = 0x00002000,
        DisabledPvpWarning = 0x00004000,
        ToggleClippings = 0x00008000,
        ToggleCutClippings = 0x00010000,
        ToggleCutReeds = 0x00020000,
        ToggleCutTopiaries = 0x00040000,
        HasValiantStatReward = 0x00080000,
        RefuseTrades = 0x00100000
    }

    public enum NpcGuild
    {
        None,
        MagesGuild,
        WarriorsGuild,
        ThievesGuild,
        RangersGuild,
        HealersGuild,
        MerchantsGuild,
        BardsGuild
    }

    public partial class PlayerMobile : Mobile
    {
        public static List<PlayerMobile> Instances { get; }

        static PlayerMobile()
        {
            Instances = new List<PlayerMobile>(0x1000);
        }

        public void SetMountBlock(BlockMountType type, TimeSpan duration, bool dismount)
        {
            if (dismount)
            {
                BaseMount.Dismount(this, this, type, duration, false);
            }
            else
            {
                BaseMount.SetMountPrevention(this, type, duration);
            }
        }

        private DesignContext m_DesignContext;

        private NpcGuild m_NpcGuild;
        private DateTime m_NpcGuildJoinTime;
        private TimeSpan m_NpcGuildGameTime;
        private PlayerFlag m_Flags;

        /*
		* a value of zero means, that the mobile is not executing the spell. Otherwise,
		* the value should match the BaseMana required
		*/
        private int m_ExecutesLightningStrike; // move to Server.Mobiles??

        private DateTime m_LastOnline;
        private RankDefinition m_GuildRank;
        private bool m_NextEnhanceSuccess;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool NextEnhanceSuccess { get => m_NextEnhanceSuccess; set => m_NextEnhanceSuccess = value; }

        private int m_GuildMessageHue, m_AllianceMessageHue;

        private List<Mobile> m_AutoStabled;
        private List<Mobile> m_AllFollowers;
        private List<Mobile> m_RecentlyReported;

        public bool UseSummoningRite { get; set; }

        private PointsSystemProps _PointsSystemProps;
        private AccountGoldProps _AccountGold;

        #region Custom
        // Overwrites the classic Skills button on the paperdoll and sends our new menu.
        // LoginStats.cs sends the skill packet once on login so it can update correctly.
        // Since we are overriding it, it won't query skills at first login per normal without this.
        public override void OnSkillsQuery(Mobile from)
        {
            if (from == this)
            {
                // on log-in skills are queried. This stops the gump from auto appearing.
                if (m_SessionStart + TimeSpan.FromSeconds(2.0) > DateTime.UtcNow)
                {
                    return;
                }

                if (from is PlayerMobile pm && pm.CharacterClass != CharacterClass.None)
                {
                    from.CloseGump(typeof(CustomSkillGump));
                    from.SendGump(new CustomSkillGump(pm));
                }
                else
                {
                    from.SendMessage("You must select a class before you can access the skills menu.");
                }
            }
        }

        private int _Xp;
        private int _Level;
        private int _SkillPointsToSpend;
        private int _StatPointsToSpend;

        private CharacterClass _CharacterClass = CharacterClass.None;

        [CommandProperty(AccessLevel.GameMaster)]
        public CharacterClass CharacterClass
        {
            get => _CharacterClass;
            set
            {
                _CharacterClass = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Xp
        {
            get => _Xp;
            set => _Xp = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Level
        {
            get => _Level;
            set
            {
                _Level = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SkillPointsToSpend
        {
            get => _SkillPointsToSpend;
            set => _SkillPointsToSpend = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int StatPointsToSpend
        {
            get => _StatPointsToSpend;
            set => _StatPointsToSpend = value;
        }
        #endregion

        [CommandProperty(AccessLevel.GameMaster)]
        public PointsSystemProps PointSystems
        {
            get
            {
                if (_PointsSystemProps == null)
                {
                    _PointsSystemProps = new PointsSystemProps(this);
                }

                return _PointsSystemProps;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AccountGoldProps AccountGold
        {
            get
            {
                if (_AccountGold == null)
                {
                    _AccountGold = new AccountGoldProps(this);
                }

                return _AccountGold;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AccountSovereigns
        {
            get
            {
                if (Account is Account acct)
                {
                    return acct.Sovereigns;
                }

                return 0;
            }
            set
            {
                Account acct = Account as Account;

                acct?.SetSovereigns(value);
            }
        }

        public bool DepositSovereigns(int amount)
        {
            if (Account is Account acct)
            {
                return acct.DepositSovereigns(amount);
            }

            return false;
        }

        public bool WithdrawSovereigns(int amount)
        {
            if (Account is Account acct)
            {
                return acct.WithdrawSovereigns(amount);
            }

            return false;
        }

        public List<Mobile> RecentlyReported { get => m_RecentlyReported; set => m_RecentlyReported = value; }

        public List<Mobile> AutoStabled => m_AutoStabled;

        public bool NinjaWepCooldown { get; set; }

        public List<Mobile> AllFollowers
        {
            get
            {
                if (m_AllFollowers == null)
                {
                    m_AllFollowers = new List<Mobile>();
                }

                return m_AllFollowers;
            }
        }

        [CommandProperty(AccessLevel.GameMaster, true)]
        public RankDefinition GuildRank
        {
            get
            {
                if (AccessLevel >= AccessLevel.GameMaster)
                {
                    return RankDefinition.Leader;
                }

                return m_GuildRank;
            }
            set => m_GuildRank = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GuildMessageHue { get => m_GuildMessageHue; set => m_GuildMessageHue = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AllianceMessageHue { get => m_AllianceMessageHue; set => m_AllianceMessageHue = value; }

        public int StepsTaken { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public NpcGuild NpcGuild { get => m_NpcGuild; set => m_NpcGuild = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NpcGuildJoinTime { get => m_NpcGuildJoinTime; set => m_NpcGuildJoinTime = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextBODTurnInTime { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastOnline { get => m_LastOnline; set => m_LastOnline = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NpcGuildGameTime { get => m_NpcGuildGameTime; set => m_NpcGuildGameTime = value; }

        public int ExecutesLightningStrike { get => m_ExecutesLightningStrike; set => m_ExecutesLightningStrike = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ToothAche { get => BaseSweet.GetToothAche(this); set => BaseSweet.SetToothAche(this, value, true); }

        public PlayerFlag Flags { get => m_Flags; set => m_Flags = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Glassblowing { get => GetFlag(PlayerFlag.Glassblowing); set => SetFlag(PlayerFlag.Glassblowing, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Masonry { get => GetFlag(PlayerFlag.Masonry); set => SetFlag(PlayerFlag.Masonry, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool SandMining { get => GetFlag(PlayerFlag.SandMining); set => SetFlag(PlayerFlag.SandMining, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool StoneMining { get => GetFlag(PlayerFlag.StoneMining); set => SetFlag(PlayerFlag.StoneMining, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool GemMining { get => GetFlag(PlayerFlag.GemMining); set => SetFlag(PlayerFlag.GemMining, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleMiningStone { get => GetFlag(PlayerFlag.ToggleMiningStone); set => SetFlag(PlayerFlag.ToggleMiningStone, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleStoneOnly { get => GetFlag(PlayerFlag.ToggleStoneOnly); set => SetFlag(PlayerFlag.ToggleStoneOnly, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DisabledPvpWarning { get => GetFlag(PlayerFlag.DisabledPvpWarning); set => SetFlag(PlayerFlag.DisabledPvpWarning, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AbyssEntry { get => GetFlag(PlayerFlag.AbyssEntry); set => SetFlag(PlayerFlag.AbyssEntry, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleMiningGem { get => GetFlag(PlayerFlag.ToggleMiningGem); set => SetFlag(PlayerFlag.ToggleMiningGem, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool KarmaLocked { get => GetFlag(PlayerFlag.KarmaLocked); set => SetFlag(PlayerFlag.KarmaLocked, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool UseOwnFilter { get => GetFlag(PlayerFlag.UseOwnFilter); set => SetFlag(PlayerFlag.UseOwnFilter, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AcceptGuildInvites { get => GetFlag(PlayerFlag.AcceptGuildInvites); set => SetFlag(PlayerFlag.AcceptGuildInvites, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool HasStatReward { get => GetFlag(PlayerFlag.HasStatReward); set => SetFlag(PlayerFlag.HasStatReward, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool HasValiantStatReward { get => GetFlag(PlayerFlag.HasValiantStatReward); set => SetFlag(PlayerFlag.HasValiantStatReward, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RefuseTrades
        {
            get => GetFlag(PlayerFlag.RefuseTrades);
            set => SetFlag(PlayerFlag.RefuseTrades, value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleClippings { get => GetFlag(PlayerFlag.ToggleClippings); set => SetFlag(PlayerFlag.ToggleClippings, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleCutReeds { get => GetFlag(PlayerFlag.ToggleCutReeds); set => SetFlag(PlayerFlag.ToggleCutReeds, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleCutClippings { get => GetFlag(PlayerFlag.ToggleCutClippings); set => SetFlag(PlayerFlag.ToggleCutClippings, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleCutTopiaries { get => GetFlag(PlayerFlag.ToggleCutTopiaries); set => SetFlag(PlayerFlag.ToggleCutTopiaries, value); }

        private DateTime m_SSNextSeed;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime SSNextSeed { get => m_SSNextSeed; set => m_SSNextSeed = value; }

        private DateTime m_SSSeedExpire;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime SSSeedExpire { get => m_SSSeedExpire; set => m_SSSeedExpire = value; }

        private Point3D m_SSSeedLocation;

        public Point3D SSSeedLocation { get => m_SSSeedLocation; set => m_SSSeedLocation = value; }

        private Map m_SSSeedMap;

        public Map SSSeedMap { get => m_SSSeedMap; set => m_SSSeedMap = value; }

        private readonly Dictionary<Type, int> m_RecoverableAmmo = new Dictionary<Type, int>();

        public Dictionary<Type, int> RecoverableAmmo => m_RecoverableAmmo;

        public void RecoverAmmo()
        {
            if (Alive)
            {
                foreach (KeyValuePair<Type, int> kvp in m_RecoverableAmmo)
                {
                    if (kvp.Value > 0)
                    {
                        Item ammo = null;

                        try
                        {
                            ammo = Activator.CreateInstance(kvp.Key) as Item;
                        }
                        catch (Exception e)
                        {
                            Diagnostics.ExceptionLogging.LogException(e);
                        }

                        if (ammo != null)
                        {
                            string name = ammo.Name;
                            ammo.Amount = kvp.Value;

                            if (name == null)
                            {
                                if (ammo is Arrow)
                                {
                                    name = "arrow";
                                }
                                else if (ammo is Bolt)
                                {
                                    name = "bolt";
                                }
                            }

                            if (name != null && ammo.Amount > 1)
                            {
                                name = $"{name}s";
                            }

                            if (name == null)
                            {
                                name = $"#{ammo.LabelNumber}";
                            }

                            PlaceInBackpack(ammo);
                            SendLocalizedMessage(1073504, $"{ammo.Amount}\t{name}"); // You recover ~1_NUM~ ~2_AMMO~.
                        }
                    }
                }

                m_RecoverableAmmo.Clear();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RewardStableSlots { get; set; }

        private DateTime m_AnkhNextUse;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime AnkhNextUse { get => m_AnkhNextUse; set => m_AnkhNextUse = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextGemOfSalvationUse { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan DisguiseTimeLeft => DisguiseTimers.TimeRemaining(this);

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime PeacedUntil { get; set; }

        [CommandProperty(AccessLevel.Decorator)]
        public override string TitleName
        {
            get
            {
                string name;

                name = Fame >= 10000 ? $"{(Female ? "Lady" : "Lord")} {RawName}" : RawName;

                return name;
            }
        }

        public static Direction GetDirection4(Point3D from, Point3D to)
        {
            int dx = from.X - to.X;
            int dy = from.Y - to.Y;

            int rx = dx - dy;
            int ry = dx + dy;

            Direction ret;

            if (rx >= 0 && ry >= 0)
            {
                ret = Direction.West;
            }
            else if (rx >= 0 && ry < 0)
            {
                ret = Direction.South;
            }
            else if (rx < 0 && ry < 0)
            {
                ret = Direction.East;
            }
            else
            {
                ret = Direction.North;
            }

            return ret;
        }

        public override bool OnDroppedItemToWorld(Item item, Point3D location)
        {
            if (!base.OnDroppedItemToWorld(item, location))
            {
                return false;
            }

            IPooledEnumerable mobiles = Map.GetMobilesInRange(location, 0);

            foreach (Mobile m in mobiles)
            {
                if (m.Z >= location.Z && m.Z < location.Z + 16)
                {
                    mobiles.Free();
                    return false;
                }
            }

            mobiles.Free();

            BounceInfo bi = item.GetBounce();

            if (bi != null && AccessLevel > AccessLevel.Counselor)
            {
                Type type = item.GetType();

                if (type.IsDefined(typeof(FurnitureAttribute), true) || type.IsDefined(typeof(DynamicFlipingAttribute), true))
                {
                    object[] objs = type.GetCustomAttributes(typeof(FlipableAttribute), true);

                    if (objs.Length > 0)
                    {
                        if (objs[0] is FlipableAttribute fp)
                        {
                            int[] itemIDs = fp.ItemIDs;

                            Point3D oldWorldLoc = bi.m_WorldLoc;
                            Point3D newWorldLoc = location;

                            if (oldWorldLoc.X != newWorldLoc.X || oldWorldLoc.Y != newWorldLoc.Y)
                            {
                                Direction dir = GetDirection4(oldWorldLoc, newWorldLoc);

                                if (itemIDs.Length == 2)
                                {
                                    switch (dir)
                                    {
                                        case Direction.North:
                                        case Direction.South:
                                        {
                                            item.ItemID = itemIDs[0];
                                            break;
                                        }
                                        case Direction.East:
                                        case Direction.West:
                                        {
                                            item.ItemID = itemIDs[1];
                                            break;
                                        }
                                    }
                                }
                                else if (itemIDs.Length == 4)
                                {
                                    switch (dir)
                                    {
                                        case Direction.South:
                                        {
                                            item.ItemID = itemIDs[0];
                                            break;
                                        }
                                        case Direction.East:
                                        {
                                            item.ItemID = itemIDs[1];
                                            break;
                                        }
                                        case Direction.North:
                                        {
                                            item.ItemID = itemIDs[2];
                                            break;
                                        }
                                        case Direction.West:
                                        {
                                            item.ItemID = itemIDs[3];
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        public override int GetPacketFlags()
        {
            int flags = base.GetPacketFlags();

            return flags;
        }

        public override int GetOldPacketFlags()
        {
            int flags = base.GetOldPacketFlags();

            return flags;
        }

        public bool GetFlag(PlayerFlag flag)
        {
            return (m_Flags & flag) != 0;
        }

        public void SetFlag(PlayerFlag flag, bool value)
        {
            if (value)
            {
                m_Flags |= flag;
            }
            else
            {
                m_Flags &= ~flag;
            }
        }

        public DesignContext DesignContext { get => m_DesignContext; set => m_DesignContext = value; }

        public static void Initialize()
        {
            if (FastwalkPrevention)
            {
                PacketHandlers.RegisterThrottler(0x02, MovementThrottle_Callback);
            }

            Timer.DelayCall(TimeSpan.Zero, CheckPets);
        }

        public static void TargetedSkillUse(Mobile from, IEntity target, int skillId)
        {
            if (from == null || target == null)
            {
                return;
            }

            from.TargetLocked = true;

            if (skillId == (short)SkillName.Provocation)
            {
                Provocation.DeferredSecondaryTarget = true;
                InvokeTarget(from, target, skillId);
                Provocation.DeferredSecondaryTarget = false;
            }

            else if (skillId == (short)SkillName.AnimalTaming)
            {
                AnimalTaming.DisableMessage = true;
                AnimalTaming.DeferredTarget = false;
                InvokeTarget(from, target, skillId);
                AnimalTaming.DeferredTarget = true;
                AnimalTaming.DisableMessage = false;

            }
            else
            {
                InvokeTarget(from, target, skillId);
            }


            from.TargetLocked = false;
        }

        private static void InvokeTarget(Mobile from, IEntity target, int skillId)
        {
            if (from.UseSkill(skillId))
            {
                from.Target?.Invoke(from, target);
            }
        }

        public static void EquipMacro(Mobile m, List<Serial> list)
        {
            if (m is PlayerMobile pm && pm.Backpack != null && pm.Alive && list != null && list.Count > 0 && !pm.HasTrade)
            {
                if (pm.IsStaff() || Core.TickCount - pm.NextActionTime >= 0)
                {
                    Container pack = pm.Backpack;

                    for (int itemSerial = 0; itemSerial < list.Count; itemSerial++)
                    {
                        Serial serial = list[itemSerial];

                        Item item = null;

                        for (int index = 0; index < pack.Items.Count; index++)
                        {
                            Item i = pack.Items[index];

                            if (i.Serial == serial)
                            {
                                item = i;
                                break;
                            }
                        }

                        if (item != null)
                        {
                            Item toMove = pm.FindItemOnLayer(item.Layer);

                            if (toMove != null)
                            {
                                toMove.Internalize();

                                if (!pm.EquipItem(item))
                                {
                                    pm.EquipItem(toMove);
                                }
                                else
                                {
                                    pack.DropItem(toMove);
                                }
                            }
                            else
                            {
                                pm.EquipItem(item);
                            }
                        }
                    }

                    pm.NextActionTime = Core.TickCount + ActionDelay * list.Count;
                }
	            else
	            {
	                pm.SendActionMessage();
	            }
	        }
        }

        public static void UnequipMacro(Mobile m, List<Layer> layers)
        {
            if (m is PlayerMobile pm && pm.Backpack != null && pm.Alive && layers != null && layers.Count > 0 && !pm.HasTrade)
            {
                if (pm.IsStaff() || Core.TickCount - pm.NextActionTime >= 0)
                {
                    Container pack = pm.Backpack;

                    List<Item> worn = new List<Item>(pm.Items);

                    for (int index = 0; index < worn.Count; index++)
                    {
                        Item item = worn[index];

                        if (layers.Contains(item.Layer))
                        {
                            pack.TryDropItem(pm, item, false);
                        }
                    }

                    pm.NextActionTime = Core.TickCount + ActionDelay;
                    ColUtility.Free(worn);
                }
	            else
	            {
	                pm.SendActionMessage();
	            }
	        }
        }

        private static void CheckPets()
        {
            foreach (Mobile value in World.Mobiles.Values)
            {
                if (value is PlayerMobile pm && ((!pm.Mounted || pm.Mount is EtherealMount) && pm.AllFollowers.Count > pm.AutoStabled.Count || pm.Mounted && pm.AllFollowers.Count > pm.AutoStabled.Count + 1))
                {
                    pm.AutoStablePets(); /* autostable checks summons, et al: no need here */
                }
            }
        }

        public override void OnSkillInvalidated(Skill skill)
        {
            if (skill.SkillName == SkillName.MagicResist)
            {
                UpdateResistances();
            }
        }

        public override int GetMaxResistance(ResistanceType type)
        {
            int max = base.GetMaxResistance(type);

            max += Spells.Mysticism.StoneFormSpell.GetMaxResistBonus(this);

            if (type != ResistanceType.Physical && 60 < max && CurseSpell.UnderEffect(this))
            {
                max -= 10;
            }

            if ((type == ResistanceType.Fire || type == ResistanceType.Poison) && CorpseSkinSpell.IsUnderEffects(this))
            {
                max = CorpseSkinSpell.GetResistMalus(this);
            }

            if (type == ResistanceType.Physical && MagicReflectSpell.HasReflect(this))
            {
                max -= 5;
            }

            return max;
        }

        public override void ComputeResistances()
        {
            base.ComputeResistances();

            for (int i = 0; i < Resistances.Length; ++i)
            {
                Resistances[i] = 0;
            }

            Resistances[0] += BasePhysicalResistance;
            Resistances[1] += BaseFireResistance;
            Resistances[2] += BaseColdResistance;
            Resistances[3] += BasePoisonResistance;
            Resistances[4] += BaseEnergyResistance;

            for (int i = 0; ResistanceMods != null && i < ResistanceMods.Count; ++i)
            {
                ResistanceMod mod = ResistanceMods[i];
                int v = (int)mod.Type;

                if (v >= 0 && v < Resistances.Length)
                {
                    Resistances[v] += mod.Offset;
                }
            }

            for (int i = 0; i < Items.Count; ++i)
            {
                Item item = Items[i];

                if (item.CheckPropertyConfliction(this))
                {
                    continue;
                }

                ISetItem setItem = item as ISetItem;

                Resistances[0] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Physical) : item.PhysicalResistance;
                Resistances[1] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Fire) : item.FireResistance;
                Resistances[2] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Cold) : item.ColdResistance;
                Resistances[3] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Poison) : item.PoisonResistance;
                Resistances[4] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Energy) : item.EnergyResistance;
            }

            for (int i = 0; i < Resistances.Length; ++i)
            {
                int min = GetMinResistance((ResistanceType)i);
                int max = GetMaxResistance((ResistanceType)i);

                if (max < min)
                {
                    max = min;
                }

                if (Resistances[i] > max)
                {
                    Resistances[i] = max;
                }
                else if (Resistances[i] < min)
                {
                    Resistances[i] = min;
                }
            }
        }

        public override int MaxWeight => (Race == Race.Human ? 100 : 40) + (int)(3.5 * Str);

        private int m_LastGlobalLight = -1, m_LastPersonalLight = -1;

        public override void OnNetStateChanged()
        {
            m_LastGlobalLight = -1;
            m_LastPersonalLight = -1;
        }

        public override void ComputeBaseLightLevels(out int global, out int personal)
        {
            global = LightCycle.ComputeLevelFor(this);

            if (LightLevel < 21 && AosAttributes.GetValue(this, AosAttribute.NightSight) > 0)
            {
                personal = 21;
            }
            else
            {
                personal = LightLevel;
            }
        }

        public override void CheckLightLevels(bool forceResend)
        {
            NetState ns = NetState;

            if (ns == null)
            {
                return;
            }

            int global, personal;

            ComputeLightLevels(out global, out personal);

            if (!forceResend)
            {
                forceResend = global != m_LastGlobalLight || personal != m_LastPersonalLight;
            }

            if (!forceResend)
            {
                return;
            }

            m_LastGlobalLight = global;
            m_LastPersonalLight = personal;

            ns.Send(GlobalLightLevelPacket.Instantiate(global));
            ns.Send(new PersonalLightLevelPacket(this, personal));
        }

        public override bool SendSpeedControl(SpeedControlType type)
        {
            AnimalFormContext context = AnimalForm.GetContext(this);

            if (context != null && context.SpeedBoost)
            {
                switch (type)
                {
                    case SpeedControlType.WalkSpeed:
                    {
                        return base.SendSpeedControl(SpeedControlType.WalkSpeedFast);
                    }
                    case SpeedControlType.Disable:
                    {
                        return base.SendSpeedControl(SpeedControlType.MountSpeed);
                    }
                }
            }

            return base.SendSpeedControl(type);
        }

        public override int GetMinResistance(ResistanceType type)
        {
            int magicResist = (int)(Skills[SkillName.MagicResist].Value * 10);
            int min = int.MinValue;

            if (magicResist >= 1000)
            {
                min = 40 + (magicResist - 1000) / 50;
            }
            else if (magicResist >= 400)
            {
                min = (magicResist - 400) / 15;
            }

            return Math.Max(MinPlayerResistance, Math.Min(MaxPlayerResistance, min));
        }

        public override int GetResistance(ResistanceType type)
        {
            int resistance = base.GetResistance(type) + SphynxFortune.GetResistanceBonus(this, type);

            if (CityLoyaltySystem.HasTradeDeal(this, TradeDeal.SocietyOfClothiers))
            {
                resistance++;
                return Math.Min(resistance, GetMaxResistance(type));
            }

            return resistance;
        }

        public override void OnManaChange(int oldValue)
        {
            base.OnManaChange(oldValue);
            if (m_ExecutesLightningStrike > 0)
            {
                if (Mana < m_ExecutesLightningStrike)
                {
                    SpecialMove.ClearCurrentMove(this);
                }
            }
        }

        public override void OnLogin()
        {
            CheckAtrophies(this);

            if (AccountHandler.LockdownLevel > AccessLevel.VIP)
            {
                string notice;

                Account acct = Account as Account;

                if (acct == null || !acct.HasAccess(NetState))
                {
                    notice = IsPlayer() ? "The server is currently under lockdown. No players are allowed to log in at this time." : "The server is currently under lockdown. You do not have sufficient access level to connect.";

                    Timer.DelayCall(TimeSpan.FromSeconds(1.0), Disconnect, this);
                }
                else if (AccessLevel >= AccessLevel.Administrator)
                {
                    notice = "The server is currently under lockdown. As you are an administrator, you may change this from the [Admin gump.";
                }
                else
                {
                    notice = "The server is currently under lockdown. You have sufficient access level to connect.";
                }

                SendGump(new NoticeGump(1060637, 30720, notice, 0xFFC000, 300, 140, null, null));
                return;
            }

            ClaimAutoStabledPets();
            ValidateEquipment();

            ReportMurdererGump.CheckMurderer(this);

            QuestHelper.QuestionQuestCheck(this);

            if (NetState != null && NetState.IsEnhancedClient && Mount is EtherealMount fromMount)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(1), mount =>
                {
                    if (mount.IsChildOf(Backpack))
                    {
                        mount.Rider = this;
                    }
                },
                fromMount);
            }

            CheckStatTimers();

            AnimalForm.OnLogin(this);
            BaseBeverage.OnLogin(this);
            ChampionSpawnRegion.OnLogin(this);
            CityLoyaltySystem.OnLogin(this);
            CorgulRegion.OnLogin(this);
            FellowshipMedallion.OnLogin(this);
            Focus.OnLogin(this);
            GiftGiving.OnLogin(this);
            GiftOfLifeSpell.OnLogin(this);
            HouseRegion.OnLogin(this);
            LampRoomRegion.OnLogin(this);
            LightCycle.OnLogin(this);
            LoginStats.OnLogin(this);
            MaginciaLottoSystem.OnLogin(this);
            MasteryInfo.OnLogin(this);
            PlantSystem.OnLogin(this);
            PotionOfGloriousFortune.OnLogin(this);
            ShadowguardController.OnLogin(this);
            ShardPoller.OnLogin(this);
            StormLevelGump.OnLogin(this);
            Strandedness.OnLogin(this);
            TwistedWealdDesert.OnLogin(this);

            Engines.PartySystem.Party.OnLogin(this);

            if (PreventInaccess.Enabled)
            {
                PreventInaccess.OnLogin(this);
            }

            if (TownCryerSystem.Enabled)
            {
                TownCryerSystem.OnLogin(this);
            }

            if (ViceVsVirtueSystem.Enabled)
            {
                ViceVsVirtueSystem.OnLogin(this);
            }

            ResendBuffs();
        }

        private bool m_NoDeltaRecursion;

        public void ValidateEquipment()
        {
            if (m_NoDeltaRecursion || Map == null || Map == Map.Internal)
            {
                return;
            }

            if (Items == null)
            {
                return;
            }

            m_NoDeltaRecursion = true;
            Timer.DelayCall(TimeSpan.Zero, ValidateEquipment_Sandbox);
        }

        private void ValidateEquipment_Sandbox()
        {
            try
            {
                if (Map == null || Map == Map.Internal)
                {
                    return;
                }

                List<Item> items = Items;

                if (items == null)
                {
                    return;
                }

                bool moved = false;

                int str = Str;
                int dex = Dex;
                int intel = Int;

                Mobile from = this;

                for (int i = items.Count - 1; i >= 0; --i)
                {
                    if (i >= items.Count)
                    {
                        continue;
                    }

                    Item item = items[i];

                    bool drop = !RaceDefinitions.ValidateEquipment(from, item, false);

                    if (item is BaseWeapon weapon)
                    {
                        if (!drop)
                        {
                            if (dex < weapon.DexRequirement)
                            {
                                drop = true;
                            }
                            else if (str < AOS.Scale(weapon.StrRequirement, 100 - weapon.GetLowerStatReq()))
                            {
                                drop = true;
                            }
                            else if (intel < weapon.IntRequirement)
                            {
                                drop = true;
                            }
                        }

                        if (drop)
                        {
                            string name = weapon.Name;

                            if (name == null)
                            {
                                name = $"#{weapon.LabelNumber}";
                            }

                            from.SendLocalizedMessage(1062001, name); // You can no longer wield your ~1_WEAPON~
                            from.AddToBackpack(weapon);
                            moved = true;
                        }
                    }
                    else if (item is BaseArmor armor)
                    {
                        if (!drop)
                        {
                            if (!armor.AllowMaleWearer && !from.Female && from.AccessLevel < AccessLevel.GameMaster)
                            {
                                drop = true;
                            }
                            else if (!armor.AllowFemaleWearer && from.Female && from.AccessLevel < AccessLevel.GameMaster)
                            {
                                drop = true;
                            }
                            else
                            {
                                int strBonus = armor.ComputeStatBonus(StatType.Str), strReq = armor.ComputeStatReq(StatType.Str);
                                int dexBonus = armor.ComputeStatBonus(StatType.Dex), dexReq = armor.ComputeStatReq(StatType.Dex);
                                int intBonus = armor.ComputeStatBonus(StatType.Int), intReq = armor.ComputeStatReq(StatType.Int);

                                if (dex < dexReq || dex + dexBonus < 1)
                                {
                                    drop = true;
                                }
                                else if (str < strReq || str + strBonus < 1)
                                {
                                    drop = true;
                                }
                                else if (intel < intReq || intel + intBonus < 1)
                                {
                                    drop = true;
                                }
                            }
                        }

                        if (drop)
                        {
                            string name = armor.Name;

                            if (name == null)
                            {
                                name = $"#{armor.LabelNumber}";
                            }

                            if (armor is BaseShield)
                            {
                                from.SendLocalizedMessage(1062003, name); // You can no longer equip your ~1_SHIELD~
                            }
                            else
                            {
                                from.SendLocalizedMessage(1062002, name); // You can no longer wear your ~1_ARMOR~
                            }

                            from.AddToBackpack(armor);
                            moved = true;
                        }
                    }
                    else if (item is BaseClothing clothing)
                    {
                        if (!drop)
                        {
                            if (!clothing.AllowMaleWearer && !from.Female && from.AccessLevel < AccessLevel.GameMaster)
                            {
                                drop = true;
                            }
                            else if (!clothing.AllowFemaleWearer && from.Female && from.AccessLevel < AccessLevel.GameMaster)
                            {
                                drop = true;
                            }
                            else
                            {
                                int strBonus = clothing.ComputeStatBonus(StatType.Str);
                                int strReq = clothing.ComputeStatReq(StatType.Str);

                                if (str < strReq || str + strBonus < 1)
                                {
                                    drop = true;
                                }
                            }
                        }

                        if (drop)
                        {
                            string name = clothing.Name;

                            if (name == null)
                            {
                                name = $"#{clothing.LabelNumber}";
                            }

                            from.SendLocalizedMessage(1062002, name); // You can no longer wear your ~1_ARMOR~

                            from.AddToBackpack(clothing);
                            moved = true;
                        }
                    }
                    else if (item is BaseQuiver && drop)
                    {
                        from.AddToBackpack(item);

                        from.SendLocalizedMessage(1062002, "quiver"); // You can no longer wear your ~1_ARMOR~
                        moved = true;
                    }

                    if (item is IVvVItem vvvItem && vvvItem.IsVvVItem && !ViceVsVirtueSystem.IsVvV(from))
                    {
                        from.AddToBackpack(item);
                        moved = true;
                    }
                }

                if (from.Mount is VvVMount && !ViceVsVirtueSystem.IsVvV(from))
                {
                    from.Mount.Rider = null;
                }

                if (moved)
                {
                    from.SendLocalizedMessage(500647); // Some equipment has been moved to your backpack.
                }
            }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            finally
            {
                m_NoDeltaRecursion = false;
            }
        }

        public override void Delta(MobileDelta flag)
        {
            base.Delta(flag);

            if ((flag & MobileDelta.Stat) != 0)
            {
                ValidateEquipment();
            }

            InvalidateProperties();
        }

        public override void OnConnected(Mobile m)
        {
            base.OnConnected(m);

            if (m is PlayerMobile pm)
            {
                pm.m_SessionStart = DateTime.UtcNow;

                QuestHelper.StartTimer(pm);

                pm.BedrollLogout = false;

                pm.LastOnline = DateTime.UtcNow;
            }

            DisguiseTimers.StartTimer(m);

            Timer.DelayCall(TimeSpan.Zero, ClearSpecialMovesCallback, m);
        }

        private static void ClearSpecialMovesCallback(object state)
        {
            Mobile from = (Mobile)state;

            SpecialMove.ClearAllMoves(from);
        }

        public override void OnDisconnected(Mobile m)
        {
            base.OnDisconnected(m);

            // Young timer
            Accounting.Account.OnDisconnected(m);

            DesignContext context = DesignContext.Find(m);
            if (context != null)
            {
                /* Client disconnected
                 *  - Remove design context
                 *  - Eject all from house
                 *  - Restore relocated entities
                 */
                // Remove design context
                DesignContext.Remove(m);

                // Eject all from house
                m.RevealingAction();

                List<Item> list = context.Foundation.GetItems();
                for (int index = 0; index < list.Count; index++)
                {
                    Item item = list[index];
                    item.Location = context.Foundation.BanLocation;
                }

                List<Mobile> mobiles = context.Foundation.GetMobiles();
                for (int index = 0; index < mobiles.Count; index++)
                {
                    Mobile mobile = mobiles[index];
                    mobile.Location = context.Foundation.BanLocation;
                }

                // Restore relocated entities
                context.Foundation.RestoreRelocatedEntities();
            }

            if (m is PlayerMobile pm)
            {
                pm.m_GameTime += DateTime.UtcNow - pm.m_SessionStart;

                QuestHelper.StopTimer(pm);

                pm.LastOnline = DateTime.UtcNow;

                pm.AutoStablePets();
            }

            DisguiseTimers.StopTimer(m);

            BaseBoat.OnDisconnected(m);

            // Remove from public chat
            Channel.OnDisconnected(m);

            ShadowguardController.OnDisconnected(m);
        }

        private static void Disconnect(object state)
        {
            NetState ns = ((Mobile)state).NetState;

            ns?.Dispose();
        }

        public override void OnLogout(Mobile m)
        {
            base.OnLogout(m);

            PlayerMobile pm = m as PlayerMobile;

            if (pm == null)
            {
                return;
            }

            ChampionSpawnRegion.OnLogout(pm);
            BaseEscort.DeleteEscort(pm);
            BaseFamiliar.OnLogout(pm);
            BasketOfHerbs.CheckBonus(pm);
            InstanceRegion.OnLogout(pm);
            Engines.PartySystem.Party.OnLogout(pm);
        }

        public override void RevealingAction()
        {
            if (m_DesignContext != null)
            {
                return;
            }

            InvisibilitySpell.RemoveTimer(this);

            base.RevealingAction();
        }

        public override void OnHiddenChanged()
        {
            base.OnHiddenChanged();

            RemoveBuff(BuffIcon.Invisibility);
            //Always remove, default to the hiding icon EXCEPT in the invis spell where it's explicitly set

            if (!Hidden)
            {
                RemoveBuff(BuffIcon.HidingAndOrStealth);
            }
            else // if( !InvisibilitySpell.HasTimer( this ) )
            {
                BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.HidingAndOrStealth, 1075655)); //Hidden/Stealthing & You Are Hidden
            }
        }

        public override void OnSubItemAdded(Item item)
        {
            if (AccessLevel < AccessLevel.GameMaster && item.IsChildOf(Backpack))
            {
                int curWeight = BodyWeight + TotalWeight;

                if (curWeight > MaxWeight)
                {
                    SendLocalizedMessage(1019035, true, $" : {curWeight} / {MaxWeight}");
                }
            }
        }

        public override void OnSubItemRemoved(Item item)
        {
            if (Engines.UOStore.UltimaStore.HasPendingItem(this))
            {
                Timer.DelayCall(TimeSpan.FromSeconds(1.5), Engines.UOStore.UltimaStore.CheckPendingItem, this);
            }
        }

        public override void AggressiveAction(Mobile aggressor, bool criminal)
        {
            // This will update aggressor for the aggressors master
            if (aggressor is BaseCreature creature && creature.ControlMaster != null && creature.ControlMaster != this)
            {
                Mobile aggressiveMaster = creature.ControlMaster;

                // First lets find out if the creatures master is in our aggressor list
                AggressorInfo info = null;

                for (int index = 0; index < Aggressors.Count; index++)
                {
                    AggressorInfo i = Aggressors[index];

                    if (i.Attacker == aggressiveMaster)
                    {
                        info = i;
                        break;
                    }
                }

                if (info != null)
                {
                    // already in the list, so we're refreshing it
                    info.Refresh();
                    info.CriminalAggression = criminal;
                }
                else
                {
                    // not in the list, so we're adding it
                    Aggressors.Add(AggressorInfo.Create(aggressiveMaster, this, criminal));

                    if (CanSee(aggressiveMaster))
                    {
                        NetState?.Send(MobileIncoming.Create(NetState, this, aggressiveMaster));
                    }

                    UpdateAggrExpire();
                }

                // Now, if I am in the creatures master aggressor list, it needs to be refreshed
                info = null;

                for (int index = 0; index < aggressiveMaster.Aggressors.Count; index++)
                {
                    AggressorInfo i = aggressiveMaster.Aggressors[index];

                    if (i.Attacker == this)
                    {
                        info = i;
                        break;
                    }
                }

                info?.Refresh();

                info = null;

                for (int index = 0; index < Aggressed.Count; index++)
                {
                    AggressorInfo i = Aggressed[index];

                    if (i.Defender == aggressiveMaster)
                    {
                        info = i;
                        break;
                    }
                }

                info?.Refresh();

                // next lets find out if we're on the creatures master aggressed list
                info = null;

                for (int index = 0; index < aggressiveMaster.Aggressed.Count; index++)
                {
                    AggressorInfo i = aggressiveMaster.Aggressed[index];

                    if (i.Defender == this)
                    {
                        info = i;
                        break;
                    }
                }

                if (info != null)
                {
                    // already in the list, so we're refreshing it
                    info.Refresh();
                    info.CriminalAggression = criminal;
                }
                else
                {
                    // not in the list, so we're adding it
                    creature.Aggressed.Add(AggressorInfo.Create(aggressiveMaster, this, criminal));

                    if (CanSee(aggressiveMaster))
                    {
                        NetState?.Send(MobileIncoming.Create(NetState, this, aggressiveMaster));
                    }

                    UpdateAggrExpire();
                }

                if (aggressiveMaster is PlayerMobile || aggressiveMaster is BaseCreature bc && !bc.IsMonster)
                {
                    BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.HeatOfBattleStatus, 1153801, 1153827, Aggression.CombatHeatDelay, this, true));
                    BuffInfo.AddBuff(aggressiveMaster, new BuffInfo(BuffIcon.HeatOfBattleStatus, 1153801, 1153827, Aggression.CombatHeatDelay, aggressiveMaster, true));
                }
            }

            base.AggressiveAction(aggressor, criminal);
        }

        public override void DoHarmful(IDamageable damageable, bool indirect)
        {
            base.DoHarmful(damageable, indirect);

            if (ViceVsVirtueSystem.Enabled && (ViceVsVirtueSystem.EnhancedRules || Map == ViceVsVirtueSystem.Facet) && damageable is Mobile mobile)
            {
                ViceVsVirtueSystem.CheckHarmful(this, mobile);
            }
        }

        public override void DoBeneficial(Mobile target)
        {
            base.DoBeneficial(target);

            if (ViceVsVirtueSystem.Enabled && (ViceVsVirtueSystem.EnhancedRules || Map == ViceVsVirtueSystem.Facet) && target != null)
            {
                ViceVsVirtueSystem.CheckBeneficial(this, target);
            }
        }

        public override bool CanBeHarmful(IDamageable damageable, bool message, bool ignoreOurBlessedness, bool ignorePeaceCheck)
        {
            Mobile target = damageable as Mobile;

            if (m_DesignContext != null || target is PlayerMobile mobile && mobile.m_DesignContext != null)
            {
                return false;
            }

            if (target is BaseVendor vendor && vendor.IsInvulnerable || target is PlayerVendor || target is TownCrier)
            {
                if (message)
                {
                    if (target.Title == null)
                    {
                        SendMessage($"{target.Name} the vendor cannot be harmed.");
                    }
                    else
                    {
                        SendMessage($"{target.Name} {target.Title} cannot be harmed.");
                    }
                }

                return false;
            }

            if (damageable is IDamageableItem item && !item.CanDamage)
            {
                if (message)
                {
                    SendMessage("That cannot be harmed.");
                }

                return false;
            }

            return base.CanBeHarmful(damageable, message, ignoreOurBlessedness, ignorePeaceCheck);
        }

        public override bool CanBeBeneficial(Mobile target, bool message, bool allowDead)
        {
            if (m_DesignContext != null || target is PlayerMobile pm && pm.m_DesignContext != null)
            {
                return false;
            }

            return base.CanBeBeneficial(target, message, allowDead);
        }

        public override bool CheckContextMenuDisplay(IEntity target)
        {
            return m_DesignContext == null;
        }

        public override void OnItemAdded(Item item)
        {
            base.OnItemAdded(item);

            if (item is BaseArmor || item is BaseWeapon)
            {
                Hits = Hits;
                Stam = Stam;
                Mana = Mana;
            }

            if (NetState != null)
            {
                CheckLightLevels(false);
            }
        }

        private BaseWeapon m_LastWeapon;

        [CommandProperty(AccessLevel.GameMaster)]
        public BaseWeapon LastWeapon { get => m_LastWeapon; set => m_LastWeapon = value; }

        public override void OnItemRemoved(Item item)
        {
            base.OnItemRemoved(item);

            if (item is BaseArmor || item is BaseWeapon)
            {
                Hits = Hits;
                Stam = Stam;
                Mana = Mana;
            }

            if (item is BaseWeapon weapon)
            {
                m_LastWeapon = weapon;
            }

            if (NetState != null)
            {
                CheckLightLevels(false);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override int HitsMax
        {
            get
            {
                int strBase;
                int strOffs = GetStatOffset(StatType.Str);

                strBase = Str; //Str already includes GetStatOffset/str
                strOffs = AosAttributes.GetValue(this, AosAttribute.BonusHits);

                if (strOffs > 25 && IsPlayer())
                {
                    strOffs = 25;
                }

                if (AnimalForm.UnderTransformation(this, typeof(BakeKitsune)) ||
                    AnimalForm.UnderTransformation(this, typeof(GreyWolf)))
                {
                    strOffs += 20;
                }

                // Skill Masteries
                strOffs += ToughnessSpell.GetHPBonus(this);
                strOffs += InvigorateSpell.GetHPBonus(this);

                return strBase / 2 + 50 + strOffs;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override int StamMax => base.StamMax + AosAttributes.GetValue(this, AosAttribute.BonusStam);

        [CommandProperty(AccessLevel.GameMaster)]
        public override int ManaMax => base.ManaMax + AosAttributes.GetValue(this, AosAttribute.BonusMana) + MasteryInfo.IntuitionBonus(this) + UraliTranceTonic.GetManaBuff(this);

        [CommandProperty(AccessLevel.GameMaster)]
        public override int Str
        {
            get
            {
                if (IsPlayer())
                {
                    return Math.Min(base.Str, StrMaxCap);
                }

                return base.Str;
            }
            set => base.Str = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override int Int
        {
            get
            {
                if (IsPlayer())
                {
                    return Math.Min(base.Int, IntMaxCap);
                }

                return base.Int;
            }
            set => base.Int = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override int Dex
        {
            get
            {
                if (IsPlayer())
                {
                    int dex = base.Dex;

                    return Math.Min(dex, DexMaxCap);
                }

                return base.Dex;
            }
            set => base.Dex = value;
        }

        public long NextPassiveDetectHidden { get; set; }

        public override bool Move(Direction d)
        {
            NetState ns = NetState;

            if (ns != null)
            {
                if (HasGump(typeof(ResurrectGump)))
                {
                    if (Alive)
                    {
                        CloseGump(typeof(ResurrectGump));
                    }
                    else
                    {
                        SendLocalizedMessage(500111); // You are frozen and cannot move.
                        return false;
                    }
                }
            }

            int speed = ComputeMovementSpeed(d);

            bool result = base.Move(d);

            if (result && Core.TickCount - NextPassiveDetectHidden >= 0)
            {
                DetectHidden.DoPassiveDetect(this);
                NextPassiveDetectHidden = Core.TickCount + (int)TimeSpan.FromSeconds(2).TotalMilliseconds;
            }

            m_NextMovementTime += speed;

            return result;
        }

        public override bool CheckMovement(Direction d, out int newZ)
        {
            DesignContext context = m_DesignContext;

            if (context == null)
            {
                bool check = base.CheckMovement(d, out newZ);

                if (check && VvVSigil.ExistsOn(this, true) && !VvVSigil.CheckMovement(this, d))
                {
                    SendLocalizedMessage(1155414); // You may not remove the sigil from the battle region!
                    return false;
                }

                return check;
            }

            HouseFoundation foundation = context.Foundation;

            newZ = foundation.Z + HouseFoundation.GetLevelZ(context.Level, context.Foundation);

            int newX = X, newY = Y;
            Movement.Movement.Offset(d, ref newX, ref newY);

            int startX = foundation.X + foundation.Components.Min.X + 1;
            int startY = foundation.Y + foundation.Components.Min.Y + 1;
            int endX = startX + foundation.Components.Width - 1;
            int endY = startY + foundation.Components.Height - 2;

            return newX >= startX && newY >= startY && newX < endX && newY < endY && Map == foundation.Map;
        }

        public override void OnHeal(ref int amount, Mobile from)
        {
            base.OnHeal(ref amount, from);

            if (from == null)
            {
                return;
            }

            BestialSetHelper.OnHeal(this, from, ref amount);

            if (amount > 0 && from != this)
            {
                for (int i = Aggressed.Count - 1; i >= 0; i--)
                {
                    AggressorInfo info = Aggressed[i];

                    bool any = false;

                    for (int index = 0; index < info.Defender.DamageEntries.Count; index++)
                    {
                        DamageEntry de = info.Defender.DamageEntries[index];

                        if (de.Damager == this)
                        {
                            any = true;
                            break;
                        }
                    }

                    if (info.Defender.InRange(Location, Core.GlobalMaxUpdateRange) && any)
                    {
                        info.Defender.RegisterDamage(amount, from);
                    }

                    if (info.Defender.Player && from.CanBeHarmful(info.Defender, false))
                    {
                        from.DoHarmful(info.Defender, true);
                    }
                }

                for (int i = Aggressors.Count - 1; i >= 0; i--)
                {
                    AggressorInfo info = Aggressors[i];

                    bool any = false;

                    for (int index = 0; index < info.Attacker.DamageEntries.Count; index++)
                    {
                        DamageEntry de = info.Attacker.DamageEntries[index];

                        if (de.Damager == this)
                        {
                            any = true;
                            break;
                        }
                    }

                    if (info.Attacker.InRange(Location, Core.GlobalMaxUpdateRange) && any)
                    {
                        info.Attacker.RegisterDamage(amount, from);
                    }

                    if (info.Attacker.Player && from.CanBeHarmful(info.Attacker, false))
                    {
                        from.DoHarmful(info.Attacker, true);
                    }
                }
            }
        }

        public override bool AllowItemUse(Item item)
        {
            return DesignContext.Check(this);
        }

        public SkillName[] AnimalFormRestrictedSkills => m_AnimalFormRestrictedSkills;

        private readonly SkillName[] m_AnimalFormRestrictedSkills =
        {
            SkillName.ArmsLore, SkillName.Begging, SkillName.Discordance, SkillName.Forensics, SkillName.Inscribe,
            SkillName.ItemID, SkillName.Meditation, SkillName.Peacemaking, SkillName.Provocation, SkillName.RemoveTrap,
            SkillName.SpiritSpeak, SkillName.Stealing, SkillName.TasteID
        };

        public override bool AllowSkillUse(SkillName skill)
        {
            if (AnimalForm.UnderTransformation(this))
            {
                for (int i = 0; i < m_AnimalFormRestrictedSkills.Length; i++)
                {
                    if (m_AnimalFormRestrictedSkills[i] == skill)
                    {
                        AnimalFormContext context = AnimalForm.GetContext(this);

                        if (skill == SkillName.Stealing && context.StealingMod != null && context.StealingMod.Value > 0)
                        {
                            continue;
                        }

                        SendLocalizedMessage(1070771); // You cannot use that skill in this form.
                        return false;
                    }
                }
            }

            return DesignContext.Check(this);
        }

        private bool m_LastProtectedMessage;
        private int m_NextProtectionCheck = 10;

        public virtual void RecheckTownProtection()
        {
            m_NextProtectionCheck = 10;

            GuardedRegion reg = (GuardedRegion)Region.GetRegion(typeof(GuardedRegion));

            bool isProtected = reg != null && !reg.IsDisabled();

            if (isProtected != m_LastProtectedMessage)
            {
                if (isProtected)
                {
                    SendLocalizedMessage(500112); // You are now under the protection of the town guards.
                }
                else
                {
                    SendLocalizedMessage(500113); // You have left the protection of the town guards.
                }

                m_LastProtectedMessage = isProtected;
            }
        }

        public override void MoveToWorld(Point3D loc, Map map)
        {
            base.MoveToWorld(loc, map);

            RecheckTownProtection();
        }

        public override void SetLocation(Point3D loc, bool isTeleport)
        {
            if (!isTeleport && IsPlayer() && !Flying)
            {
                // moving, not teleporting
                int zDrop = Location.Z - loc.Z;

                if (zDrop > 20) // we fell more than one story
                {
                    Hits -= zDrop / 20 * 10 - 5; // deal some damage; does not kill, disrupt, etc
                    SendMessage("Ouch!");
                }
            }

            base.SetLocation(loc, isTeleport);

            if (isTeleport || --m_NextProtectionCheck == 0)
            {
                RecheckTownProtection();
            }
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            list.Add(new PaperdollEntry(this));

            if (from == this)
            {
                list.Add(new OpenBackpackEntry(this));

                if (ShadowguardController.GetInstance(Location, Map) != null)
                {
                    list.Add(new ExitEntry(this));
                }

                if (Alive)
                {
                    list.Add(new SearchVendors(this));
                }

                BaseHouse house = BaseHouse.FindHouseAt(this);

                if (house != null)
                {
                    if (house.IsCoOwner(this))
                    {
                        list.Add(new CallbackEntry(6205, ReleaseCoOwnership));
                    }
                }

                list.Add(new TitlesMenuEntry(this));

                if (Alive)
                {
                    list.Add(new Engines.Points.LoyaltyRating(this));
                }

                if (Alive)
                {
                    QuestHelper.GetContextMenuEntries(list);
                }

                if (house != null)
                {
                    if (Alive && house.InternalizedVendors.Count > 0 && house.IsOwner(this))
                    {
                        list.Add(new CallbackEntry(6204, GetVendor));
                    }

                    list.Add(new CallbackEntry(6207, LeaveHouse));
                }

                list.Add(new CallbackEntry(RefuseTrades ? 1154112 : 1154113, ToggleTrades)); // Allow Trades / Refuse Trades				

                if (Region.IsPartOf<VoidPoolRegion>())
                {
                    VoidPoolController controller = Map == Map.Felucca ? VoidPoolController.InstanceFel : VoidPoolController.InstanceTram;

                    if (controller != null)
                    {
                        list.Add(new VoidPoolInfo(this, controller));
                    }
                }

                if (DisabledPvpWarning)
                {
                    list.Add(new CallbackEntry(1113797, EnablePvpWarning));
                }
            }
            else
            {
                BaseGalleon galleon = BaseGalleon.FindGalleonAt(from.Location, from.Map);

                if (galleon != null && galleon.IsOwner(from))
                {
                    list.Add(new ShipAccessEntry(this, from, galleon));
                }

                if (Alive)
                {
                    Party theirParty = from.Party as Party;
                    Party ourParty = Party as Party;

                    if (theirParty == null && ourParty == null)
                    {
                        list.Add(new AddToPartyEntry(from, this));
                    }
                    else if (theirParty != null && theirParty.Leader == from)
                    {
                        if (ourParty == null)
                        {
                            list.Add(new AddToPartyEntry(from, this));
                        }
                        else if (ourParty == theirParty)
                        {
                            list.Add(new RemoveFromPartyEntry(from, this));
                        }
                    }
                }

                if (from.InRange(this, 10))
                {
                    list.Add(new CallbackEntry(1077728, () => OpenTrade(from))); // Trade
                }

                if (Alive && EjectPlayerEntry.CheckAccessible(from, this))
                {
                    list.Add(new EjectPlayerEntry(from, this));
                }
            }
        }

        private void ToggleTrades()
        {
            RefuseTrades = !RefuseTrades;
        }

        private void GetVendor()
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (CheckAlive() && house != null && house.IsOwner(this) && house.InternalizedVendors.Count > 0)
            {
                CloseGump(typeof(ReclaimVendorGump));
                SendGump(new ReclaimVendorGump(house));
            }
        }

        private void LeaveHouse()
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (house != null)
            {
                Location = house.BanLocation;
            }
        }

        private void ReleaseCoOwnership()
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (house != null && house.IsCoOwner(this))
            {
                SendGump(new WarningGump(1060635, 30720, 1062006, 32512, 420, 280, ClearCoOwners_Callback, house));
            }
        }

        public static void ClearCoOwners_Callback(Mobile from, bool okay, object state)
        {
            BaseHouse house = (BaseHouse)state;

            if (house.Deleted)
            {
                return;
            }

            if (okay && house.IsCoOwner(from))
            {
                house.CoOwners?.Remove(from);
                from.SendLocalizedMessage(501300); // You have been removed as a house co-owner.
            }
        }

        private void EnablePvpWarning()
        {
            DisabledPvpWarning = false;
            SendLocalizedMessage(1113798); // Your PvP warning query has been re-enabled.
        }

        private delegate void ContextCallback();

        private class CallbackEntry : ContextMenuEntry
        {
            private readonly ContextCallback m_Callback;

            public CallbackEntry(int number, ContextCallback callback)
                : this(number, -1, callback)
            { }

            public CallbackEntry(int number, int range, ContextCallback callback)
                : base(number, range)
            {
                m_Callback = callback;
            }

            public override void OnClick()
            {
                m_Callback?.Invoke();
            }
        }

        public override void DisruptiveAction()
        {
            if (Meditating)
            {
                RemoveBuff(BuffIcon.ActiveMeditation);
            }

            base.DisruptiveAction();
        }

        public override bool Meditating
        {
            set
            {
                base.Meditating = value;
                if (value == false)
                {
                    RemoveBuff(BuffIcon.ActiveMeditation);
                }
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (this == from && !Warmode)
            {
                IMount mount = Mount;

                if (mount != null && !DesignContext.Check(this))
                {
                    return;
                }
            }

            base.OnDoubleClick(from);
        }

        public override void DisplayPaperdollTo(Mobile to)
        {
            if (DesignContext.Check(this))
            {
                base.DisplayPaperdollTo(to);
            }
        }

        private static bool m_NoRecursion;

        public override bool CheckEquip(Item item)
        {
            if (!base.CheckEquip(item))
            {
                return false;
            }

            Region r = Region.Find(Location, Map);

            if (r is ArenaRegion region && !region.AllowItemEquip(this, item))
            {
                return false;
            }

            if (item is IVvVItem vvvItem && vvvItem.IsVvVItem && !ViceVsVirtueSystem.IsVvV(this))
            {
                return false;
            }

            if (AccessLevel < AccessLevel.GameMaster && item.Layer != Layer.Mount && HasTrade)
            {
                BounceInfo bounce = item.GetBounce();

                if (bounce != null)
                {
                    if (bounce.m_Parent is Item parent)
                    {
                        if (parent == Backpack || parent.IsChildOf(Backpack))
                        {
                            return true;
                        }
                    }
                    else if (bounce.m_Parent == this)
                    {
                        return true;
                    }
                }

                SendLocalizedMessage(1004042); // You can only equip what you are already carrying while you have a trade pending.
                return false;
            }

            return true;
        }

        public override bool OnDragLift(Item item)
        {
            if (item is IPromotionalToken token && token.GumpType != null)
            {
                Type t = token.GumpType;

                if (HasGump(t))
                {
                    CloseGump(t);
                }
            }

            return base.OnDragLift(item);
        }

        public override bool CheckTrade(
            Mobile to, Item item, SecureTradeContainer cont, bool message, bool checkItems, int plusItems, int plusWeight)
        {
            int msgNum = 0;

            if (cont == null)
            {
                if (to.Holding != null)
                {
                    msgNum = 1062727; // You cannot trade with someone who is dragging something.
                }
                else if (HasTrade)
                {
                    msgNum = 1062781; // You are already trading with someone else!
                }
                else if (to.HasTrade)
                {
                    msgNum = 1062779; // That person is already involved in a trade
                }
                else if (to is PlayerMobile pm && pm.RefuseTrades)
                {
                    msgNum = 1154111; // ~1_NAME~ is refusing all trades.
                }
            }

            if (msgNum == 0 && item != null)
            {
                if (cont != null)
                {
                    plusItems += cont.TotalItems;
                    plusWeight += cont.TotalWeight;
                }

                if (Backpack == null || !Backpack.CheckHold(this, item, false, checkItems, plusItems, plusWeight))
                {
                    msgNum = 1004040; // You would not be able to hold this if the trade failed.
                }
                else if (to.Backpack == null || !to.Backpack.CheckHold(to, item, false, checkItems, plusItems, plusWeight))
                {
                    msgNum = 1004039; // The recipient of this trade would not be able to carry 
                }
                else
                {
                    msgNum = CheckContentForTrade(item);
                }
            }

            if (msgNum == 0)
            {
                return true;
            }

            if (!message)
            {
                return false;
            }

            if (msgNum == 1154111)
            {
                if (to != null)
                {
                    SendLocalizedMessage(msgNum, to.Name);
                }
            }
            else
            {
                SendLocalizedMessage(msgNum);
            }

            return false;
        }

        private static int CheckContentForTrade(Item item)
        {
            if (item is TrapableContainer container && container.TrapType != TrapType.None)
            {
                return 1004044; // You may not trade trapped items.
            }

            if (StolenItem.IsStolen(item))
            {
                return 1004043; // You may not trade recently stolen items.
            }

            if (item is Container)
            {
                for (int index = 0; index < item.Items.Count; index++)
                {
                    Item subItem = item.Items[index];

                    int msg = CheckContentForTrade(subItem);

                    if (msg != 0)
                    {
                        return msg;
                    }
                }
            }

            return 0;
        }

        public override bool CheckHasTradeDrop(Mobile from, Item item, Item target)
        {
            if (!base.CheckHasTradeDrop(from, item, target))
            {
                return false;
            }

            if (from.AccessLevel >= AccessLevel.GameMaster)
            {
                return true;
            }

            Container pack = Backpack;
            if (from == this && HasTrade && (target == pack || target.IsChildOf(pack)))
            {
                BounceInfo bounce = item.GetBounce();

                if (bounce != null && bounce.m_Parent is Item pItem && pItem == pack)
                {
                    if (pItem.IsChildOf(pack))
                    {
                        return true;
                    }
                }

                SendLocalizedMessage(1004041); // You can't do that while you have a trade pending.
                return false;
            }

            return true;
        }

        protected override void OnLocationChange(Point3D oldLocation)
        {
            CheckLightLevels(false);

            DesignContext context = m_DesignContext;

            if (context == null || m_NoRecursion)
            {
                return;
            }

            m_NoRecursion = true;

            HouseFoundation foundation = context.Foundation;

            int newX = X, newY = Y;
            int newZ = foundation.Z + HouseFoundation.GetLevelZ(context.Level, context.Foundation);

            int startX = foundation.X + foundation.Components.Min.X + 1;
            int startY = foundation.Y + foundation.Components.Min.Y + 1;
            int endX = startX + foundation.Components.Width - 1;
            int endY = startY + foundation.Components.Height - 2;

            if (newX >= startX && newY >= startY && newX < endX && newY < endY && Map == foundation.Map)
            {
                if (Z != newZ)
                {
                    Location = new Point3D(X, Y, newZ);
                }

                m_NoRecursion = false;
                return;
            }

            Location = new Point3D(foundation.X, foundation.Y, newZ);
            Map = foundation.Map;

            m_NoRecursion = false;
        }

        public override bool OnMoveOver(Mobile m)
        {
            if (m is BaseCreature bc && !bc.Controlled)
            {
                return !Alive || !bc.Alive || IsDeadBondedPet || bc.IsDeadBondedPet || Hidden && IsStaff();
            }

            return base.OnMoveOver(m);
        }

        public override bool CheckShove(Mobile shoved)
        {
            if (TransformationSpellHelper.UnderTransformation(shoved, typeof(WraithFormSpell)))
            {
                return true;
            }

            return base.CheckShove(shoved);
        }

        protected override void OnMapChange(Map oldMap)
        {
            ViceVsVirtueSystem.OnMapChange(this);

            if (NetState != null && NetState.IsEnhancedClient)
            {
                Waypoints.OnMapChange(this, oldMap);
            }

            if (Map != ViceVsVirtueSystem.Facet && oldMap == ViceVsVirtueSystem.Facet || Map == ViceVsVirtueSystem.Facet && oldMap != ViceVsVirtueSystem.Facet)
            {
                InvalidateProperties();
            }

            BaseGump.CheckCloseGumps(this);

            DesignContext context = m_DesignContext;

            if (context == null || m_NoRecursion)
            {
                return;
            }

            m_NoRecursion = true;

            HouseFoundation foundation = context.Foundation;

            if (Map != foundation.Map)
            {
                Map = foundation.Map;
            }

            m_NoRecursion = false;
        }

        public override bool IsBeneficialCriminal(Mobile target)
        {
            if (!target.Criminal && target is BaseCreature bc && bc.GetMaster() == this)
            {
                return false;
            }

            return base.IsBeneficialCriminal(target);
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            int disruptThreshold;

            if (from != null && from.Player)
            {
                disruptThreshold = 19;
            }
            else
            {
                disruptThreshold = 26;
            }

            disruptThreshold += Dex / 12;

            if (amount > disruptThreshold)
            {
                BandageContext c = BandageContext.GetContext(this);

                c?.Slip();
            }

            if (Confidence.IsRegenerating(this))
            {
                Confidence.StopRegenerating(this);
            }

            if (willKill && from is PlayerMobile pm)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(10), pm.RecoverAmmo);
            }

            if (InvisibilityPotion.HasTimer(this))
            {
                InvisibilityPotion.Iterrupt(this);
            }

            UndertakersStaff.TryRemoveTimer(this);

            base.OnDamage(amount, from, willKill);
        }

        public override void Resurrect()
        {
            bool wasAlive = Alive;

            base.Resurrect();

            if (Alive && !wasAlive)
            {
                if (NetState != null)
                {
                    Waypoints.RemoveHealers(this, Map);
                }
            }
        }

        public override void OnWarmodeChanged()
        {
            if (!Warmode)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(10), RecoverAmmo);
            }
        }

        private List<Item> m_EquipSnapshot;
        public List<Item> EquipSnapshot => m_EquipSnapshot;

        private bool FindItems_Callback(Item item)
        {
            // Make sure Stackable items are pulled from bags and left on corpse.
            if (!item.Deleted && item.LootType != LootType.Blessed && item.Stackable)
            {
                if (Backpack != item.Parent)
                {
                    return true;
                }
            }

            return false;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override bool Criminal
        {
            get => base.Criminal;
            set
            {
                bool crim = base.Criminal;
                base.Criminal = value;

                if (value != crim)
                {
                    if (value)
                    {
                        BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.CriminalStatus, 1153802, 1153828));
                    }
                    else
                    {
                        BuffInfo.RemoveBuff(this, BuffIcon.CriminalStatus);
                    }
                }
            }
        }

        public override bool OnBeforeDeath()
        {
            NetState state = NetState;

            state?.CancelAllTrades();

            if (Criminal)
            {
                BuffInfo.RemoveBuff(this, BuffIcon.CriminalStatus);
            }

            DropHolding();

            if (Backpack != null && !Backpack.Deleted)
            {
                List<Item> ilist = Backpack.FindItemsByType<Item>(FindItems_Callback);

                for (int i = 0; i < ilist.Count; i++)
                {
                    Backpack.AddItem(ilist[i]);
                }
            }

            m_EquipSnapshot = new List<Item>(Items);

            RecoverAmmo();

            if (NetState != null && NetState.IsEnhancedClient)
            {
                Waypoints.AddCorpse(this);
            }

            return base.OnBeforeDeath();
        }


        // This determines what items remain on you when you die.
        private static bool KeepOnDeath(Item item)
        {
            if (item is MountItem || item is VvVSigil)
            {
                return false;
            }

            if (item.Stackable && item.LootType != LootType.Blessed)
            {
                return false;
            }

            if (item.LootType == LootType.Cursed)
            {
                return false;
            }

            return true;
        }

        public override DeathMoveResult GetParentMoveResultFor(Item item)
        {
            DeathMoveResult res = base.GetParentMoveResultFor(item);

            if (res == DeathMoveResult.MoveToCorpse && item.Movable && KeepOnDeath(item))
            {
                res = DeathMoveResult.MoveToBackpack;
            }

            return res;
        }

        public override DeathMoveResult GetInventoryMoveResultFor(Item item)
        {
            DeathMoveResult res = base.GetInventoryMoveResultFor(item);

            if (res == DeathMoveResult.MoveToCorpse && item.Movable && KeepOnDeath(item))
            {
                res = DeathMoveResult.MoveToBackpack;
            }

            return res;
        }

        public override void OnDeath(Container c)
        {
            if (NetState != null)
            {
                Waypoints.OnDeath(this);
            }

            Mobile m = FindMostRecentDamager(false);
            PlayerMobile killer = m as PlayerMobile;

            if (killer == null && m is BaseCreature bc)
            {
                killer = bc.GetMaster() as PlayerMobile;
            }

            base.OnDeath(c);

            m_EquipSnapshot = null;

            HueMod = -1;
            NameMod = null;

            SetHairMods(-1, -1);

            PolymorphSpell.StopTimer(this);
            IncognitoSpell.StopTimer(this);
            DisguiseTimers.RemoveTimer(this);

            WeakenSpell.RemoveEffects(this);
            ClumsySpell.RemoveEffects(this);
            FeeblemindSpell.RemoveEffects(this);
            CurseSpell.RemoveEffect(this);
            StrangleSpell.RemoveCurse(this);
            Spells.Second.ProtectionSpell.EndProtection(this);

            BaseFishPie.RemoveEffects(this);

            EndAction(typeof(PolymorphSpell));
            EndAction(typeof(IncognitoSpell));

            MeerMage.StopEffect(this, false);

            BaseEscort.DeleteEscort(this);

            if (Flying)
            {
                Flying = false;
                BuffInfo.RemoveBuff(this, BuffIcon.Fly);
            }

            StolenItem.ReturnOnDeath(this, c);

            if (m_PermaFlags.Count > 0)
            {
                m_PermaFlags.Clear();

                if (c is Corpse corpse)
                {
                    corpse.Criminal = true;
                }

                if (Stealing.ClassicMode)
                {
                    Criminal = true;
                }
            }

            Aggression.OnPlayerDeath(this);
            BaseBoat.OnPlayerDeath(this);
            EodonianPotion.OnPlayerDeath(this);
            GemOfSalvation.OnPlayerDeath(this);
            GiftOfLifeSpell.HandleDeath(this);
            KhaldunRevenant.OnPlayerDeath(this);
            ReportMurdererGump.OnPlayerDeath(this);
            ViceVsVirtueSystem.OnPlayerDeath(this);

            if (DateTime.UtcNow >= HalloweenSettings.StartHalloween && DateTime.UtcNow <= HalloweenSettings.FinishHalloween)
            {
                HalloweenHauntings.OnPlayerDeath(this);
            }

            Engines.PartySystem.Party.OnPlayerDeath(this);

            // add back in a death teleport option at a later date using the old young system death tele system
            /*if (Young)
            {
                if (YoungDeathTeleport())
                {
                    Timer.DelayCall(TimeSpan.FromSeconds(2.5), SendYoungDeathNotice);
                }
            }*/

            Guilds.Guild.HandleDeath(this, killer);

            if (m_BuffTable != null)
            {
                List<BuffInfo> list = new List<BuffInfo>();

                foreach (BuffInfo buff in m_BuffTable.Values)
                {
                    if (!buff.RetainThroughDeath)
                    {
                        list.Add(buff);
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    RemoveBuff(list[i]);
                }
            }

            if (Region.IsPartOf("Abyss") && SSSeedExpire > DateTime.UtcNow)
            {
                SendGump(new ResurrectGump(this, ResurrectMessage.SilverSapling));
            }

            if (LastKiller is BaseVoidCreature vc)
            {
                vc.Mutate(VoidEvolution.Killing);
            }
        }

        private List<Mobile> m_PermaFlags;
        private readonly List<Mobile> m_VisList;
        private TimeSpan m_GameTime;
        private TimeSpan m_ShortTermElapse;
        private TimeSpan m_LongTermElapse;
        private DateTime m_SessionStart;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastEscortTime { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastPetBallTime { get; set; }

        public PlayerMobile()
        {
            Instances.Add(this);

            m_AutoStabled = new List<Mobile>();

            m_DoneQuests = new List<QuestRestartInfo>();
            m_Collections = new Dictionary<Collection, int>();
            m_RewardTitles = new List<object>();

            m_VisList = new List<Mobile>();
            m_PermaFlags = new List<Mobile>();
            m_RecentlyReported = new List<Mobile>();

            m_GameTime = TimeSpan.Zero;
            m_ShortTermElapse = TimeSpan.FromHours(8.0);
            m_LongTermElapse = TimeSpan.FromHours(40.0);

            m_GuildRank = RankDefinition.Lowest;

            m_ChampionTitles = new ChampionTitleInfo();
        }

        public override bool MutateSpeech(List<Mobile> hears, ref string text, ref object context)
        {
            if (Alive)
            {
                return false;
            }

            if (Skills[SkillName.SpiritSpeak].Value >= 100.0)
            {
                return false;
            }

            for (int i = 0; i < hears.Count; ++i)
            {
                Mobile m = hears[i];

                if (m != this && m.Skills[SkillName.SpiritSpeak].Value >= 100.0)
                {
                    return false;
                }
            }

            return base.MutateSpeech(hears, ref text, ref context);
        }

        public override void DoSpeech(string text, int[] keywords, MessageType type, int hue)
        {
            if (type == MessageType.Guild || type == MessageType.Alliance)
            {
                Guild g = Guild as Guild;
                if (g == null)
                {
                    SendLocalizedMessage(1063142); // You are not in a guild!
                }
                else if (type == MessageType.Alliance)
                {
                    if (g.Alliance != null && g.Alliance.IsMember(g))
                    {
                        g.Alliance.AllianceChat(this, text);
                        SendToStaffMessage(this, $"[Alliance]: {text}");

                        m_AllianceMessageHue = hue;
                    }
                    else
                    {
                        SendLocalizedMessage(1071020); // You are not in an alliance!
                    }
                }
                else //Type == MessageType.Guild
                {
                    m_GuildMessageHue = hue;

                    g.GuildChat(this, text);
                    SendToStaffMessage(this, $"[Guild]: {text}");
                }
            }
            else
            {
                base.DoSpeech(text, keywords, type, hue);
            }
        }

        private static void SendToStaffMessage(Mobile from, string text)
        {
            Packet p = null;

            foreach (NetState ns in from.GetClientsInRange(8))
            {
                Mobile mob = ns.Mobile;

                if (mob != null && mob.AccessLevel >= AccessLevel.GameMaster && mob.AccessLevel > from.AccessLevel)
                {
                    if (p == null)
                    {
                        p = Packet.Acquire(new UnicodeMessage(from.Serial, from.Body, MessageType.Regular, from.SpeechHue, 3, from.Language, from.Name, text));
                    }

                    ns.Send(p);
                }
            }

            Packet.Release(p);
        }

        private static void SendToStaffMessage(Mobile from, string format, params object[] args)
        {
            SendToStaffMessage(from, string.Format(format, args));
        }

        public override void OnCured(Mobile from, Poison oldPoison)
        {
            BuffInfo.RemoveBuff(this, BuffIcon.Poison);
        }

        public override ApplyPoisonResult ApplyPoison(Mobile from, Poison poison)
        {
            if (!Alive || poison == null)
            {
                return ApplyPoisonResult.Immune;
            }

            //Skill Masteries
            if (ResilienceSpell.UnderEffects(this) && 0.25 > Utility.RandomDouble())
            {
                return ApplyPoisonResult.Immune;
            }

            if (EvilOmenSpell.TryEndEffect(this))
            {
                poison = PoisonImpl.IncreaseLevel(poison);
            }

            //Skill Masteries
            if ((Poison == null || Poison.Level < poison.Level) && ToleranceSpell.OnPoisonApplied(this))
            {
                poison = PoisonImpl.DecreaseLevel(poison);

                if (poison == null || poison.Level <= 0)
                {
                    PrivateOverheadMessage(MessageType.Regular, 0x3F, 1053092, NetState); // * You feel yourself resisting the effects of the poison *
                    return ApplyPoisonResult.Immune;
                }
            }

            ApplyPoisonResult result = base.ApplyPoison(from, poison);

            if (from != null && result == ApplyPoisonResult.Poisoned && PoisonTimer is PoisonImpl.PoisonTimer timer)
            {
                timer.From = from;
            }

            return result;
        }

        public PlayerMobile(Serial s)
            : base(s)
        {
            Instances.Add(this);

            m_VisList = new List<Mobile>();
        }

        public List<Mobile> VisibilityList => m_VisList;

        public List<Mobile> PermaFlags => m_PermaFlags;

        public override int Luck => AosAttributes.GetValue(this, AosAttribute.Luck) + TenthAnniversarySculpture.GetLuckBonus(this) + FountainOfFortune.GetLuckBonus(this);
        public int RealLuck => Luck;

        public override bool IsHarmfulCriminal(IDamageable damageable)
        {
            Mobile target = damageable as Mobile;

            if (Stealing.ClassicMode && target is PlayerMobile pm && pm.m_PermaFlags.Count > 0)
            {
                int noto = Notoriety.Compute(this, target);

                if (noto == Notoriety.Innocent)
                {
                    pm.Delta(MobileDelta.Noto);
                }

                return false;
            }

            if (target is BaseCreature bc && bc.InitialInnocent && !bc.Controlled)
            {
                return false;
            }

            if (target is BaseCreature creature && creature.Controlled && creature.ControlMaster == this)
            {
                return false;
            }

            if (target is BaseCreature baseCreature && baseCreature.Summoned && baseCreature.SummonMaster == this)
            {
                return false;
            }

            return base.IsHarmfulCriminal(damageable);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        _Xp = reader.ReadInt();
                        _Level = reader.ReadInt();
                        _SkillPointsToSpend = reader.ReadInt();
                        _StatPointsToSpend = reader.ReadInt();
                        _CharacterClass = (CharacterClass)reader.ReadInt();

                        NextGemOfSalvationUse = reader.ReadDateTime();

                        RewardStableSlots = reader.ReadInt();

                        DisplayGuildTitle = reader.ReadBool();
                        m_FameKarmaTitle = reader.ReadString();
                        m_PaperdollSkillTitle = reader.ReadString();
                        m_OverheadTitle = reader.ReadString();
                        m_SubtitleSkillTitle = reader.ReadString();

                        m_CurrentChampTitle = reader.ReadString();
                        m_CurrentVeteranTitle = reader.ReadInt();

                        m_SSNextSeed = reader.ReadDateTime();
                        m_SSSeedExpire = reader.ReadDateTime();
                        m_SSSeedLocation = reader.ReadPoint3D();
                        m_SSSeedMap = reader.ReadMap();

                        m_Collections = new Dictionary<Collection, int>();
                        m_RewardTitles = new List<object>();

                        for (int i = reader.ReadInt(); i > 0; i--)
                        {
                            m_Collections.Add((Collection)reader.ReadInt(), reader.ReadInt());
                        }

                        for (int i = reader.ReadInt(); i > 0; i--)
                        {
                            m_RewardTitles.Add(QuestReader.Object(reader));
                        }

                        m_SelectedTitle = reader.ReadInt();

                        m_AnkhNextUse = reader.ReadDateTime();

                        m_AutoStabled = reader.ReadStrongMobileList();

                        int recipeCount = reader.ReadInt();
                        if (recipeCount > 0)
                        {
                            m_AcquiredRecipes = new Dictionary<int, bool>();

                            for (int i = 0; i < recipeCount; i++)
                            {
                                int r = reader.ReadInt();
                                if (reader.ReadBool()) //Don't add in recipies which we haven't gotten or have been removed
                                {
                                    m_AcquiredRecipes.Add(r, true);
                                }
                            }
                        }

                        m_ChampionTitles = new ChampionTitleInfo(reader);

                        m_AllianceMessageHue = reader.ReadEncodedInt();
                        m_GuildMessageHue = reader.ReadEncodedInt();

                        int rank = reader.ReadEncodedInt();
                        int maxRank = RankDefinition.Ranks.Length - 1;
                        if (rank > maxRank)
                        {
                            rank = maxRank;
                        }

                        m_GuildRank = RankDefinition.Ranks[rank];
                        m_LastOnline = reader.ReadDateTime();

                        int count = reader.ReadEncodedInt();
                        if (count > 0)
                        {
                            m_DoneQuests = new List<QuestRestartInfo>();

                            for (int i = 0; i < count; ++i)
                            {
                                Type questType = reader.ReadObjectType();
                                DateTime restartTime = reader.ReadDateTime();

                                m_DoneQuests.Add(new QuestRestartInfo(questType, restartTime));
                            }
                        }

                        // hair mod
                        if (reader.ReadBool())
                        {
                            m_HairModID = reader.ReadInt();
                            m_HairModHue = reader.ReadInt();
                            m_BeardModID = reader.ReadInt();
                            m_BeardModHue = reader.ReadInt();
                        }

                        m_NpcGuild = (NpcGuild)reader.ReadInt();
                        m_NpcGuildJoinTime = reader.ReadDateTime();
                        m_NpcGuildGameTime = reader.ReadTimeSpan();

                        m_PermaFlags = reader.ReadStrongMobileList();

                        m_Flags = (PlayerFlag)reader.ReadInt();

                        m_LongTermElapse = reader.ReadTimeSpan();
                        m_ShortTermElapse = reader.ReadTimeSpan();
                        m_GameTime = reader.ReadTimeSpan();

                        break;
                    }
            }

            if (m_RecentlyReported == null)
            {
                m_RecentlyReported = new List<Mobile>();
            }

            if (m_DoneQuests == null)
            {
                m_DoneQuests = new List<QuestRestartInfo>();
            }

            if (m_Collections == null)
            {
                m_Collections = new Dictionary<Collection, int>();
            }

            if (m_RewardTitles == null)
            {
                m_RewardTitles = new List<object>();
            }

            if (m_PermaFlags == null)
            {
                m_PermaFlags = new List<Mobile>();
            }

            if (m_GuildRank == null)
            {
                m_GuildRank = RankDefinition.Member;
                //Default to member if going from older version to new version (only time it should be null)
            }

            if (m_LastOnline == DateTime.MinValue && Account != null)
            {
                m_LastOnline = ((Account)Account).LastLogin;
            }

            if (m_ChampionTitles == null)
            {
                m_ChampionTitles = new ChampionTitleInfo();
            }

            List<Mobile> list = Stabled;

            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] is BaseCreature bc)
                {
                    bc.IsStabled = true;
                    bc.StabledBy = this;
                }
            }

            CheckAtrophies(this);

            if (Hidden) //Hiding is the only buff where it has an effect that's serialized.
            {
                AddBuff(new BuffInfo(BuffIcon.HidingAndOrStealth, 1075655));
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            CheckKillDecay();
            CheckAtrophies(this);

            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(_Xp);
            writer.Write(_Level);
            writer.Write(_SkillPointsToSpend);
            writer.Write(_StatPointsToSpend);
            writer.Write((int)_CharacterClass);

            writer.Write(NextGemOfSalvationUse);

            writer.Write(RewardStableSlots);

            writer.Write(DisplayGuildTitle);
            writer.Write(m_FameKarmaTitle);
            writer.Write(m_PaperdollSkillTitle);
            writer.Write(m_OverheadTitle);
            writer.Write(m_SubtitleSkillTitle);
            writer.Write(m_CurrentChampTitle);
            writer.Write(m_CurrentVeteranTitle);

            writer.Write(m_SSNextSeed);
            writer.Write(m_SSSeedExpire);
            writer.Write(m_SSSeedLocation);
            writer.Write(m_SSSeedMap);

            if (m_Collections == null)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(m_Collections.Count);

                foreach (KeyValuePair<Collection, int> pair in m_Collections)
                {
                    writer.Write((int)pair.Key);
                    writer.Write(pair.Value);
                }
            }

            if (m_RewardTitles == null)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(m_RewardTitles.Count);

                for (int i = 0; i < m_RewardTitles.Count; i++)
                {
                    QuestWriter.Object(writer, m_RewardTitles[i]);
                }
            }

            writer.Write(m_SelectedTitle);

            writer.Write(m_AnkhNextUse);
            writer.Write(m_AutoStabled, true);

            if (m_AcquiredRecipes == null)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(m_AcquiredRecipes.Count);

                foreach (KeyValuePair<int, bool> kvp in m_AcquiredRecipes)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }
            }

            ChampionTitleInfo.Serialize(writer, m_ChampionTitles);

            writer.WriteEncodedInt(m_AllianceMessageHue);
            writer.WriteEncodedInt(m_GuildMessageHue);

            writer.WriteEncodedInt(m_GuildRank.Rank);
            writer.Write(m_LastOnline);

            if (m_DoneQuests == null)
            {
                writer.WriteEncodedInt(0);
            }
            else
            {
                writer.WriteEncodedInt(m_DoneQuests.Count);

                for (int i = 0; i < m_DoneQuests.Count; ++i)
                {
                    QuestRestartInfo restartInfo = m_DoneQuests[i];

                    writer.WriteObjectType(restartInfo.QuestType);
                    writer.Write(restartInfo.RestartTime);
                }
            }

            bool useMods = m_HairModID != -1 || m_BeardModID != -1;
            writer.Write(useMods);
            if (useMods)
            {
                writer.Write(m_HairModID);
                writer.Write(m_HairModHue);
                writer.Write(m_BeardModID);
                writer.Write(m_BeardModHue);
            }

            writer.Write((int)m_NpcGuild);
            writer.Write(m_NpcGuildJoinTime);
            writer.Write(m_NpcGuildGameTime);

            writer.Write(m_PermaFlags, true);

            writer.Write((int)m_Flags);

            writer.Write(m_LongTermElapse);
            writer.Write(m_ShortTermElapse);
            writer.Write(GameTime);
        }

        public static void CheckAtrophies(PlayerMobile pm)
        {
            ChampionTitleInfo.CheckAtrophy(pm);
        }

        public void CheckKillDecay()
        {
            if (m_ShortTermElapse < GameTime)
            {
                m_ShortTermElapse += TimeSpan.FromHours(8);
                if (ShortTermMurders > 0)
                {
                    --ShortTermMurders;
                }
            }

            if (m_LongTermElapse < GameTime)
            {
                m_LongTermElapse += TimeSpan.FromHours(40);
                if (Kills > 0)
                {
                    --Kills;
                }
            }
        }

        public void ResetKillTime()
        {
            m_ShortTermElapse = GameTime + TimeSpan.FromHours(8);
            m_LongTermElapse = GameTime + TimeSpan.FromHours(40);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime SessionStart => m_SessionStart;

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan GameTime
        {
            get
            {
                if (NetState != null)
                {
                    return m_GameTime + (DateTime.UtcNow - m_SessionStart);
                }

                return m_GameTime;
            }
        }

        public override bool CanSee(Mobile m)
        {
            if (m is IConditionalVisibility && !((IConditionalVisibility)m).CanBeSeenBy(this))
            {
                return false;
            }

            if (m is CharacterStatue statue)
            {
                statue.OnRequestedAnimation(this);
            }

            if (m is PlayerMobile pm && pm.m_VisList.Contains(this))
            {
                return true;
            }

            return base.CanSee(m);
        }

        public override bool CanSee(Item item)
        {
            if (item is IConditionalVisibility vis && !vis.CanBeSeenBy(this))
            {
                return false;
            }

            if (m_DesignContext != null && m_DesignContext.Foundation.IsHiddenToCustomizer(this, item))
            {
                return false;
            }

            if (AccessLevel == AccessLevel.Player)
            {
                Region r = item.GetRegion();

                if (r is BaseRegion region && !region.CanSee(this, item))
                {
                    return false;
                }
            }

            return base.CanSee(item);
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            Instances.Remove(this);

            BaseHouse.HandleDeletion(this);

            DisguiseTimers.RemoveTimer(this);
        }

        public delegate void PlayerPropertiesEventHandler(PlayerPropertiesEventArgs e);

        public static event PlayerPropertiesEventHandler PlayerProperties;

        public sealed class PlayerPropertiesEventArgs(PlayerMobile player, ObjectPropertyList list) : EventArgs
        {
            public PlayerMobile Player = player;
            public ObjectPropertyList PropertyList = list;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            // Only display Class and Level if a player.
            if (AccessLevel < AccessLevel.Counselor)
            {
                list.Add(1060658, $"Class\t{ClassSystemHelper.GetClassName(_CharacterClass)}"); // ~1_val~: ~2_val~
                list.Add(1060659, $"Level\t{Level}"); // ~1_val~: ~2_val~
            }
            else if (AccessLevel > AccessLevel.Counselor)
            {
                list.Add(1018085); // Game Master
            }

            Engines.JollyRoger.JollyRogerData.DisplayTitle(this, list);

            if (m_SubtitleSkillTitle != null)
            {
                list.Add(1042971, m_SubtitleSkillTitle);
            }

            if (m_CurrentVeteranTitle > 0)
            {
                list.Add(m_CurrentVeteranTitle);
            }

            if (m_RewardTitles != null && m_SelectedTitle > -1)
            {
                if (m_SelectedTitle < m_RewardTitles.Count)
                {
                    if (m_RewardTitles[m_SelectedTitle] is int)
                    {
                        string customTitle;

                        if ((int)m_RewardTitles[m_SelectedTitle] == 1154017 && CityLoyaltySystem.HasCustomTitle(this, out customTitle))
                        {
                            list.Add(1154017, customTitle); // ~1_TITLE~ of ~2_CITY~
                        }
                        else
                        {
                            list.Add((int)m_RewardTitles[m_SelectedTitle]);
                        }
                    }
                    else if (m_RewardTitles[m_SelectedTitle] is string)
                    {
                        list.Add(1070722, (string)m_RewardTitles[m_SelectedTitle]);
                    }
                }
            }

            for (int i = AllFollowers.Count - 1; i >= 0; i--)
            {
                if (AllFollowers[i] is BaseCreature c && c.GuardMode == GuardType.Active)
                {
                    list.Add(501129); // guarded
                    break;
                }
            }

            if (TestCenter.Enabled)
            {
                VvVPlayerEntry entry = PointsSystem.ViceVsVirtue.GetPlayerEntry<VvVPlayerEntry>(this);

                list.Add($"Kills: {(entry == null ? "0" : entry.Kills.ToString())} / Deaths: {(entry == null ? "0" : entry.Deaths.ToString())} / Assists: {(entry == null ? "0" : entry.Assists.ToString())}");

                list.Add(1060415, AosAttributes.GetValue(this, AosAttribute.AttackChance).ToString()); // hit chance increase ~1_val~%
                list.Add(1060408, AosAttributes.GetValue(this, AosAttribute.DefendChance).ToString()); // defense chance increase ~1_val~%
                list.Add(1060486, AosAttributes.GetValue(this, AosAttribute.WeaponSpeed).ToString()); // swing speed increase ~1_val~%
                list.Add(1060401, AosAttributes.GetValue(this, AosAttribute.WeaponDamage).ToString()); // damage increase ~1_val~%
                list.Add(1060483, AosAttributes.GetValue(this, AosAttribute.SpellDamage).ToString()); // spell damage increase ~1_val~%
                list.Add(1060433, AosAttributes.GetValue(this, AosAttribute.LowerManaCost).ToString()); // lower mana cost
            }

            PlayerProperties?.Invoke(new PlayerPropertiesEventArgs(this, list));
        }

        protected override bool OnMove(Direction d)
        {
            if (Party != null && NetState != null)
            {
                Waypoints.UpdateToParty(this);
            }

            if (IsStaff())
            {
                return true;
            }

            if (Hidden && DesignContext.Find(this) == null) //Hidden & NOT customizing a house
            {
                if (!Mounted && Skills.Stealth.Value >= 25.0)
                {
                    bool running = (d & Direction.Running) != 0;

                    if (running)
                    {
                        if ((AllowedStealthSteps -= 2) <= 0)
                        {
                            RevealingAction();
                        }
                    }
                    else if (AllowedStealthSteps-- <= 0)
                    {
                        Stealth.OnUse(this);
                    }
                }
                else
                {
                    RevealingAction();
                }
            }

            if (InvisibilityPotion.HasTimer(this))
            {
                InvisibilityPotion.Iterrupt(this);
            }

            return true;
        }

        public bool BedrollLogout { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public override bool Paralyzed
        {
            get => base.Paralyzed;
            set
            {
                base.Paralyzed = value;

                if (value)
                {
                    AddBuff(new BuffInfo(BuffIcon.Paralyze, 1075827)); //Paralyze/You are frozen and can not move
                }
                else
                {
                    RemoveBuff(BuffIcon.Paralyze);
                }
            }
        }

        private List<QuestRestartInfo> m_DoneQuests;
        public List<QuestRestartInfo> DoneQuests { get => m_DoneQuests; set => m_DoneQuests = value; }

        public List<BaseQuest> Quests => MondainQuestData.GetQuests(this);
        public Dictionary<QuestChain, BaseChain> Chains => MondainQuestData.GetChains(this);

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Peaced => PeacedUntil > DateTime.UtcNow;

        private Dictionary<Collection, int> m_Collections;
        private List<object> m_RewardTitles;
        private int m_SelectedTitle;

        public Dictionary<Collection, int> Collections => m_Collections;

        public List<object> RewardTitles => m_RewardTitles;

        public int SelectedTitle => m_SelectedTitle;

        public bool RemoveRewardTitle(object o, bool silent)
        {
            if (m_RewardTitles.Contains(o))
            {
                int i = m_RewardTitles.IndexOf(o);

                if (i == m_SelectedTitle)
                {
                    SelectRewardTitle(-1, silent);
                }
                else if (i > m_SelectedTitle)
                {
                    SelectRewardTitle(m_SelectedTitle - 1, silent);
                }

                m_RewardTitles.Remove(o);

                return true;
            }

            return false;
        }

        public int GetCollectionPoints(Collection collection)
        {
            if (m_Collections == null)
            {
                m_Collections = new Dictionary<Collection, int>();
            }

            int points = 0;

            if (m_Collections.ContainsKey(collection))
            {
                m_Collections.TryGetValue(collection, out points);
            }

            return points;
        }

        public void AddCollectionPoints(Collection collection, int points)
        {
            if (m_Collections == null)
            {
                m_Collections = new Dictionary<Collection, int>();
            }

            if (!m_Collections.TryAdd(collection, points))
            {
                m_Collections[collection] += points;
            }
        }

        public void SelectRewardTitle(int num, bool silent = false)
        {
            if (num == -1)
            {
                m_SelectedTitle = num;

                if (!silent)
                {
                    SendLocalizedMessage(1074010); // You elect to hide your Reward Title.
                }
            }
            else if (num < m_RewardTitles.Count && num >= -1)
            {
                if (m_SelectedTitle != num)
                {
                    m_SelectedTitle = num;

                    if (m_RewardTitles[num] is int && !silent)
                    {
                        SendLocalizedMessage(1074008, "#" + (int)m_RewardTitles[num]);
                        // You change your Reward Title to "~1_TITLE~".
                    }
                    else if (m_RewardTitles[num] is string && !silent)
                    {
                        SendLocalizedMessage(1074008, (string)m_RewardTitles[num]); // You change your Reward Title to "~1_TITLE~".
                    }
                }
                else if (!silent)
                {
                    SendLocalizedMessage(1074009); // You decide to leave your title as it is.
                }
            }

            InvalidateProperties();
        }

        public bool AddRewardTitle(object title)
        {
            if (m_RewardTitles == null)
            {
                m_RewardTitles = new List<object>();
            }

            if (title != null && !m_RewardTitles.Contains(title))
            {
                m_RewardTitles.Add(title);

                InvalidateProperties();
                return true;
            }

            return false;
        }

        public void ShowChangeTitle()
        {
            SendGump(new SelectTitleGump(this, m_SelectedTitle));
        }

        private string m_FameKarmaTitle;
        private string m_PaperdollSkillTitle;
        private string m_SubtitleSkillTitle;
        private string m_CurrentChampTitle;
        private string m_OverheadTitle;
        private int m_CurrentVeteranTitle;

        public string FameKarmaTitle
        {
            get => m_FameKarmaTitle;
            set { m_FameKarmaTitle = value; InvalidateProperties(); }
        }

        public string PaperdollSkillTitle
        {
            get => m_PaperdollSkillTitle;
            set { m_PaperdollSkillTitle = value; InvalidateProperties(); }
        }

        public string SubtitleSkillTitle
        {
            get => m_SubtitleSkillTitle;
            set { m_SubtitleSkillTitle = value; InvalidateProperties(); }
        }

        public string CurrentChampTitle
        {
            get => m_CurrentChampTitle;
            set { m_CurrentChampTitle = value; InvalidateProperties(); }
        }

        public string OverheadTitle
        {
            get => m_OverheadTitle;
            set { m_OverheadTitle = value; InvalidateProperties(); }
        }

        public int CurrentVeteranTitle
        {
            get => m_CurrentVeteranTitle;
            set { m_CurrentVeteranTitle = value; InvalidateProperties(); }
        }

        public override bool ShowAccessTitle
        {
            get
            {
                switch (AccessLevel)
                {
                    case AccessLevel.VIP:
                    case AccessLevel.Counselor:
                    case AccessLevel.GameMaster:
                    case AccessLevel.Seer:
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            string prefix = "";

            if (ShowFameTitle && Fame >= 10000)
            {
                prefix = Female ? "Lady" : "Lord";
            }

            string suffix = "";

            if (PropertyTitle && !string.IsNullOrEmpty(Title))
            {
                suffix = Title;
            }

            BaseGuild guild = Guild;
            bool vvv = ViceVsVirtueSystem.IsVvV(this) && (ViceVsVirtueSystem.EnhancedRules || Map == ViceVsVirtueSystem.Facet);

            if (m_OverheadTitle != null)
            {
                if (vvv)
                {
                    suffix = "[VvV]";
                }
                else
                {
                    int loc = Utility.ToInt32(m_OverheadTitle);

                    if (loc > 0)
                    {
                        if (CityLoyaltySystem.ApplyCityTitle(this, list, prefix, loc))
                        {
                            return;
                        }
                    }
                    else if (suffix.Length > 0)
                    {
                        suffix = $"{suffix} {m_OverheadTitle}";
                    }
                    else
                    {
                        suffix = $"{m_OverheadTitle}";
                    }
                }
            }
            else if (guild != null && DisplayGuildAbbr)
            {
                if (vvv)
                {
                    suffix = $"[{Utility.FixHtml(guild.Abbreviation)}] [VvV]";
                }
                else if (suffix.Length > 0)
                {
                    suffix = $"{suffix} [{Utility.FixHtml(guild.Abbreviation)}]";
                }
                else
                {
                    suffix = $"[{Utility.FixHtml(guild.Abbreviation)}]";
                }
            }
            else if (vvv)
            {
                suffix = "[VvV]";
            }

            suffix = ApplyNameSuffix(suffix);
            string name = Name;

            list.Add(1050045, $"{prefix} \t{name}\t {suffix}"); // ~1_PREFIX~~2_NAME~~3_SUFFIX~

            if (guild != null && DisplayGuildTitle)
            {
                string title = GuildTitle;

                title = title == null ? "" : title.Trim();

                if (title.Length > 0)
                {
                    list.Add(1060776, $"{Utility.FixHtml(title)}\t{Utility.FixHtml(guild.Name)}"); // ~1_val~, ~2_val~
                }
            }
        }

        public override void OnAfterNameChange(string oldName, string newName)
        {
            if (m_FameKarmaTitle != null)
            {
                FameKarmaTitle = FameKarmaTitle.Replace(oldName, newName);
            }
        }

        public override void OnKillsChange(int oldValue)
        {
            if (Kills > oldValue)
            {
                this.SendMessage("No equipment protection for Murderers");
            }
        }

        public override void OnKarmaChange(int oldValue)
        {
            EpiphanyHelper.OnKarmaChange(this);
        }

        public override void OnSkillChange(SkillName skill, double oldBase)
        {
            if (skill != SkillName.Alchemy && Skills.CurrentMastery == skill && Skills[skill].Value < MasteryInfo.MinSkillRequirement)
            {
                SkillName mastery = Skills.CurrentMastery;
                Skills.CurrentMastery = SkillName.Alchemy;

                MasteryInfo.OnMasteryChanged(this, mastery);
            }

            TransformContext context = TransformationSpellHelper.GetContext(this);

            if (context != null)
            {
                TransformationSpellHelper.CheckCastSkill(this, context);
            }
        }

        public override void OnAccessLevelChanged(AccessLevel oldLevel)
        {
            IgnoreMobiles = !IsPlayer() || TransformationSpellHelper.UnderTransformation(this, typeof(WraithFormSpell));
        }

        public override void OnDelete()
        {
            Instances.Remove(this);
        }

        private const bool FastwalkPrevention = true; // Is fastwalk prevention enabled?
        private const int FastwalkThreshold = 400; // Fastwalk prevention will become active after 0.4 seconds

        private long m_NextMovementTime;
        private bool m_HasMoved;

        public long NextMovementTime => m_NextMovementTime;

        public virtual bool UsesFastwalkPrevention => IsPlayer();

        public override int ComputeMovementSpeed(Direction dir, bool checkTurning)
        {
            if (checkTurning && (dir & Direction.Mask) != (Direction & Direction.Mask))
            {
                return RunMount; // We are NOT actually moving (just a direction change)
            }

            bool running = (dir & Direction.Running) != 0;

            bool onHorse = Mount != null || Flying;

            AnimalFormContext animalContext = AnimalForm.GetContext(this);

            if (onHorse || animalContext != null && animalContext.SpeedBoost)
            {
                return running ? RunMount : WalkMount;
            }

            return running ? RunFoot : WalkFoot;
        }

        public static bool MovementThrottle_Callback(byte packetID, NetState ns, out bool drop)
        {
            drop = false;

            PlayerMobile pm = ns.Mobile as PlayerMobile;

            if (pm == null || !pm.UsesFastwalkPrevention)
            {
                return true;
            }

            if (!pm.m_HasMoved)
            {
                // has not yet moved
                pm.m_NextMovementTime = Core.TickCount;
                pm.m_HasMoved = true;
                return true;
            }

            long ts = pm.m_NextMovementTime - Core.TickCount;

            if (ts < 0)
            {
                // been a while since we've last moved
                pm.m_NextMovementTime = Core.TickCount;
                return true;
            }

            return ts < FastwalkThreshold;
        }

        private int m_HairModID = -1, m_HairModHue;
        private int m_BeardModID = -1, m_BeardModHue;

        public void SetHairMods(int hairID, int beardID)
        {
            if (hairID == -1)
            {
                InternalRestoreHair(true, ref m_HairModID, ref m_HairModHue);
            }
            else if (hairID != -2)
            {
                InternalChangeHair(true, hairID, ref m_HairModID, ref m_HairModHue);
            }

            if (beardID == -1)
            {
                InternalRestoreHair(false, ref m_BeardModID, ref m_BeardModHue);
            }
            else if (beardID != -2)
            {
                InternalChangeHair(false, beardID, ref m_BeardModID, ref m_BeardModHue);
            }
        }

        private void CreateHair(bool hair, int id, int hue)
        {
            if (hair)
            {
                //TODO Verification?
                HairItemID = id;
                HairHue = hue;
            }
            else
            {
                FacialHairItemID = id;
                FacialHairHue = hue;
            }
        }

        private void InternalRestoreHair(bool hair, ref int id, ref int hue)
        {
            if (id == -1)
            {
                return;
            }

            if (hair)
            {
                HairItemID = 0;
            }
            else
            {
                FacialHairItemID = 0;
            }

            //if( id != 0 )
            CreateHair(hair, id, hue);

            id = -1;
            hue = 0;
        }

        private void InternalChangeHair(bool hair, int id, ref int storeID, ref int storeHue)
        {
            if (storeID == -1)
            {
                storeID = hair ? HairItemID : FacialHairItemID;
                storeHue = hair ? HairHue : FacialHairHue;
            }
            CreateHair(hair, id, 0);
        }

        public override TimeSpan GetLogoutDelay()
        {
            if (BedrollLogout || TestCenter.Enabled)
            {
                return TimeSpan.Zero;
            }

            return base.GetLogoutDelay();
        }

        private DateTime _LastNpcHealTime = DateTime.MinValue;

        public bool CheckNpcHealTime()
        {
            if (DateTime.UtcNow - _LastNpcHealTime > TimeSpan.FromMinutes(5.0))
            {
                _LastNpcHealTime = DateTime.UtcNow;
                return true;
            }

            return false;
        }

        private static readonly Point3D[] m_TrammelDeathDestinations =
        {
            new Point3D(1481, 1612, 20), new Point3D(2708, 2153, 0), new Point3D(2249, 1230, 0), new Point3D(5197, 3994, 37),
            new Point3D(1412, 3793, 0), new Point3D(3688, 2232, 20), new Point3D(2578, 604, 0), new Point3D(4397, 1089, 0),
            new Point3D(5741, 3218, -2), new Point3D(2996, 3441, 15), new Point3D(624, 2225, 0), new Point3D(1916, 2814, 0),
            new Point3D(2929, 854, 0), new Point3D(545, 967, 0), new Point3D(3469, 2559, 36)
        };

        public bool YoungDeathTeleport()
        {
            if (Region.IsPartOf<Jail>() || Region.IsPartOf("Ninja start location"))
            {
                return false;
            }

            Point3D loc;
            Map map;

            DungeonRegion dungeon = (DungeonRegion)Region.GetRegion(typeof(DungeonRegion));
            if (dungeon != null && dungeon.EntranceLocation != Point3D.Zero)
            {
                loc = dungeon.EntranceLocation;
                map = dungeon.EntranceMap;
            }
            else
            {
                loc = Location;
                map = Map;
            }

            Point3D[] list;

            if (map == Map.Trammel)
            {
                list = m_TrammelDeathDestinations;
            }
            else
            {
                return false;
            }

            Point3D dest = Point3D.Zero;
            int sqDistance = int.MaxValue;

            for (int i = 0; i < list.Length; i++)
            {
                Point3D curDest = list[i];

                int width = loc.X - curDest.X;
                int height = loc.Y - curDest.Y;
                int curSqDistance = width * width + height * height;

                if (curSqDistance < sqDistance)
                {
                    dest = curDest;
                    sqDistance = curSqDistance;
                }
            }

            MoveToWorld(dest, map);
            return true;
        }

        private void SendYoungDeathNotice()
        {
            SendGump(new YoungDeathNotice());
        }

        private bool m_TempSquelched;

        [CommandProperty(AccessLevel.Administrator)]
        public bool TempSquelched { get => m_TempSquelched; set => m_TempSquelched = value; }

        public override void OnSaid(SpeechEventArgs e)
        {
            if (m_TempSquelched)
            {
                SendLocalizedMessage(500168); // You can not say anything, you have been muted.
                e.Blocked = true;
            }
            else
            {
                base.OnSaid(e);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DisplayChampionTitle { get => GetFlag(PlayerFlag.DisplayChampionTitle); set => SetFlag(PlayerFlag.DisplayChampionTitle, value); }

        private ChampionTitleInfo m_ChampionTitles;

        [CommandProperty(AccessLevel.GameMaster)]
        public ChampionTitleInfo ChampionTitles { get => m_ChampionTitles; set { } }

        [PropertyObject]
        public class ChampionTitleInfo
        {
            public static TimeSpan LossDelay = TimeSpan.FromDays(1.0);
            public const int LossAmount = 90;

            private class TitleInfo
            {
                private int m_Value;
                private DateTime m_LastDecay;

                public int Value { get => m_Value; set => m_Value = value; }
                public DateTime LastDecay { get => m_LastDecay; set => m_LastDecay = value; }

                public TitleInfo()
                { }

                public TitleInfo(GenericReader reader)
                {
                    int version = reader.ReadEncodedInt();

                    switch (version)
                    {
                        case 0:
                            {
                                m_Value = reader.ReadEncodedInt();
                                m_LastDecay = reader.ReadDateTime();
                                break;
                            }
                    }
                }

                public static void Serialize(GenericWriter writer, TitleInfo info)
                {
                    writer.WriteEncodedInt(0); // version

                    writer.WriteEncodedInt(info.m_Value);
                    writer.Write(info.m_LastDecay);
                }
            }

            private TitleInfo[] m_Values;

            private int m_Harrower; //Harrower titles do NOT decay

            public int GetValue(ChampionSpawnType type)
            {
                return GetValue((int)type);
            }

            public void SetValue(ChampionSpawnType type, int value)
            {
                SetValue((int)type, value);
            }

            public void Award(ChampionSpawnType type, int value)
            {
                Award((int)type, value);
            }

            public int GetValue(int index)
            {
                if (m_Values == null || index < 0 || index >= m_Values.Length)
                {
                    return 0;
                }

                if (m_Values[index] == null)
                {
                    m_Values[index] = new TitleInfo();
                }

                return m_Values[index].Value;
            }

            public DateTime GetLastDecay(int index)
            {
                if (m_Values == null || index < 0 || index >= m_Values.Length)
                {
                    return DateTime.MinValue;
                }

                if (m_Values[index] == null)
                {
                    m_Values[index] = new TitleInfo();
                }

                return m_Values[index].LastDecay;
            }

            public void SetValue(int index, int value)
            {
                if (m_Values == null)
                {
                    m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
                }

                if (value < 0)
                {
                    value = 0;
                }

                if (index < 0 || index >= m_Values.Length)
                {
                    return;
                }

                if (m_Values[index] == null)
                {
                    m_Values[index] = new TitleInfo();
                }

                m_Values[index].Value = value;
            }

            public void Award(int index, int value)
            {
                if (m_Values == null)
                {
                    m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
                }

                if (index < 0 || index >= m_Values.Length || value <= 0)
                {
                    return;
                }

                if (m_Values[index] == null)
                {
                    m_Values[index] = new TitleInfo();
                }

                m_Values[index].Value += value;
            }

            public void Atrophy(int index, int value)
            {
                if (m_Values == null)
                {
                    m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
                }

                if (index < 0 || index >= m_Values.Length || value <= 0)
                {
                    return;
                }

                if (m_Values[index] == null)
                {
                    m_Values[index] = new TitleInfo();
                }

                int before = m_Values[index].Value;

                if (m_Values[index].Value - value < 0)
                {
                    m_Values[index].Value = 0;
                }
                else
                {
                    m_Values[index].Value -= value;
                }

                if (before != m_Values[index].Value)
                {
                    m_Values[index].LastDecay = DateTime.UtcNow;
                }
            }

            public override string ToString()
            {
                return "...";
            }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Abyss { get => GetValue(ChampionSpawnType.Abyss); set => SetValue(ChampionSpawnType.Abyss, value); }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Arachnid { get => GetValue(ChampionSpawnType.Arachnid); set => SetValue(ChampionSpawnType.Arachnid, value); }

            [CommandProperty(AccessLevel.GameMaster)]
            public int ColdBlood { get => GetValue(ChampionSpawnType.ColdBlood); set => SetValue(ChampionSpawnType.ColdBlood, value); }

            [CommandProperty(AccessLevel.GameMaster)]
            public int ForestLord { get => GetValue(ChampionSpawnType.ForestLord); set => SetValue(ChampionSpawnType.ForestLord, value); }

            [CommandProperty(AccessLevel.GameMaster)]
            public int SleepingDragon { get => GetValue(ChampionSpawnType.SleepingDragon); set => SetValue(ChampionSpawnType.SleepingDragon, value); }

            [CommandProperty(AccessLevel.GameMaster)]
            public int UnholyTerror { get => GetValue(ChampionSpawnType.UnholyTerror); set => SetValue(ChampionSpawnType.UnholyTerror, value); }

            [CommandProperty(AccessLevel.GameMaster)]
            public int VerminHorde { get => GetValue(ChampionSpawnType.VerminHorde); set => SetValue(ChampionSpawnType.VerminHorde, value); }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Harrower { get => m_Harrower; set => m_Harrower = value; }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Glade { get => GetValue(ChampionSpawnType.Glade); set => SetValue(ChampionSpawnType.Glade, value); }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Corrupt { get => GetValue(ChampionSpawnType.Corrupt); set => SetValue(ChampionSpawnType.Corrupt, value); }

            public ChampionTitleInfo()
            { }

            public ChampionTitleInfo(GenericReader reader)
            {
                int version = reader.ReadEncodedInt();

                switch (version)
                {
                    case 0:
                        {
                            m_Harrower = reader.ReadEncodedInt();

                            int length = reader.ReadEncodedInt();
                            m_Values = new TitleInfo[length];

                            for (int i = 0; i < length; i++)
                            {
                                m_Values[i] = new TitleInfo(reader);
                            }

                            if (m_Values.Length != ChampionSpawnInfo.Table.Length)
                            {
                                TitleInfo[] oldValues = m_Values;
                                m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];

                                for (int i = 0; i < m_Values.Length && i < oldValues.Length; i++)
                                {
                                    m_Values[i] = oldValues[i];
                                }
                            }
                            break;
                        }
                }
            }

            public static void Serialize(GenericWriter writer, ChampionTitleInfo titles)
            {
                writer.WriteEncodedInt(0); // version

                writer.WriteEncodedInt(titles.m_Harrower);

                int length = titles.m_Values.Length;
                writer.WriteEncodedInt(length);

                for (int i = 0; i < length; i++)
                {
                    if (titles.m_Values[i] == null)
                    {
                        titles.m_Values[i] = new TitleInfo();
                    }

                    TitleInfo.Serialize(writer, titles.m_Values[i]);
                }
            }

            public static void CheckAtrophy(PlayerMobile pm)
            {
                ChampionTitleInfo t = pm.m_ChampionTitles;
                if (t == null)
                {
                    return;
                }

                if (t.m_Values == null)
                {
                    t.m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
                }

                for (int i = 0; i < t.m_Values.Length; i++)
                {
                    if (t.GetLastDecay(i) + LossDelay < DateTime.UtcNow)
                    {
                        t.Atrophy(i, LossAmount);
                    }
                }
            }

            public static void AwardHarrowerTitle(PlayerMobile pm)
            //Called when killing a harrower.  Will give a minimum of 1 point.
            {
                ChampionTitleInfo t = pm.m_ChampionTitles;
                if (t == null)
                {
                    return;
                }

                if (t.m_Values == null)
                {
                    t.m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
                }

                int count = 1;

                for (int i = 0; i < t.m_Values.Length; i++)
                {
                    if (t.m_Values[i].Value > 900)
                    {
                        count++;
                    }
                }

                t.m_Harrower = Math.Max(count, t.m_Harrower); //Harrower titles never decay.
            }

            public bool HasChampionTitle(PlayerMobile pm)
            {
                if (m_Harrower > 0)
                {
                    return true;
                }

                if (m_Values == null)
                {
                    return false;
                }

                for (int index = 0; index < m_Values.Length; index++)
                {
                    TitleInfo info = m_Values[index];

                    if (info.Value > 300)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private Dictionary<int, bool> m_AcquiredRecipes;

        public virtual bool HasRecipe(Recipe r)
        {
            if (r == null)
            {
                return false;
            }

            return HasRecipe(r.ID);
        }

        public virtual bool HasRecipe(int recipeID)
        {
            if (m_AcquiredRecipes != null && m_AcquiredRecipes.TryGetValue(recipeID, out bool value))
            {
                return value;
            }

            return false;
        }

        public virtual void AcquireRecipe(Recipe r)
        {
            if (r != null)
            {
                AcquireRecipe(r.ID);
            }
        }

        public virtual void AcquireRecipe(int recipeID)
        {
            if (m_AcquiredRecipes == null)
            {
                m_AcquiredRecipes = new Dictionary<int, bool>();
            }

            m_AcquiredRecipes[recipeID] = true;
        }

        public virtual void ResetRecipes()
        {
            m_AcquiredRecipes = null;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int KnownRecipes
        {
            get
            {
                if (m_AcquiredRecipes == null)
                {
                    return 0;
                }

                return m_AcquiredRecipes.Count;
            }
        }

        public void ResendBuffs()
        {
            if (m_BuffTable == null)
            {
                return;
            }

            NetState state = NetState;

            if (state != null)
            {
                foreach (BuffInfo info in m_BuffTable.Values)
                {
                    state.Send(new AddBuffPacket(this, info));
                }
            }
        }

        private Dictionary<BuffIcon, BuffInfo> m_BuffTable;

        public void AddBuff(BuffInfo b)
        {
            if (b == null)
            {
                return;
            }

            RemoveBuff(b); //Check & subsequently remove the old one.

            if (m_BuffTable == null)
            {
                m_BuffTable = new Dictionary<BuffIcon, BuffInfo>();
            }

            m_BuffTable.Add(b.ID, b);

            NetState state = NetState;

            state?.Send(new AddBuffPacket(this, b));
        }

        public void RemoveBuff(BuffInfo b)
        {
            if (b == null)
            {
                return;
            }

            RemoveBuff(b.ID);
        }

        public void RemoveBuff(BuffIcon b)
        {
            if (m_BuffTable == null || !m_BuffTable.TryGetValue(b, out BuffInfo value))
            {
                return;
            }

            if (value.Timer != null && value.Timer.Running)
            {
                value.Timer.Stop();
            }

            m_BuffTable.Remove(b);

            NetState state = NetState;

            state?.Send(new RemoveBuffPacket(this, b));

            if (m_BuffTable.Count <= 0)
            {
                m_BuffTable = null;
            }
        }

        public void AutoStablePets()
        {
            if (AllFollowers.Count > 0)
            {
                for (int i = m_AllFollowers.Count - 1; i >= 0; --i)
                {
                    BaseCreature pet = AllFollowers[i] as BaseCreature;

                    if (pet == null)
                    {
                        continue;
                    }

                    if (pet.Summoned && pet.Map != Map)
                    {
                        pet.PlaySound(pet.GetAngerSound());

                        Timer.DelayCall(pet.Delete);

                        continue;
                    }

                    if (!pet.CanAutoStable || Stabled.Count >= AnimalTrainer.GetMaxStabled(this))
                    {
                        continue;
                    }

                    pet.ControlTarget = null;
                    pet.ControlOrder = LastOrderType.Stay;
                    pet.Internalize();

                    pet.SetControlMaster(null);
                    pet.SummonMaster = null;

                    pet.IsStabled = true;
                    pet.StabledBy = this;

                    Stabled.Add(pet);
                    m_AutoStabled.Add(pet);
                }
            }
        }

        public void ClaimAutoStabledPets()
        {
            if (!Region.AllowAutoClaim(this) || m_AutoStabled.Count <= 0)
            {
                return;
            }

            if (!Alive)
            {
                SendGump(new ReLoginClaimGump());
                return;
            }

            for (int i = m_AutoStabled.Count - 1; i >= 0; --i)
            {
                BaseCreature pet = m_AutoStabled[i] as BaseCreature;

                if (pet == null || pet.Deleted)
                {
                    if (pet != null)
                    {
                        pet.IsStabled = false;
                        pet.StabledBy = null;

                        if (Stabled.Contains(pet))
                        {
                            Stabled.Remove(pet);
                        }
                    }

                    continue;
                }

                if (Followers + pet.ControlSlots <= FollowersMax)
                {
                    pet.SetControlMaster(this);

                    if (pet.Summoned)
                    {
                        pet.SummonMaster = this;
                    }

                    pet.FollowTarget = this;
                    pet.ControlOrder = LastOrderType.Follow;

                    pet.MoveToWorld(Location, Map);

                    pet.IsStabled = false;
                    pet.StabledBy = null;

                    if (Stabled.Contains(pet))
                    {
                        Stabled.Remove(pet);
                    }
                }
                else
                {
                    SendLocalizedMessage(1049612, pet.Name); // ~1_NAME~ remained in the stables because you have too many followers.
                }
            }

            m_AutoStabled.Clear();
        }
    }
}
