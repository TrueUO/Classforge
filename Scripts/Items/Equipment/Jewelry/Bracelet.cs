using Server.Engines.Craft;

namespace Server.Items
{
    public abstract class BaseBracelet : BaseJewel
    {
        public BaseBracelet(int itemID)
            : base(itemID, Layer.Bracelet)
        {
        }

        public BaseBracelet(Serial serial)
            : base(serial)
        {
        }

        public override int BaseGemTypeNumber => 1044221;// star sapphire bracelet

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

    public class GoldBracelet : BaseBracelet
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        [Constructable]
        public GoldBracelet()
            : base(0x1086)
        {
        }

        public GoldBracelet(Serial serial)
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

    public class SilverBracelet : BaseBracelet, IRepairable
    {
        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        public CraftSystem RepairSystem => DefTinkering.CraftSystem;

        [Constructable]
        public SilverBracelet()
            : base(0x1F06)
        {
        }

        public SilverBracelet(Serial serial)
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
