using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Bowyer : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();

        [Constructable]
        public Bowyer()
            : base("the bowyer")
        {
        }

        public Bowyer(Serial serial)
            : base(serial)
        {
        }

        public override VendorShoeType ShoeType => Female ? VendorShoeType.ThighBoots : VendorShoeType.Boots;
        protected override List<SBInfo> SBInfos => m_SBInfos;

        public override int GetShoeHue()
        {
            return 0;
        }

        public override void InitOutfit()
        {
            base.InitOutfit();

            AddItem(new Items.Bow());
            AddItem(new Items.LeatherGorget());
        }

        public override void InitSBInfo()
        {
            m_SBInfos.Add(new SBBowyer());
            m_SBInfos.Add(new SBRangedWeapon());
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
