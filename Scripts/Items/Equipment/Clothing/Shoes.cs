namespace Server.Items
{
    public abstract class BaseShoes : BaseClothing
    {
        public BaseShoes(int itemID)
            : this(itemID, 0)
        {
        }

        public BaseShoes(int itemID, int hue)
            : base(itemID, Layer.Shoes, hue)
        {
        }

        public BaseShoes(Serial serial)
            : base(serial)
        {
        }

        public override bool Scissor(Mobile from, Scissors scissors)
        {
            if (DefaultResource == CraftResource.None)
            {
                return base.Scissor(from, scissors);
            }

            from.SendLocalizedMessage(502440); // Scissors can not be used on that to produce anything.
            return false;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    [Flipable(0x2307, 0x2308)]
    public class FurBoots : BaseShoes
    {
        [Constructable]
        public FurBoots()
            : this(0)
        {
        }

        [Constructable]
        public FurBoots(int hue)
            : base(0x2307, hue)
        {
            Weight = 3.0;
        }

        public FurBoots(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    [Flipable(0x170b, 0x170c)]
    public class Boots : BaseShoes
    {
        public override CraftResource DefaultResource => CraftResource.RegularLeather;

        [Constructable]
        public Boots()
            : this(0)
        {
        }

        [Constructable]
        public Boots(int hue)
            : base(0x170B, hue)
        {
            Weight = 3.0;
        }

        public Boots(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    [Flipable]
    public class ThighBoots : BaseShoes
    {
        public override CraftResource DefaultResource => CraftResource.RegularLeather;

        [Constructable]
        public ThighBoots()
            : this(0)
        {
        }

        [Constructable]
        public ThighBoots(int hue)
            : base(0x1711, hue)
        {
            Weight = 4.0;
        }

        public ThighBoots(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    [Flipable(0x170f, 0x1710)]
    public class Shoes : BaseShoes
    {
        public override CraftResource DefaultResource => CraftResource.RegularLeather;

        [Constructable]
        public Shoes()
            : this(0)
        {
        }

        [Constructable]
        public Shoes(int hue)
            : base(0x170F, hue)
        {
            Weight = 2.0;
        }

        public Shoes(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    [Flipable(0x170d, 0x170e)]
    public class Sandals : BaseShoes
    {
        public override CraftResource DefaultResource => CraftResource.RegularLeather;

        [Constructable]
        public Sandals()
            : this(0)
        {
        }

        [Constructable]
        public Sandals(int hue)
            : base(0x170D, hue)
        {
            Weight = 1.0;
        }

        public Sandals(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    [Flipable(0x2797, 0x27E2)]
    public class NinjaTabi : BaseShoes
    {
        [Constructable]
        public NinjaTabi()
            : this(0)
        {
        }

        [Constructable]
        public NinjaTabi(int hue)
            : base(0x2797, hue)
        {
            Weight = 2.0;
        }

        public NinjaTabi(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    [Flipable(0x2796, 0x27E1)]
    public class SamuraiTabi : BaseShoes
    {
        [Constructable]
        public SamuraiTabi()
            : this(0)
        {
        }

        [Constructable]
        public SamuraiTabi(int hue)
            : base(0x2796, hue)
        {
            Weight = 2.0;
        }

        public SamuraiTabi(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    [Flipable(0x2796, 0x27E1)]
    public class Waraji : BaseShoes
    {
        [Constructable]
        public Waraji()
            : this(0)
        {
        }

        [Constructable]
        public Waraji(int hue)
            : base(0x2796, hue)
        {
            Weight = 2.0;
        }

        public Waraji(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    [Flipable(0x2FC4, 0x317A)]
    public class ElvenBoots : BaseShoes
    {
        public override CraftResource DefaultResource => CraftResource.RegularLeather;

        [Constructable]
        public ElvenBoots()
            : this(0)
        {
        }

        [Constructable]
        public ElvenBoots(int hue)
            : base(0x2FC4, hue)
        {
            Weight = 2.0;
        }

        public ElvenBoots(Serial serial)
            : base(serial)
        {
        }

        public override bool Dye(Mobile from, DyeTub sender)
        {
            return false;
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
    }

    public class JesterShoes : BaseShoes
    {
        public override int LabelNumber => 1109617;  // Jester Shoes

        [Constructable]
        public JesterShoes()
            : this(0)
        {
        }

        [Constructable]
        public JesterShoes(int hue)
            : base(0x7819, hue)
        {
        }

        public JesterShoes(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
