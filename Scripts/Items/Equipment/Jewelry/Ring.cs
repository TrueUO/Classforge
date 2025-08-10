using Server.Engines.Craft;

namespace Server.Items
{
    public abstract class BaseRing : BaseJewel
    {
        public BaseRing(int itemID)
            : base(itemID, Layer.Ring)
        {
        }

        public BaseRing(Serial serial)
            : base(serial)
        {
        }

        public override int BaseGemTypeNumber => 1044176;// star sapphire ring

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(2); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class GoldRing : BaseRing
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        [Constructable]
        public GoldRing()
            : base(0x108a)
        {

        }

        public GoldRing(Serial serial)
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

    public class SilverRing : BaseRing, IRepairable
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        public CraftSystem RepairSystem => DefTinkering.CraftSystem;

        [Constructable]
        public SilverRing()
            : base(0x1F09)
        {

        }

        public SilverRing(Serial serial)
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
