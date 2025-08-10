using System;
using Server.Items;

namespace Server.Engines.Quests
{
    public class BaseReward
    {
        private static readonly int[] m_SatchelHues =
        {
            0x1C, 0x37, 0x71, 0x3A, 0x62, 0x44, 0x59, 0x13, 0x21, 0x3, 0xD, 0x3F
        }; 

        private static readonly int[] m_RewardBagHues =
        {
            0x385, 0x3E9, 0x4B0, 0x4E6, 0x514, 0x54A, 0x578, 0x5AE, 0x5DC, 0x612, 0x640, 0x676, 0x6A5, 0x6DA, 0x708, 0x774
        };

        public BaseReward(object name)
            : this(null, 1, name)
        { }

        public BaseReward(Type type, object name)
            : this(type, 1, name)
        { }

        public BaseReward(Type type, object name, int img, int hue)
            : this(type, 1, name, img, hue)
        { }

        public BaseReward(Type type, int amount, object name)
        : this(type, amount, name, 0, 0)
        { }

        public BaseReward(Type type, int amount, object name, int img, int hue)
        {
            Type = type;
            Amount = amount;
            Name = name;
            Image = img;
        }

        public BaseQuest Quest { get; set; }

        public Type Type { get; }

        public int Amount { get; set; }

        public object Name { get; }

        public int Image { get; set; }

        public int Hue { get; set; }

        public static int SatchelHue()
        {
            return m_SatchelHues[Utility.Random(m_SatchelHues.Length)];
        }

        public static int RewardBagHue()
        {
            if (Utility.RandomDouble() < 0.005)
            {
                return 0;
            }

            int row = Utility.Random(m_RewardBagHues.Length / 2) * 2;

            return Utility.RandomMinMax(m_RewardBagHues[row], m_RewardBagHues[row + 1]);
        }

        public static int StrongboxHue()
        {
            return Utility.RandomMinMax(0x898, 0x8B0);
        }

        public static void ApplyMods(Item item)
        {
            if (item != null)
            {
                RunicReforging.GenerateRandomItem(item, 0, 10, 850);
            }
        }

        public static Item Jewlery()
        {
            BaseJewel item = Loot.RandomJewelry();
            ApplyMods(item);

            return item;
        }

        public static Item RangedWeapon()
        {
            BaseWeapon item = Loot.RandomRangedWeapon();
            ApplyMods(item);

            return item;
        }

        public static Item Armor()
        {
            BaseArmor item = Loot.RandomArmor();
            ApplyMods(item);

            return item;
        }

        public static Item Weapon()
        {
            BaseWeapon item = Loot.RandomWeapon();
            ApplyMods(item);

            return item;
        }

        public virtual void GiveReward()
        { }
    }
}
