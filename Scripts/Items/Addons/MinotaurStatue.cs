using Server.Gumps;

namespace Server.Items
{
    public enum MinotaurStatueType
    {
        AttackSouth = 100,
        AttackEast = 101,
        DefendSouth = 102,
        DefendEast = 103
    }

    public class MinotaurStatue : BaseAddon
    {
        [Constructable]
        public MinotaurStatue(MinotaurStatueType type)
        {
            switch (type)
            {
                case MinotaurStatueType.AttackSouth:
                {
                    AddComponent(new AddonComponent(0x306C), 0, 0, 0);
                    AddComponent(new AddonComponent(0x306D), -1, 0, 0);
                    AddComponent(new AddonComponent(0x306E), 0, -1, 0);
                    break;
                }
                case MinotaurStatueType.AttackEast:
                {
                    AddComponent(new AddonComponent(0x3074), 0, 0, 0);
                    AddComponent(new AddonComponent(0x3075), -1, 0, 0);
                    AddComponent(new AddonComponent(0x3076), 0, -1, 0);
                    break;
                }
                case MinotaurStatueType.DefendSouth:
                {
                    AddComponent(new AddonComponent(0x3072), 0, 0, 0);
                    AddComponent(new AddonComponent(0x3073), 0, -1, 0);
                    break;
                }
                case MinotaurStatueType.DefendEast:
                {
                    AddComponent(new AddonComponent(0x306F), 0, 0, 0);
                    AddComponent(new AddonComponent(0x3070), -1, 0, 0);
                    AddComponent(new AddonComponent(0x3071), 0, -1, 0);
                    break;
                }
            }
        }

        public MinotaurStatue(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddonDeed Deed => new MinotaurStatueDeed();

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class MinotaurStatueDeed : BaseAddonDeed, IRewardOption
    {
        private MinotaurStatueType m_StatueType;

        [Constructable]
        public MinotaurStatueDeed()
        {
            LootType = LootType.Blessed;
        }

        public MinotaurStatueDeed(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1080409;// Minotaur Statue Deed

        public override BaseAddon Addon => new MinotaurStatue(m_StatueType);

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.CloseGump(typeof(RewardOptionGump));
                from.SendGump(new RewardOptionGump(this));
            }
            else
            {
                from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.    
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }

        public void GetOptions(RewardOptionList list)
        {
            list.Add((int)MinotaurStatueType.AttackSouth, 1080410); // Minotaur Attack South
            list.Add((int)MinotaurStatueType.AttackEast, 1080411); // Minotaur Attack East
            list.Add((int)MinotaurStatueType.DefendSouth, 1080412); // Minotaur Defend South
            list.Add((int)MinotaurStatueType.DefendEast, 1080413); // Minotaur Defend East
        }

        public void OnOptionSelected(Mobile from, int option)
        {
            m_StatueType = (MinotaurStatueType)option;

            if (!Deleted)
            {
                base.OnDoubleClick(from);
            }
        }
    }
}
