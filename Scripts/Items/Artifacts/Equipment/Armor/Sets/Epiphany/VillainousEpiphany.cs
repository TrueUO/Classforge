namespace Server.Items
{
    public class HelmOfVillainousEpiphany : DragonHelm, IEpiphanyArmor
    {
        public Alignment Alignment => Alignment.Evil;
        public SurgeType Type => SurgeType.Mana;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int LabelNumber => 1150253;  // Helm of Villainous Epiphany

        [Constructable]
        public HelmOfVillainousEpiphany()
        {
            Resource = CraftResource.None;

            Hue = 1778;
            ArmorAttributes.MageArmor = 1;
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            EpiphanyHelper.AddProperties(this, list);
        }

        public override bool OnEquip(Mobile from)
        {
            bool canEquip = base.OnEquip(from);

            if (canEquip)
            {
                for (int index = 0; index < from.Items.Count; index++)
                {
                    Item armor = from.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }

            return canEquip;
        }

        public override void OnRemoved(object parent)
        {
            base.OnRemoved(parent);

            if (parent is Mobile m)
            {
                for (int index = 0; index < m.Items.Count; index++)
                {
                    Item armor = m.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }
        }

        public HelmOfVillainousEpiphany(Serial serial) : base(serial)
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
        }
    }

    public class GorgetOfVillainousEpiphany : PlateGorget, IEpiphanyArmor
    {
        public Alignment Alignment => Alignment.Evil;
        public SurgeType Type => SurgeType.Mana;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int LabelNumber => 1150254;  // Gorget of Villainous Epiphany

        [Constructable]
        public GorgetOfVillainousEpiphany()
        {
            Hue = 1778;
            ArmorAttributes.MageArmor = 1;
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            EpiphanyHelper.AddProperties(this, list);
        }

        public override bool OnEquip(Mobile from)
        {
            bool canEquip = base.OnEquip(from);

            if (canEquip)
            {
                for (int index = 0; index < from.Items.Count; index++)
                {
                    Item armor = from.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }

            return canEquip;
        }

        public override void OnRemoved(object parent)
        {
            base.OnRemoved(parent);

            if (parent is Mobile m)
            {
                for (int index = 0; index < m.Items.Count; index++)
                {
                    Item armor = m.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }
        }

        public GorgetOfVillainousEpiphany(Serial serial) : base(serial)
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
        }
    }

    public class BreastplateOfVillainousEpiphany : DragonChest, IEpiphanyArmor
    {
        public Alignment Alignment => Alignment.Evil;
        public SurgeType Type => SurgeType.Mana;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int LabelNumber => 1150255;  // Breastplate of Villainous Epiphany

        [Constructable]
        public BreastplateOfVillainousEpiphany()
        {
            Resource = CraftResource.None;

            Hue = 1778;
            ArmorAttributes.MageArmor = 1;
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            EpiphanyHelper.AddProperties(this, list);
        }

        public override bool OnEquip(Mobile from)
        {
            bool canEquip = base.OnEquip(from);

            if (canEquip)
            {
                for (int index = 0; index < from.Items.Count; index++)
                {
                    Item armor = from.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }

            return canEquip;
        }

        public override void OnRemoved(object parent)
        {
            base.OnRemoved(parent);

            if (parent is Mobile m)
            {
                for (int index = 0; index < m.Items.Count; index++)
                {
                    Item armor = m.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }
        }

        public BreastplateOfVillainousEpiphany(Serial serial) : base(serial)
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
        }
    }

    public class ArmsOfVillainousEpiphany : DragonArms, IEpiphanyArmor
    {
        public Alignment Alignment => Alignment.Evil;
        public SurgeType Type => SurgeType.Mana;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int LabelNumber => 1150256;  // Arms of Villainous Epiphany

        [Constructable]
        public ArmsOfVillainousEpiphany()
        {
            Resource = CraftResource.None;

            Hue = 1778;
            ArmorAttributes.MageArmor = 1;
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            EpiphanyHelper.AddProperties(this, list);
        }

        public override bool OnEquip(Mobile from)
        {
            bool canEquip = base.OnEquip(from);

            if (canEquip)
            {
                for (int index = 0; index < from.Items.Count; index++)
                {
                    Item armor = from.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }

            return canEquip;
        }

        public override void OnRemoved(object parent)
        {
            base.OnRemoved(parent);

            if (parent is Mobile m)
            {
                for (int index = 0; index < m.Items.Count; index++)
                {
                    Item armor = m.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }
        }

        public ArmsOfVillainousEpiphany(Serial serial) : base(serial)
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
        }
    }

    public class GauntletsOfVillainousEpiphany : DragonGloves, IEpiphanyArmor
    {
        public Alignment Alignment => Alignment.Evil;
        public SurgeType Type => SurgeType.Mana;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int LabelNumber => 1150257;  // Gauntlets of Villainous Epiphany

        [Constructable]
        public GauntletsOfVillainousEpiphany()
        {
            Resource = CraftResource.None;

            Hue = 1778;
            ArmorAttributes.MageArmor = 1;
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            EpiphanyHelper.AddProperties(this, list);
        }

        public override bool OnEquip(Mobile from)
        {
            bool canEquip = base.OnEquip(from);

            if (canEquip)
            {
                for (int index = 0; index < from.Items.Count; index++)
                {
                    Item armor = from.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }

            return canEquip;
        }

        public override void OnRemoved(object parent)
        {
            base.OnRemoved(parent);

            if (parent is Mobile m)
            {
                for (int index = 0; index < m.Items.Count; index++)
                {
                    Item armor = m.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }
        }

        public GauntletsOfVillainousEpiphany(Serial serial) : base(serial)
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
        }
    }

    public class LegsOfVillainousEpiphany : DragonLegs, IEpiphanyArmor
    {
        public Alignment Alignment => Alignment.Evil;
        public SurgeType Type => SurgeType.Mana;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int LabelNumber => 1150258;  // Leggings of Villainous Epiphany

        [Constructable]
        public LegsOfVillainousEpiphany()
        {
            Resource = CraftResource.None;

            Hue = 1778;
            ArmorAttributes.MageArmor = 1;
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            EpiphanyHelper.AddProperties(this, list);
        }

        public override bool OnEquip(Mobile from)
        {
            bool canEquip = base.OnEquip(from);

            if (canEquip)
            {
                foreach (Item armor in from.Items)
                {
                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }

            return canEquip;
        }

        public override void OnRemoved(object parent)
        {
            base.OnRemoved(parent);

            if (parent is Mobile m)
            {
                foreach (Item armor in m.Items)
                {
                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }
        }

        public LegsOfVillainousEpiphany(Serial serial) : base(serial)
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
        }
    }
}
