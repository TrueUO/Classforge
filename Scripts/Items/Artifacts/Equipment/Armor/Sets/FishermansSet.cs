namespace Server.Items
{
    public interface IFishingAttire
    {
        int BaitBonus { get; set; }
        int SetBonus { get; set; }
    }

    public class FishermansHat : TallStrawHat, ISetItem, IFishingAttire
    {
        public override int LabelNumber => 1151190;  //Fisherman's Tall Straw Hat

        public override SetItem SetID => SetItem.Fisherman;
        public override int Pieces => 4;

        public int BaitBonus { get { return 10; } set { } }
        public int SetBonus { get { return 50; } set { } }

        public override int InitMinHits => 125;
        public override int InitMaxHits => 125;

        [Constructable]
        public FishermansHat()
        {
            Hue = 2578;
            SetHue = 2578;
        }

        public FishermansHat(Serial serial) : base(serial)
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
            int version = reader.ReadInt();
        }
    }

    public class FishermansTrousers : StuddedLegs, ISetItem, IFishingAttire
    {
        public override int LabelNumber => 1151191;  //Fisherman's Trousers

        public override SetItem SetID => SetItem.Fisherman;
        public override int Pieces => 4;

        public int BaitBonus { get { return 10; } set { } }
        public int SetBonus { get { return 50; } set { } }

        public override int InitMinHits => 125;
        public override int InitMaxHits => 125;

        [Constructable]
        public FishermansTrousers()
        {
            Hue = 2578;
            SetHue = 2578;

            ArmorAttributes.MageArmor = 1;
        }

        public FishermansTrousers(Serial serial)
            : base(serial)
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

    public class FishermansVest : LeatherChest, ISetItem, IFishingAttire
    {
        public override int LabelNumber => 1151192;  //Fisherman's Vest

        public override SetItem SetID => SetItem.Fisherman;
        public override int Pieces => 4;

        public int BaitBonus { get { return 10; } set { } }
        public int SetBonus { get { return 50; } set { } }

        public override int InitMinHits => 125;
        public override int InitMaxHits => 125;

        [Constructable]
        public FishermansVest()
        {
            Hue = 2578;
            SetHue = 2578;
        }

        public FishermansVest(Serial serial)
            : base(serial)
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
            int version = reader.ReadInt();
        }
    }

    public class FishermansEelskinGloves : LeatherGloves, ISetItem, IFishingAttire
    {
        public override int LabelNumber => 1151189;  //Fisherman's Eelskin Gloves

        public override SetItem SetID => SetItem.Fisherman;
        public override int Pieces => 4;

        public int BaitBonus { get { return 10; } set { } }
        public int SetBonus { get { return 50; } set { } }

        public override int InitMinHits => 125;
        public override int InitMaxHits => 125;

        [Constructable]
        public FishermansEelskinGloves()
        {
            Hue = 2578;
            SetHue = 2578;
        }

        public FishermansEelskinGloves(Serial serial)
            : base(serial)
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
            int version = reader.ReadInt();
        }
    }
}
