namespace Server.Items
{
    public abstract class BaseOuterTorso : BaseClothing
    {
        public BaseOuterTorso(int itemID)
            : this(itemID, 0)
        {
        }

        public BaseOuterTorso(int itemID, int hue)
            : base(itemID, Layer.OuterTorso, hue)
        {
        }

        public BaseOuterTorso(Serial serial)
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

    [Flipable(0x230E, 0x230D)]
    public class GildedDress : BaseOuterTorso
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        [Constructable]
        public GildedDress()
            : this(0)
        {
        }

        [Constructable]
        public GildedDress(int hue)
            : base(0x230E, hue)
        {
            Weight = 3.0;
        }

        public GildedDress(Serial serial)
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

    [Flipable(0x1F00, 0x1EFF)]
    public class FancyDress : BaseOuterTorso
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        [Constructable]
        public FancyDress()
            : this(0)
        {
        }

        [Constructable]
        public FancyDress(int hue)
            : base(0x1F00, hue)
        {
            Weight = 3.0;
        }

        public FancyDress(Serial serial)
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
    public class Robe : BaseOuterTorso
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        [Constructable]
        public Robe()
            : this(0)
        {
        }

        [Constructable]
        public Robe(int hue)
            : base(0x1F03, hue)
        {
            Weight = 3.0;
        }

        public Robe(Serial serial)
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

    public class MonkRobe : BaseOuterTorso
    {
        [Constructable]
        public MonkRobe()
            : this(0x21E)
        {
        }

        [Constructable]
        public MonkRobe(int hue)
            : base(0x2687, hue)
        {
            Weight = 1.0;
            StrRequirement = 0;
        }

        public override int LabelNumber => 1076584;// A monk's robe
        public override bool CanBeBlessed => false;
        public override bool Dye(Mobile from, DyeTub sender)
        {
            from.SendLocalizedMessage(sender.FailMessage);
            return false;
        }

        public MonkRobe(Serial serial)
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

    [Flipable(0x1f01, 0x1f02)]
    public class PlainDress : BaseOuterTorso
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        [Constructable]
        public PlainDress()
            : this(0)
        {
        }

        [Constructable]
        public PlainDress(int hue)
            : base(0x1F01, hue)
        {
            Weight = 2.0;
        }

        public PlainDress(Serial serial)
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

    [Flipable(0x2799, 0x27E4)]
    public class Kamishimo : BaseOuterTorso
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        [Constructable]
        public Kamishimo()
            : this(0)
        {
        }

        [Constructable]
        public Kamishimo(int hue)
            : base(0x2799, hue)
        {
            Weight = 3.0;
        }

        public Kamishimo(Serial serial)
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

    [Flipable(0x279C, 0x27E7)]
    public class HakamaShita : BaseOuterTorso
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        [Constructable]
        public HakamaShita()
            : this(0)
        {
        }

        [Constructable]
        public HakamaShita(int hue)
            : base(0x279C, hue)
        {
            Weight = 3.0;
        }

        public HakamaShita(Serial serial)
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

    [Flipable(0x2782, 0x27CD)]
    public class MaleKimono : BaseOuterTorso
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        [Constructable]
        public MaleKimono()
            : this(0)
        {
        }

        [Constructable]
        public MaleKimono(int hue)
            : base(0x2782, hue)
        {
            Weight = 3.0;
        }

        public override bool AllowFemaleWearer => false;

        public MaleKimono(Serial serial)
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

    [Flipable(0x2783, 0x27CE)]
    public class FemaleKimono : BaseOuterTorso
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        [Constructable]
        public FemaleKimono()
            : this(0)
        {
        }

        [Constructable]
        public FemaleKimono(int hue)
            : base(0x2783, hue)
        {
            Weight = 3.0;
        }

        public override bool AllowMaleWearer => false;

        public FemaleKimono(Serial serial)
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

    [Flipable(0x2FB9, 0x3173)]
    public class MaleElvenRobe : BaseOuterTorso
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        [Constructable]
        public MaleElvenRobe()
            : this(0)
        {
        }

        [Constructable]
        public MaleElvenRobe(int hue)
            : base(0x2FB9, hue)
        {
            Weight = 2.0;
        }

        public MaleElvenRobe(Serial serial)
            : base(serial)
        {
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

    [Flipable(0x2FBA, 0x3174)]
    public class FemaleElvenRobe : BaseOuterTorso
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        [Constructable]
        public FemaleElvenRobe()
            : this(0)
        {
        }

        [Constructable]
        public FemaleElvenRobe(int hue)
            : base(0x2FBA, hue)
        {
            Weight = 2.0;
        }

        public override bool AllowMaleWearer => false;

        public FemaleElvenRobe(Serial serial)
            : base(serial)
        {
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

    public class FloweredDress : BaseOuterTorso
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        public override int LabelNumber => 1109622;  // Flowered Dress

        [Constructable]
        public FloweredDress()
            : this(0)
        {
        }

        [Constructable]
        public FloweredDress(int hue)
            : base(0x781E, hue)
        {
        }

        public FloweredDress(Serial serial)
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

    public class EveningGown : BaseOuterTorso
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        public override int LabelNumber => 1109625;  // Evening Gown

        [Constructable]
        public EveningGown()
            : this(0)
        {
        }

        [Constructable]
        public EveningGown(int hue)
            : base(0x7821, hue)
        {
        }

        public EveningGown(Serial serial)
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

    public class Epaulette : BaseClothing
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        public override int LabelNumber => 1123325;  // Epaulette

        [Constructable]
        public Epaulette()
            : this(0)
        {
        }

        [Constructable]
        public Epaulette(int hue)
            : base(0x9985, Layer.OuterTorso, hue)
        {
            Weight = 1.0;
        }

        public Epaulette(Serial serial)
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
