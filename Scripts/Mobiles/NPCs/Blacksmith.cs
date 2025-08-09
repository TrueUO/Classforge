using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Blacksmith : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
        protected override List<SBInfo> SBInfos => m_SBInfos;

        [Constructable]
        public Blacksmith()
            : base("the blacksmith")
        {
        }

        public override void InitSBInfo()
        {
            m_SBInfos.Add(new SBBlacksmith());
        }

        public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Sandals : VendorShoeType.Shoes;

        public override void InitOutfit()
        {
            Item item = Utility.RandomBool() ? null : new Items.RingmailChest();

            if (item != null && !EquipItem(item))
            {
                item.Delete();
                item = null;
            }

            if (item == null)
            {
                AddItem(new Items.FullApron());
            }

            AddItem(new Items.Bascinet());
            AddItem(new Items.SmithHammer());

            base.InitOutfit();
        }

        public Blacksmith(Serial serial)
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
