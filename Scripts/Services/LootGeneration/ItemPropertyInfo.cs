using System;
using System.Collections.Generic;

namespace Server.Items
{
    public enum ItemType
    {
        Invalid,
        Melee,
        Ranged,
        Armor,
        Shield,
        Hat,
        Jewel
    }

    public class PropInfo
    {
        public ItemType ItemType { get; }          // Identifies the loot type
        public int Scale { get; }                  // scale, ie increments of 3 for regen on weapons. Most will not use this propery and will only use default of ItemPropertyInfo.Scale
        public int StandardMax { get; }            // Max Intensity for old loot system
        public int LootMax { get; }                // Max Intensity for new loot system
        public int[] PowerfulLootRange { get; }    // Range of over-cappeed for new loot system

        public bool UseStandardMax { get; }

        public PropInfo(int itemRef, int sMax, int lMax)
            : this((ItemType)itemRef, -1, sMax, lMax, null, false)
        {
        }

        public PropInfo(int itemRef, int sMax, int lMax, bool useStarndardMax)
            : this((ItemType)itemRef, -1, sMax, lMax, null, useStarndardMax)
        {
        }

        public PropInfo(int itemRef, int scale, int sMax, int lMax)
            : this((ItemType)itemRef, scale, sMax, lMax, null, false)
        {
        }

        public PropInfo(int itemRef, int sMax, int lMax, int[] powerfulRange)
            : this((ItemType)itemRef, -1, sMax, lMax, powerfulRange, false)
        {
        }

        public PropInfo(int itemRef, int scale, int sMax, int lMax, int[] powerfulRange)
            : this((ItemType)itemRef, scale, sMax, lMax, powerfulRange, false)
        {
        }

        public PropInfo(ItemType type, int scale, int sMax, int lMax, int[] powerfulRange, bool useStandardMax)
        {
            ItemType = type;
            Scale = scale;
            StandardMax = sMax;
            LootMax = lMax;
            PowerfulLootRange = powerfulRange;
            UseStandardMax = useStandardMax;
        }
    }

    public class ItemPropertyInfo
    {
        public int ID { get; set; }

        public object Attribute { get; }
        public TextDefinition AttributeName { get; }
        public int Weight { get; }

        public int Scale { get; }
        public int Start { get; }
        public int MaxIntensity { get; }

        public PropInfo[] PropCategories { get; } = new PropInfo[7];

        public ItemPropertyInfo(object attribute, TextDefinition attributeName, int weight, int scale, int start, int maxInt)
            : this(attribute, attributeName, weight, scale, start, maxInt, null)
        {
        }

        public ItemPropertyInfo(object attribute, TextDefinition attributeName, int weight, int scale, int start, int maxInt, params PropInfo[] categories)
        {
            Attribute = attribute;
            AttributeName = attributeName;
            Weight = weight;
            Scale = scale;
            Start = start;
            MaxIntensity = maxInt;

            if (categories != null)
            {
                for (int index = 0; index < categories.Length; index++)
                {
                    PropInfo cat = categories[index];

                    if (PropCategories[(int) cat.ItemType] == null)
                    {
                        PropCategories[(int) cat.ItemType] = cat;
                    }
                    else
                    {
                        throw new ArgumentException($"Property Category {cat.ItemType} already exists for {attribute}!");
                    }
                }
            }
        }

        public static Dictionary<int, ItemPropertyInfo> Table { get; } = new Dictionary<int, ItemPropertyInfo>();
        public static Dictionary<ItemType, List<int>> LootTable { get; } = new Dictionary<ItemType, List<int>>();

        static ItemPropertyInfo()
        {
            // i = runic, r = reforg, l = loot
            // 1 = melee, 2 = ranged, 3 = armor, 4 = sheild, 5 = hat, 6 = jewels
            Register(1, new ItemPropertyInfo(AosAttribute.DefendChance, 1075620, 110, 1, 1, 15,
                new PropInfo(1, 15, 15, new[] { 20 }), new PropInfo(2, 25, 25, new[] { 30, 35 }), new PropInfo(3, 0, 5, true), new PropInfo(4, 15, 15, new[] { 20 }), new PropInfo(5, 0, 5, true), new PropInfo(6, 15, 15, new[] { 20 })));

            Register(2, new ItemPropertyInfo(AosAttribute.AttackChance, 1075616, 130, 1, 1, 15,
                new PropInfo(1, 15, 15, new[] { 20 }), new PropInfo(2, 25, 25, new[] { 30, 35 }), new PropInfo(3, 0, 5, true), new PropInfo(4, 15, 15, new[] { 20 }), new PropInfo(5, 0, 5, true), new PropInfo(6, 15, 15, new[] { 20 })));

            Register(3, new ItemPropertyInfo(AosAttribute.RegenHits, 1075627, 100, 1, 1, 2,
                new PropInfo(1, 3, 0, 9), new PropInfo(2, 3, 0, 9), new PropInfo(3, 2, 2, new[] { 4 }), new PropInfo(4, 0, 2, new[] { 4 }), new PropInfo(5, 2, 2, new[] { 4 })));

            Register(4, new ItemPropertyInfo(AosAttribute.RegenStam, 1079411, 100, 1, 1, 3,
                new PropInfo(1, 3, 0, 9), new PropInfo(2, 3, 0, 9), new PropInfo(3, 3, 3, new[] { 4 }), new PropInfo(4, 0, 3, new[] { 4 }), new PropInfo(5, 3, 3, new[] { 4 })));

            Register(5, new ItemPropertyInfo(AosAttribute.RegenMana, 1079410, 100, 1, 1, 2,
                new PropInfo(1, 3, 0, 9), new PropInfo(2, 3, 0, 9), new PropInfo(3, 2, 2, new[] { 4 }), new PropInfo(4, 0, 2, new[] { 4 }), new PropInfo(5, 2, 2, new[] { 4 }), new PropInfo(6, 0, 2, 4)));

            Register(6, new ItemPropertyInfo(AosAttribute.BonusStr, 1079767, 110, 1, 1, 8,
                new PropInfo(1, 0, 5), new PropInfo(2, 0, 5), new PropInfo(3, 0, 5), new PropInfo(4, 0, 5), new PropInfo(5, 0, 5), new PropInfo(6, 8, 8, new[] { 9, 10 })));

            Register(7, new ItemPropertyInfo(AosAttribute.BonusDex, 1079732, 110, 1, 1, 8,
                 new PropInfo(1, 0, 5), new PropInfo(2, 0, 5), new PropInfo(3, 0, 5), new PropInfo(4, 0, 5), new PropInfo(5, 0, 5), new PropInfo(6, 8, 8, new[] { 9, 10 })));

            Register(8, new ItemPropertyInfo(AosAttribute.BonusInt, 1079756, 110, 1, 1, 8,
                new PropInfo(1, 0, 5), new PropInfo(2, 0, 5), new PropInfo(3, 0, 5), new PropInfo(4, 0, 5), new PropInfo(5, 0, 5), new PropInfo(6, 8, 8, new[] { 9, 10 })));

            Register(9, new ItemPropertyInfo(AosAttribute.BonusHits, 1075630, 110, 1, 1, 5,
                new PropInfo(1, 0, 5, new[] { 6, 7 }), new PropInfo(2, 0, 5, new[] { 6, 7 }), new PropInfo(3, 5, 5, new[] { 6, 7 }), new PropInfo(4, 0, 5, new[] { 6, 7 }), new PropInfo(5, 5, 5, new[] { 6, 7 })));

            Register(10, new ItemPropertyInfo(AosAttribute.BonusStam, 1075632, 110, 1, 1, 8,
                new PropInfo(1, 0, 5), new PropInfo(2, 0, 5), new PropInfo(3, 8, 8, new[] { 9, 10 }), new PropInfo(4, 0, 5), new PropInfo(5, 8, 8, new[] { 9, 10 }), new PropInfo(6, 0, 5)));

            Register(11, new ItemPropertyInfo(AosAttribute.BonusMana, 1075631, 110, 1, 1, 8,
                new PropInfo(1, 0, 5), new PropInfo(2, 0, 5), new PropInfo(3, 8, 8, new[] { 9, 10 }), new PropInfo(4, 0, 5), new PropInfo(5, 8, 8, new[] { 9, 10 }), new PropInfo(6, 0, 5)));

            Register(12, new ItemPropertyInfo(AosAttribute.WeaponDamage, 1079399, 100, 1, 1, 50,
                new PropInfo(1, 50, 50, new[] { 55, 60, 65, 70 }), new PropInfo(2, 50, 50, new[] { 55, 60, 65, 70 }), new PropInfo(4, 0, 35), new PropInfo(6, 25, 25, new[] { 30, 35 })));

            Register(13, new ItemPropertyInfo(AosAttribute.WeaponSpeed, 1075629, 110, 5, 5, 30,
                new PropInfo(1, 30, 30, new[] { 35, 40 }), new PropInfo(2, 30, 30, new[] { 35, 40 }), new PropInfo(4, 0, 5, new[] { 10 }), new PropInfo(6, 0, 5, new[] { 10 })));

            Register(14, new ItemPropertyInfo(AosAttribute.SpellDamage, 1075628, 100, 1, 1, 12,
                new PropInfo(6, 12, 12, new[] { 14, 16, 18 })));

            Register(15, new ItemPropertyInfo(AosAttribute.CastRecovery, 1075618, 120, 1, 1, 3,
                new PropInfo(6, 3, 3, new[] { 4 })));

            Register(16, new ItemPropertyInfo(AosAttribute.CastSpeed, 1075617, 140, 0, 1, 1,
                new PropInfo(1, 1, 1), new PropInfo(2, 1, 1), new PropInfo(4, 1, 1), new PropInfo(6, 1, 1)));

            Register(17, new ItemPropertyInfo(AosAttribute.LowerManaCost, 1075621, 110, 1, 1, 8,
                new PropInfo(1, 0, 5), new PropInfo(2, 0, 5), new PropInfo(3, 8, 8, new[] { 10 }), new PropInfo(4, 0, 5), new PropInfo(5, 8, 8, new[] { 10 }), new PropInfo(6, 8, 8, new[] { 10 })));

            Register(18, new ItemPropertyInfo(AosAttribute.LowerRegCost, 1075625, 100, 1, 1, 20,
                new PropInfo(3, 20, 20, new[] { 25 }), new PropInfo(5, 20, 20, new[] { 25 }), new PropInfo(6, 20, 20, new[] { 25 })));

            Register(19, new ItemPropertyInfo(AosAttribute.ReflectPhysical, 1075626, 100, 1, 1, 15,
                new PropInfo(1, 0, 15), new PropInfo(2, 0, 15), new PropInfo(3, 15, 15, new[] { 20 }), new PropInfo(4, 15, 15, new[] { 20 }), new PropInfo(5, 15, 15, new[] { 20 })));

            Register(20, new ItemPropertyInfo(AosAttribute.EnhancePotions, 1075624, 100, 5, 5, 25,
                new PropInfo(1, 0, 15), new PropInfo(2, 0, 15), new PropInfo(3, 0, 5), new PropInfo(5, 0, 5), new PropInfo(6, 25, 25, new[] { 30, 35 })));

            Register(21, new ItemPropertyInfo(AosAttribute.Luck, 1061153, 100, 1, 1, 100,
                new PropInfo(1, 100, 100, new[] { 110, 120, 130, 140, 150 }), new PropInfo(2, 120, 120, new[] { 130, 140, 150, 160, 170 }), new PropInfo(3, 100, 100, new[] { 110, 120, 130, 140, 150 }), new PropInfo(4, 100, 100, new[] { 110, 120, 130, 140, 150 }), new PropInfo(5, 100, 100, new[] { 110, 120, 130, 140, 150 }), new PropInfo(6, 100, 100, new[] { 110, 120, 130, 140, 150 })));

            Register(22, new ItemPropertyInfo(AosAttribute.SpellChanneling, 1079766, 100, 0, 1, 1,
                new PropInfo(1, 1, 1), new PropInfo(2, 1, 1), new PropInfo(4, 1, 1)));

            Register(23, new ItemPropertyInfo(AosAttribute.NightSight, 1015168, 50, 0, 1, 1,
                new PropInfo(3, 1, 1), new PropInfo(5, 1, 1), new PropInfo(6, 1, 1)));

            Register(25, new ItemPropertyInfo(AosWeaponAttribute.HitLeechHits, 1079698, 110, 1, 2, 50,
                new PropInfo(1, 2, 50, 50), new PropInfo(2, 2, 50, 50)));

            Register(26, new ItemPropertyInfo(AosWeaponAttribute.HitLeechStam, 1079707, 100, 1, 2, 50,
                new PropInfo(1, 2, 50, 50), new PropInfo(2, 2, 50, 50)));

            Register(27, new ItemPropertyInfo(AosWeaponAttribute.HitLeechMana, 1079701, 110, 1, 2, 50,
                new PropInfo(1, 2, 50, 50), new PropInfo(2, 2, 50, 50)));

            Register(28, new ItemPropertyInfo(AosWeaponAttribute.HitLowerAttack, 1079699, 110, 1, 2, 50,
                new PropInfo(1, 50, 50, new[] { 55, 60, 65, 70 }), new PropInfo(2, 50, 50, new[] { 55, 60, 65, 70 })));

            Register(29, new ItemPropertyInfo(AosWeaponAttribute.HitLowerDefend, 1079700, 130, 1, 2, 50,
                new PropInfo(1, 2, 50, 50, new[] { 55, 60, 65, 70 }), new PropInfo(2, 2, 50, 50, new[] { 55, 60, 65, 70 })));

            Register(30, new ItemPropertyInfo(AosWeaponAttribute.HitPhysicalArea, 1079696, 100, 1, 2, 50,
                new PropInfo(1, 2, 50, 50, new[] { 55, 60, 65, 70 }), new PropInfo(2, 2, 50, 50, new[] { 55, 60, 65, 70 })));

            Register(31, new ItemPropertyInfo(AosWeaponAttribute.HitFireArea, 1079695, 100, 1, 2, 50,
                new PropInfo(1, 2, 50, 50, new[] { 55, 60, 65, 70 }), new PropInfo(2, 2, 50, 50, new[] { 55, 60, 65, 70 })));

            Register(32, new ItemPropertyInfo(AosWeaponAttribute.HitColdArea, 1079693, 100, 1, 2, 50,
                new PropInfo(1, 2, 50, 50, new[] { 55, 60, 65, 70 }), new PropInfo(2, 2, 50, 50, new[] { 55, 60, 65, 70 })));

            Register(33, new ItemPropertyInfo(AosWeaponAttribute.HitPoisonArea, 1079697, 100, 1, 2, 50,
                new PropInfo(1, 2, 50, 50, new[] { 55, 60, 65, 70 }), new PropInfo(2, 2, 50, 50, new[] { 55, 60, 65, 70 })));

            Register(34, new ItemPropertyInfo(AosWeaponAttribute.HitEnergyArea, 1079694, 100, 1, 2, 50,
                new PropInfo(1, 2, 50, 50, new[] { 55, 60, 65, 70 }), new PropInfo(2, 2, 50, 50, new[] { 55, 60, 65, 70 })));

            Register(35, new ItemPropertyInfo(AosWeaponAttribute.HitMagicArrow, 1079706, 120, 1, 2, 50,
                new PropInfo(1, 2, 50, 50, new[] { 55, 60, 65, 70 }), new PropInfo(2, 2, 50, 50, new[] { 55, 60, 65, 70 })));

            Register(36, new ItemPropertyInfo(AosWeaponAttribute.HitHarm, 1079704, 110, 1, 2, 50,
                new PropInfo(1, 2, 50, 50, new[] { 55, 60, 65, 70 }), new PropInfo(2, 2, 50, 50, new[] { 55, 60, 65, 70 })));

            Register(37, new ItemPropertyInfo(AosWeaponAttribute.HitFireball, 1079703, 140, 1, 2, 50,
                new PropInfo(1, 2, 50, 50, new[] { 55, 60, 65, 70 }), new PropInfo(2, 2, 50, 50, new[] { 55, 60, 65, 70 })));

            Register(38, new ItemPropertyInfo(AosWeaponAttribute.HitLightning, 1079705, 140, 1, 2, 50,
                new PropInfo(1, 2, 50, 50, new[] { 55, 60, 65, 70 }), new PropInfo(2, 2, 50, 50, new[] { 55, 60, 65, 70 })));

            Register(39, new ItemPropertyInfo(AosWeaponAttribute.HitDispel, 1079702, 100, 1, 2, 50,
                new PropInfo(1, 2, 50, 50, new[] { 55, 60, 65, 70 }), new PropInfo(2, 2, 50, 50, new[] { 55, 60, 65, 70 })));

            Register(40, new ItemPropertyInfo(AosWeaponAttribute.UseBestSkill, 1079592, 150, 0, 1, 1,
                new PropInfo(1, 1, 1)));

            Register(41, new ItemPropertyInfo(AosWeaponAttribute.MageWeapon, 1079759, 100, 1, 1, 10,
                new PropInfo(1, 10, 10, new[] { 11, 12, 13, 14, 15 }), new PropInfo(2, 10, 10, new[] { 11, 12, 13, 14, 15 })));

            Register(44, new ItemPropertyInfo(AosWeaponAttribute.LowerStatReq, 1079757, 100, 10, 10, 100,
                new PropInfo(1, 0, 100), new PropInfo(2, 0, 100)));

            Register(45, new ItemPropertyInfo(AosArmorAttribute.LowerStatReq, 1079757, 100, 10, 10, 100,
                new PropInfo(3, 0, 100), new PropInfo(4, 0, 100), new PropInfo(5, 0, 100)));

            Register(49, new ItemPropertyInfo(AosArmorAttribute.MageArmor, 1079758, 0, 0, 1, 1,
                new PropInfo(3, 1, 1)));

            Register(51, new ItemPropertyInfo(AosElementAttribute.Physical, 1061158, 100, 1, 1, 15,
                new PropInfo(1, 10, 100, 100), new PropInfo(2, 10, 100, 100), new PropInfo(3, 15, 15, new[] { 20, 25, 30 }), new PropInfo(4, 15, 15), new PropInfo(5, 15, 15, new[] { 20, 25, 30 }), new PropInfo(6, 15, 15, new[] { 20 })));

            Register(52, new ItemPropertyInfo(AosElementAttribute.Fire, 1061159, 100, 1, 1, 15,
                new PropInfo(1, 10, 100, 100), new PropInfo(2, 10, 100, 100), new PropInfo(3, 15, 15, new[] { 20, 25, 30 }), new PropInfo(4, 15, 15), new PropInfo(5, 15, 15, new[] { 20, 25, 30 }), new PropInfo(6, 15, 15, new[] { 20 })));

            Register(53, new ItemPropertyInfo(AosElementAttribute.Cold, 1061160, 100, 1, 1, 15,
                new PropInfo(1, 10, 100, 100), new PropInfo(2, 10, 100, 100), new PropInfo(3, 15, 15, new[] { 20, 25, 30 }), new PropInfo(4, 15, 15), new PropInfo(5, 15, 15, new[] { 20, 25, 30 }), new PropInfo(6, 15, 15, new[] { 20 })));

            Register(54, new ItemPropertyInfo(AosElementAttribute.Poison, 1061161, 100, 1, 1, 15,
                new PropInfo(1, 10, 100, 100), new PropInfo(2, 10, 100, 100), new PropInfo(3, 15, 15, new[] { 20, 25, 30 }), new PropInfo(4, 15, 15), new PropInfo(5, 15, 15, new[] { 20, 25, 30 }), new PropInfo(6, 15, 15, new[] { 20 })));

            Register(55, new ItemPropertyInfo(AosElementAttribute.Energy, 1061162, 100, 1, 1, 15,
                new PropInfo(1, 10, 100, 100), new PropInfo(2, 10, 100, 100), new PropInfo(3, 15, 15, new[] { 20, 25, 30 }), new PropInfo(4, 15, 15), new PropInfo(5, 15, 15, new[] { 20, 25, 30 }), new PropInfo(6, 15, 15, new[] { 20 })));

            // Non-Imbuable
            Register(56, new ItemPropertyInfo(AosElementAttribute.Chaos, 1151805, 100, 1, 1, 15,
                new PropInfo(1, 10, 100, 100), new PropInfo(2, 10, 100, 100)));

            Register(60, new ItemPropertyInfo("WeaponVelocity", 1080416, 140, 1, 11, 50,
                new PropInfo(2, 50, 50)));

            Register(61, new ItemPropertyInfo(AosAttribute.BalancedWeapon, 1072792, 150, 0, 1, 1,
               new PropInfo(2, 1, 1)));            

            // Non-Imbuable, Non-Loot
            Register(62, new ItemPropertyInfo("SearingWeapon", 1151183, 150, 0, 1, 1));

            Register(63, new ItemPropertyInfo(AosAttribute.BalancedWeapon, 1072792, 100, 0, 1, 1,
                new PropInfo(1, 1, 1)));

            // Slayers
            Register(101, new ItemPropertyInfo(SlayerName.OrcSlaying, 1079741, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(102, new ItemPropertyInfo(SlayerName.TrollSlaughter, 1079754, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(103, new ItemPropertyInfo(SlayerName.OgreTrashing, 1079739, 100 , 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(104, new ItemPropertyInfo(SlayerName.DragonSlaying, 1061284, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(105, new ItemPropertyInfo(SlayerName.Terathan, 1079753, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(106, new ItemPropertyInfo(SlayerName.SnakesBane, 1079744, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(107, new ItemPropertyInfo(SlayerName.LizardmanSlaughter, 1079738, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(108, new ItemPropertyInfo(SlayerName.GargoylesFoe, 1079737, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(111, new ItemPropertyInfo(SlayerName.Ophidian, 1079740, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(112, new ItemPropertyInfo(SlayerName.SpidersDeath, 1079746, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(113, new ItemPropertyInfo(SlayerName.ScorpionsBane, 1079743, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(114, new ItemPropertyInfo(SlayerName.FlameDousing, 1079736, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(115, new ItemPropertyInfo(SlayerName.WaterDissipation, 1079755, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(116, new ItemPropertyInfo(SlayerName.Vacuum, 1079733, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(117, new ItemPropertyInfo(SlayerName.ElementalHealth, 1079742, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(118, new ItemPropertyInfo(SlayerName.EarthShatter, 1079735, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(119, new ItemPropertyInfo(SlayerName.BloodDrinking, 1079734, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(120, new ItemPropertyInfo(SlayerName.SummerWind, 1079745, 100, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));

            //Super Slayers
            Register(121, new ItemPropertyInfo(SlayerName.Silver, 1079752, 130, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(122, new ItemPropertyInfo(SlayerName.Repond, 1079750, 130, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(123, new ItemPropertyInfo(SlayerName.ReptilianDeath, 1079751, 130, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(124, new ItemPropertyInfo(SlayerName.Exorcism, 1079748, 130, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(125, new ItemPropertyInfo(SlayerName.ArachnidDoom, 1079747, 130, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(126, new ItemPropertyInfo(SlayerName.ElementalBan, 1079749, 130, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));
            Register(127, new ItemPropertyInfo(SlayerName.Fey, 1154652, 130, 0, 1, 1, new PropInfo(1, 1, 1), new PropInfo(2, 1, 1)));

            // Non-Imbuable, non-Loot
            Register(128, new ItemPropertyInfo(SlayerName.Dinosaur, 1156240, 130, 0, 1, 1));
            Register(129, new ItemPropertyInfo(SlayerName.Myrmidex, 1156241, 130, 0, 1, 1));
            Register(130, new ItemPropertyInfo(SlayerName.Eodon, 1156126, 130, 0, 1, 1));
            Register(131, new ItemPropertyInfo(SlayerName.EodonTribe, 1156347, 130, 0, 1, 1));

            // Talisman Slayers, non-Imbuable, non-Loot
            Register(135, new ItemPropertyInfo(TalismanSlayerName.Bear, 1072504, 130, 0, 1, 1));
            Register(136, new ItemPropertyInfo(TalismanSlayerName.Vermin, 1072505, 130, 0, 1, 1));
            Register(137, new ItemPropertyInfo(TalismanSlayerName.Bat, 1072506, 130, 0, 1, 1));
            Register(138, new ItemPropertyInfo(TalismanSlayerName.Mage, 1072507, 130, 0, 1, 1));
            Register(139, new ItemPropertyInfo(TalismanSlayerName.Beetle, 1072508, 130, 0, 1, 1));
            Register(140, new ItemPropertyInfo(TalismanSlayerName.Bird, 1072509, 130, 0, 1, 1));
            Register(141, new ItemPropertyInfo(TalismanSlayerName.Ice, 1072510, 130, 0, 1, 1));
            Register(142, new ItemPropertyInfo(TalismanSlayerName.Flame, 1072511, 130, 0, 1, 1));
            Register(143, new ItemPropertyInfo(TalismanSlayerName.Bovine, 1072512, 130, 0, 1, 1));
            Register(144, new ItemPropertyInfo(TalismanSlayerName.Wolf, 1075462, 130, 0, 1, 1));
            Register(145, new ItemPropertyInfo(TalismanSlayerName.Undead, 1079752, 130, 0, 1, 1));
            Register(146, new ItemPropertyInfo(TalismanSlayerName.Goblin, 1095010, 130, 0, 1, 1));

            Register(151, new ItemPropertyInfo(SkillName.Fencing, 1044102, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(152, new ItemPropertyInfo(SkillName.Macing, 1044101, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(153, new ItemPropertyInfo(SkillName.Swords, 1044100, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(154, new ItemPropertyInfo(SkillName.Musicianship, 1044089, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(155, new ItemPropertyInfo(SkillName.Magery, 1044085, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));

            Register(156, new ItemPropertyInfo(SkillName.Wrestling, 1044103, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(157, new ItemPropertyInfo(SkillName.AnimalTaming, 1044095, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(158, new ItemPropertyInfo(SkillName.SpiritSpeak, 1044092, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(159, new ItemPropertyInfo(SkillName.Tactics, 1044087, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(160, new ItemPropertyInfo(SkillName.Provocation, 1044082, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));

            Register(161, new ItemPropertyInfo(SkillName.Focus, 1044110, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(162, new ItemPropertyInfo(SkillName.Parry, 1044065, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(163, new ItemPropertyInfo(SkillName.Stealth, 1044107, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(164, new ItemPropertyInfo(SkillName.Meditation, 1044106, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(165, new ItemPropertyInfo(SkillName.AnimalLore, 1044062, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(166, new ItemPropertyInfo(SkillName.Discordance, 1044075, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));

            Register(167, new ItemPropertyInfo(SkillName.Mysticism, 1044115, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(168, new ItemPropertyInfo(SkillName.Bushido, 1044112, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(169, new ItemPropertyInfo(SkillName.Necromancy, 1044109, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(170, new ItemPropertyInfo(SkillName.Veterinary, 1044099, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(171, new ItemPropertyInfo(SkillName.Stealing, 1044093, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(172, new ItemPropertyInfo(SkillName.EvalInt, 1044076, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(173, new ItemPropertyInfo(SkillName.Anatomy, 1044061, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));

            Register(174, new ItemPropertyInfo(SkillName.Peacemaking, 1044069, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(175, new ItemPropertyInfo(SkillName.Ninjitsu, 1044113, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(176, new ItemPropertyInfo(SkillName.Chivalry, 1044111, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(177, new ItemPropertyInfo(SkillName.Archery, 1044091, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(178, new ItemPropertyInfo(SkillName.MagicResist, 1044086, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(179, new ItemPropertyInfo(SkillName.Healing, 1044077, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            
            Register(181, new ItemPropertyInfo(SkillName.Lumberjacking, 1002100, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(182, new ItemPropertyInfo(SkillName.Snooping, 1002138, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));
            Register(183, new ItemPropertyInfo(SkillName.Mining, 1002111, 140, 1, 1, 15, new PropInfo(6, 15, 15, new[] { 20 })));

            // Non-Imbuables for getting item intensity only
            Register(200, new ItemPropertyInfo(AosWeaponAttribute.BloodDrinker, 1017407, 140, 0, 1, 1,
                new PropInfo(1, 0, 1)));

            Register(201, new ItemPropertyInfo(AosWeaponAttribute.BattleLust, 1113710, 140, 0, 1, 1,
                new PropInfo(1, 0, 1)));

            Register(202, new ItemPropertyInfo(AosWeaponAttribute.HitCurse, 1154673, 140, 1, 2, 50));

            Register(203, new ItemPropertyInfo(AosWeaponAttribute.HitFatigue, 1154668, 140, 1, 2, 50,
                 new PropInfo(1, 0, 70), new PropInfo(2, 0, 70)));

            Register(204, new ItemPropertyInfo(AosWeaponAttribute.HitManaDrain, 1154669, 140, 1, 2, 50,
                 new PropInfo(1, 0, 70), new PropInfo(2, 0, 70)));

            Register(206, new ItemPropertyInfo(AosWeaponAttribute.ReactiveParalyze, 1154660, 140, 0, 1, 1,
                 new PropInfo(1, 0, 1)));

            Register(233, new ItemPropertyInfo(AosWeaponAttribute.ResistPhysicalBonus, 1061158, 100, 1, 1, 15,
                new PropInfo(1, 15, 15, new[] { 20 }), new PropInfo(2, 15, 15, new[] { 20 })));

            Register(234, new ItemPropertyInfo(AosWeaponAttribute.ResistFireBonus, 1061159, 100, 1, 1, 15,
                new PropInfo(1, 15, 15, new[] { 20 }), new PropInfo(2, 15, 15, new[] { 20 })));

            Register(235, new ItemPropertyInfo(AosWeaponAttribute.ResistColdBonus, 1061160, 100, 1, 1, 15,
                new PropInfo(1, 15, 15, new[] { 20 }), new PropInfo(2, 15, 15, new[] { 20 })));

            Register(236, new ItemPropertyInfo(AosWeaponAttribute.ResistPoisonBonus, 1061161, 100, 1, 1, 15,
                new PropInfo(1, 15, 15, new[] { 20 }), new PropInfo(2, 15, 15, new[] { 20 })));

            Register(237, new ItemPropertyInfo(AosWeaponAttribute.ResistEnergyBonus, 1061162, 100, 1, 1, 15,
                new PropInfo(1, 15, 15, new[] { 20 }), new PropInfo(2, 15, 15, new[] { 20 })));

            Register(208, new ItemPropertyInfo(SAAbsorptionAttribute.EaterFire, 1154662, 140, 1, 1, 15,
                new PropInfo(3, 0, 15), new PropInfo(4, 0, 15), new PropInfo(5, 0, 15)));

            Register(209, new ItemPropertyInfo(SAAbsorptionAttribute.EaterCold, 1154663, 140, 1, 1, 15,
                new PropInfo(3, 0, 15), new PropInfo(4, 0, 15), new PropInfo(5, 0, 15)));

            Register(210, new ItemPropertyInfo(SAAbsorptionAttribute.EaterPoison, 1154664, 140, 1, 1, 15,
                new PropInfo(3, 0, 15), new PropInfo(4, 0, 15), new PropInfo(5, 0, 15)));

            Register(211, new ItemPropertyInfo(SAAbsorptionAttribute.EaterEnergy, 1154665, 140, 1, 1, 15,
                new PropInfo(3, 0, 15), new PropInfo(4, 0, 15), new PropInfo(5, 0, 15)));

            Register(212, new ItemPropertyInfo(SAAbsorptionAttribute.EaterKinetic, 1154666, 140, 1, 1, 15,
                new PropInfo(3, 0, 15), new PropInfo(4, 0, 15), new PropInfo(5, 0, 15)));

            Register(213, new ItemPropertyInfo(SAAbsorptionAttribute.EaterDamage, 1154667, 140, 1, 1, 15,
                new PropInfo(3, 0, 15), new PropInfo(4, 0, 15), new PropInfo(5, 0, 15)));

            // Non-Imbuable, non-loot
            Register(214, new ItemPropertyInfo(SAAbsorptionAttribute.ResonanceFire, 1154655, 140, 1, 1, 20));
            Register(215, new ItemPropertyInfo(SAAbsorptionAttribute.ResonanceCold, 1154656, 140, 1, 1, 20));
            Register(216, new ItemPropertyInfo(SAAbsorptionAttribute.ResonancePoison, 1154657, 140, 1, 1, 20));
            Register(217, new ItemPropertyInfo(SAAbsorptionAttribute.ResonanceEnergy, 1154658, 140, 1, 1, 20));
            Register(218, new ItemPropertyInfo(SAAbsorptionAttribute.ResonanceKinetic, 1154659, 140, 1, 1, 20));

            Register(219, new ItemPropertyInfo(SAAbsorptionAttribute.CastingFocus, 1116535, 140, 1, 1, 3,
                new PropInfo(3, 0, 3), new PropInfo(5, 0, 3)));

            Register(220, new ItemPropertyInfo(AosArmorAttribute.ReactiveParalyze, 1154660, 140, 0, 1, 1,
                new PropInfo(1, 0, 1), new PropInfo(4, 0, 1)));

            Register(221, new ItemPropertyInfo(AosArmorAttribute.SoulCharge, 1116536, 140, 5, 5, 30,
                new PropInfo(4, 0, 30)));

            Register(500, new ItemPropertyInfo(AosArmorAttribute.SelfRepair, 1079709, 100, 1, 1, 5,
                new PropInfo(3, 5, 5), new PropInfo(4, 5, 5), new PropInfo(5, 5, 5)));

            Register(501, new ItemPropertyInfo(AosWeaponAttribute.SelfRepair, 1079709, 100, 1, 1, 5,
                new PropInfo(1, 5, 5), new PropInfo(2, 5, 5)));

            // Non-Imbuable, non-loot
            Register(600, new ItemPropertyInfo(ExtendedWeaponAttribute.BoneBreaker, 1157318, 140, 1, 1, 1));
            Register(601, new ItemPropertyInfo(ExtendedWeaponAttribute.HitSwarm, 1157328, 140, 1, 1, 20));
            Register(602, new ItemPropertyInfo(ExtendedWeaponAttribute.HitSparks, 1157330, 140, 1, 1, 20));
            Register(603, new ItemPropertyInfo(ExtendedWeaponAttribute.Bane, 1154671, 140, 1, 1, 1));

            BuildLootTables();
        }

        private static void Register(int id, ItemPropertyInfo info)
        {
            if (Table.ContainsKey(id))
            {
                throw new ArgumentException($"ID Already Exists: {id}");
            }

            info.ID = id;
            Table[id] = info;
        }

        public PropInfo GetItemTypeInfo(ItemType type)
        {
            for (int index = 0; index < PropCategories.Length; index++)
            {
                PropInfo prop = PropCategories[index];

                if (prop != null && prop.ItemType == type)
                {
                    return prop;
                }
            }

            return null;
        }

        public static ItemPropertyInfo GetInfo(object attribute)
        {
            int id = GetID(attribute);

            if (Table.TryGetValue(id, out ItemPropertyInfo value))
            {
                return value;
            }

            return null;
        }

        public static ItemPropertyInfo GetInfo(int id)
        {
            if (Table.TryGetValue(id, out ItemPropertyInfo value))
            {
                return value;
            }

            return null;
        }

        public static int GetWeight(int id)
        {
            if (Table.TryGetValue(id, out ItemPropertyInfo value))
            {
                return value.Weight;
            }

            return 0;
        }

        public static int GetMaxIntensity(Item item, object attribute)
        {
            return GetMaxIntensity(item, GetID(attribute), false, false);
        }

        /// <summary>
        /// Maximum intensity in regards to imbuing weight calculation. Some items may be over this 'cap'
        /// </summary>
        /// <param name="item">item to check</param>
        /// <param name="id">property id</param>
        /// <param name="imbuing">true for imbuing, false for loot</param>
        /// <param name="applyingProperty">are we calling this to assign a property value</param>
        /// <returns></returns>
        public static int GetMaxIntensity(Item item, int id, bool imbuing, bool applyingProperty = false)
        {
            if (Table.TryGetValue(id, out ItemPropertyInfo value))
            {
                PropInfo info = value.GetItemTypeInfo(GetItemType(item));

                // First, we try to get the max intensity from the PropInfo. If null or we're getting an intensity for special imbuing purpopses, we go to the default MaxIntenity
                if (info == null || !applyingProperty && info.UseStandardMax || imbuing && !ForcesNewLootMax(id))
                {
                    if (item is BaseWeapon weapon && (id == 25 || id == 27))
                    {
                        return GetSpecialMaxIntensity(weapon);
                    }

                    return value.MaxIntensity;
                }

                if (item is BaseWeapon baseWeapon && (id == 25 || id == 27))
                {
                    return GetSpecialMaxIntensity(baseWeapon);
                }

                return info.LootMax;
            }

            return 0;
        }

        private static readonly int[] _ForceUseNewTable = { 1, 2, 12 };

        /// <summary>
        /// We may want to force the new loot tables for items such as ranged weapons that have a different max than melee, think hci/dci (15/25).
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool ForcesNewLootMax(int id)
        {
            for (int index = 0; index < _ForceUseNewTable.Length; index++)
            {
                int i = _ForceUseNewTable[index];

                if (i == id)
                {
                    return true;
                }
            }

            return false;
        }

        public static int[] GetMaxOvercappedRange(Item item, int id)
        {
            if (Table.TryGetValue(id, out ItemPropertyInfo value))
            {
                PropInfo info = value.GetItemTypeInfo(GetItemType(item));

                if (info != null && info.PowerfulLootRange != null && info.PowerfulLootRange.Length > 0)
                {
                    return info.PowerfulLootRange;
                }
            }

            return null;
        }

        public static int GetSpecialMaxIntensity(BaseWeapon wep)
        {
            int max = (int)(wep.Speed * 2500 / (100 + wep.Attributes.WeaponSpeed));

            if (wep is BaseRanged)
            {
                max /= 2;
            }

            return max;
        }

        /// <summary>
        /// Minimum intensity. An item property will never start lower than this value.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="loot">this will determine if default min intensity is used, or if it uses its min scaled value</param>
        /// <returns></returns>
        public static int GetMinIntensity(Item item, int id, bool loot = false)
        {
            if (Table.TryGetValue(id, out ItemPropertyInfo value))
            {
                if (loot)
                {
                    // Loot min intensity is always the lowest scale value.
                    return GetScale(item, id, loot);
                }

                // Not so with imbuing, for most properties.
                return value.Start;
            }

            return 0;
        }

        /// <summary>
        /// Item property incremental scale, ie 10, 20, 30 luck
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static int GetScale(Item item, object attribute, bool loot)
        {
            return GetScale(item, GetID(attribute), loot);
        }

        public static int GetScale(Item item, int id, bool loot)
        {
            if (Table.TryGetValue(id, out ItemPropertyInfo value))
            {
                if (loot)
                {
                    if (id >= 151 && id <= 183)
                    {
                        return 5;
                    }

                    if (id == 21)
                    {
                        return 10;
                    }

                    if (id == 12)
                    {
                        return 5;
                    }
                }

                PropInfo info = value.GetItemTypeInfo(GetItemType(item));

                if (info != null && info.Scale >= 0)
                {
                    return info.Scale;
                }

                return value.Scale;
            }

            return 1;
        }

        /// <summary>
        /// Gets the associated attribute, ie AosAttribute, etc
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static object GetAttribute(int id)
        {
            if (Table.TryGetValue(id, out ItemPropertyInfo value))
            {
                return value.Attribute;
            }

            return null;
        }

        public static int GetTotalWeight(Item item, object attribute, int value)
        {
            ItemPropertyInfo info = GetInfo(attribute);
            int max = GetMaxIntensity(item, attribute);

            if (info != null && max > 0)
            {
                return (int)(info.Weight / (double)max * value);
            }

            return 0;
        }

        public static ItemType GetItemType(Item item)
        {
            if (item is BaseRanged)
            {
                return ItemType.Ranged;
            }

            if (item is BaseWeapon)
            {
                return ItemType.Melee;
            }

            if (item is BaseShield)
            {
                return ItemType.Shield;
            }

            if (item is BaseArmor)
            {
                return ItemType.Armor;
            }

            if (item is BaseHat)
            {
                return ItemType.Hat;
            }

            if (item is BaseJewel)
            {
                return ItemType.Jewel;
            }

            return ItemType.Invalid;
        }

        public static int GetID(object attr)
        {
            int id = -1;

            if (attr is AosAttribute aosAttribute)
            {
                id = GetIDForAttribute(aosAttribute);
            }
            else if (attr is AosWeaponAttribute weaponAttribute)
            {
                id = GetIDForAttribute(weaponAttribute);
            }
            else if (attr is ExtendedWeaponAttribute extendedWeaponAttribute)
            {
                id = GetIDForAttribute(extendedWeaponAttribute);
            }
            else if (attr is SkillName skillName)
            {
                id = GetIDForAttribute(skillName);
            }
            else if (attr is SlayerName slayerName)
            {
                id = GetIDForAttribute(slayerName);
            }
            else if (attr is SAAbsorptionAttribute absorptionAttribute)
            {
                id = GetIDForAttribute(absorptionAttribute);
            }
            else if (attr is AosArmorAttribute armorAttribute)
            {
                id = GetIDForAttribute(armorAttribute);
            }
            else if (attr is AosElementAttribute elementAttribute)
            {
                id = GetIDForAttribute(elementAttribute);
            }
            else if (attr is TalismanSlayerName name)
            {
                id = GetIDForAttribute(name);
            }
            else if (attr is string s)
            {
                id = GetIDForAttribute(s);
            }

            return id;
        }

        public static int GetIDForAttribute(AosAttribute attr)
        {
            foreach (KeyValuePair<int, ItemPropertyInfo> kvp in Table)
            {
                ItemPropertyInfo info = kvp.Value;

                if (info.Attribute is AosAttribute attribute && attribute == attr)
                {
                    return kvp.Key;
                }
            }

            return -1;
        }

        public static int GetIDForAttribute(AosWeaponAttribute attr)
        {
            foreach (KeyValuePair<int, ItemPropertyInfo> kvp in Table)
            {
                ItemPropertyInfo info = kvp.Value;

                if (info.Attribute is AosWeaponAttribute weaponAttribute && weaponAttribute == attr)
                {
                    return kvp.Key;
                }
            }

            return -1;
        }

        public static int GetIDForAttribute(ExtendedWeaponAttribute attr)
        {
            foreach (KeyValuePair<int, ItemPropertyInfo> kvp in Table)
            {
                ItemPropertyInfo info = kvp.Value;

                if (info.Attribute is ExtendedWeaponAttribute attribute && attribute == attr)
                {
                    return kvp.Key;
                }
            }

            return -1;
        }

        public static int GetIDForAttribute(SAAbsorptionAttribute attr)
        {
            foreach (KeyValuePair<int, ItemPropertyInfo> kvp in Table)
            {
                ItemPropertyInfo info = kvp.Value;

                if (info.Attribute is SAAbsorptionAttribute attribute && attribute == attr)
                {
                    return kvp.Key;
                }
            }

            return -1;
        }

        public static int GetIDForAttribute(AosArmorAttribute attr)
        {
            foreach (KeyValuePair<int, ItemPropertyInfo> kvp in Table)
            {
                ItemPropertyInfo info = kvp.Value;

                if (info.Attribute is AosArmorAttribute attribute && attribute == attr)
                {
                    return kvp.Key;
                }
            }

            return -1;
        }

        public static int GetIDForAttribute(SkillName attr)
        {
            foreach (KeyValuePair<int, ItemPropertyInfo> kvp in Table)
            {
                ItemPropertyInfo info = kvp.Value;

                if (info.Attribute is SkillName name && name == attr)
                {
                    return kvp.Key;
                }
            }

            return -1;
        }

        public static int GetIDForAttribute(SlayerName attr)
        {
            foreach (KeyValuePair<int, ItemPropertyInfo> kvp in Table)
            {
                ItemPropertyInfo info = kvp.Value;

                if (info.Attribute is SlayerName name && name == attr)
                {
                    return kvp.Key;
                }
            }

            return -1;
        }

        public static int GetIDForAttribute(TalismanSlayerName attr)
        {
            foreach (KeyValuePair<int, ItemPropertyInfo> kvp in Table)
            {
                ItemPropertyInfo info = kvp.Value;

                if (info.Attribute is TalismanSlayerName name && name == attr)
                {
                    return kvp.Key;
                }
            }

            return -1;
        }

        public static int GetIDForAttribute(AosElementAttribute type)
        {
            switch (type)
            {
                case AosElementAttribute.Physical:
                {
                    return 51;
                }
                case AosElementAttribute.Fire:
                {
                    return 52;
                }
                case AosElementAttribute.Cold:
                {
                    return 53;
                }
                case AosElementAttribute.Poison:
                {
                    return 54;
                }
                case AosElementAttribute.Energy:
                {
                    return 55;
                }
                case AosElementAttribute.Chaos:
                {
                    return 56;
                }
            }

            return -1;
        }

        public static int GetIDForAttribute(string str)
        {
            if (str == "WeaponVelocity")
            {
                return 60;
            }

            if (str == "SearingWeapon")
            {
                return 62;
            }

            if (str == "Slayer")
            {
                return 101;
            }

            if (str == "ElementalDamage")
            {
                return 51;
            }

            if (str == "HitSpell")
            {
                return 37;
            }

            if (str == "HitArea")
            {
                return 30;
            }

            if (str == "RandomEater")
            {
                return 208;
            }

            return -1;
        }

        public static List<int> LookupLootTable(Item item)
        {
            ItemType type = GetItemType(item);

            if (type != ItemType.Invalid)
            {
                return LootTable[type];
            }

            return new List<int>();
        }

        public static void BuildLootTables()
        {
            foreach (object i in Enum.GetValues(typeof(ItemType)))
            {
                ItemType type = (ItemType)i;

                if (type == ItemType.Invalid)
                {
                    continue;
                }

                List<int> list = new List<int>();

                foreach (ItemPropertyInfo prop in Table.Values)
                {
                    if (prop.Attribute is SlayerName || prop.Attribute is SkillName)
                    {
                        continue;
                    }

                    PropInfo info = prop.GetItemTypeInfo(type);

                    if (info != null && info.LootMax > 0)
                    {
                        list.Add(prop.ID);
                    }
                }

                if (list.Count > 0)
                {
                    LootTable[type] = list;
                    AddSpecial(type, list);
                }
            }
        }

        /// <summary>
        /// These are a group of properties that can only count as limited property slots. This will prevent slayers and skills from dominating the property roll
        /// </summary>
        /// <param name="type"></param>
        private static void AddSpecial(ItemType type, List<int> list)
        {
            switch (type)
            {
                case ItemType.Melee:
                case ItemType.Ranged:
                {
                    list.Add(1000); // Placeholder for random slayers
                    break;
                }
                case ItemType.Jewel:
                {
                    list.Add(1001); // Placeholders for random skills
                    list.Add(1002);
                    list.Add(1003);
                    list.Add(1004);
                    list.Add(1005);
                    break;
                }
            }
        }

        public static bool ValidateProperty(object attribute)
        {
            return GetID(attribute) > 0;
        }

        /// <summary>
        /// Loot/Reforged Validator
        /// </summary>
        /// <param name="item"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static bool ValidateProperty(Item item, object attribute)
        {
            return ValidateProperty(item, GetID(attribute));
        }

        public static bool ValidateProperty(Item item, int id)
        {
            ItemPropertyInfo info = GetInfo(id);

            if (info != null)
            {
                PropInfo typeInfo = info.GetItemTypeInfo(GetItemType(item));

                if (typeInfo != null)
                {
                    if (typeInfo.LootMax <= 0)
                    {
                        return false;
                    }

                    switch (id)
                    {
                        case 200: // Blood Drinking
                        {
                            return item is BaseWeapon weapon && (weapon.PrimaryAbility == WeaponAbility.BleedAttack || weapon.SecondaryAbility == WeaponAbility.BleedAttack);
                        }
                        case 206: // Reactive Paralyze Weapon
                        {
                            return item is BaseWeapon && item.Layer == Layer.TwoHanded;
                        }
                        case 220: // Reactive Paralyze Armor
                        {
                            return item is BaseShield;
                        }
                        case 63:  // Balanced
                        case 61:
                        {
                            return item.Layer == Layer.TwoHanded;
                        }
                        case 40:  // UBWS
                        {
                            return GetItemType(item) == ItemType.Melee;
                        }
                        case 30:
                        case 31:
                        case 32:
                        case 33:
                        case 34: // Hit Area Cannot be applied if it already has one present
                        {
                            return item is BaseWeapon baseWeapon && !HasHitArea(baseWeapon);
                        }
                        case 35:
                        case 36:
                        case 37:
                        case 38:
                        case 39: // Hit Spell Cannot be applied if it already has one present
                        {
                            return item is BaseWeapon bw && !HasHitSpell(bw);
                        }
                        case 49: // MageArmor cannot be applied if the armor is already meddable
                        {
                            return item is BaseArmor armor && armor.MeditationAllowance != ArmorMeditationAllowance.All;
                        }
                        case 208:
                        case 209:
                        case 210:
                        case 211:
                        case 212:
                        case 213: // Eaters Cannot be applied if it already has one present
                        {
                            return (item is BaseArmor || item is BaseJewel || item is BaseWeapon) && !HasEater(item);
                        }
                        case 500:
                        case 501: // Self Repair cannot be added to items with brittle/antique/no repair or items that have been imbued
                        {
                            NegativeAttributes neg = RunicReforging.GetNegativeAttributes(item);

                            if (neg != null && (neg.Brittle > 0 || neg.Antique > 0))
                            {
                                return false;
                            }

                            break;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public static bool HasHitSpell(BaseWeapon wep)
        {
            return wep.WeaponAttributes.HitFireball > 0 || wep.WeaponAttributes.HitLightning > 0 || wep.WeaponAttributes.HitMagicArrow > 0
                /*|| wep.WeaponAttributes.HitCurse > 0*/ || wep.WeaponAttributes.HitHarm > 0 || wep.WeaponAttributes.HitDispel > 0;
        }

        public static bool HasHitArea(BaseWeapon wep)
        {
            return wep.WeaponAttributes.HitPhysicalArea > 0 || wep.WeaponAttributes.HitFireArea > 0 || wep.WeaponAttributes.HitColdArea > 0
                || wep.WeaponAttributes.HitPoisonArea > 0 || wep.WeaponAttributes.HitEnergyArea > 0;
        }

        public static bool HasEater(Item item)
        {
            SAAbsorptionAttributes attr = RunicReforging.GetSAAbsorptionAttributes(item);

            return attr != null && (attr.EaterKinetic > 0 || attr.EaterFire > 0 || attr.EaterCold > 0 || attr.EaterPoison > 0 || attr.EaterEnergy > 0 || attr.EaterDamage > 0);
        }
    }
}
