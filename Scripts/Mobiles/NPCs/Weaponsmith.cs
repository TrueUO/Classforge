using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Weaponsmith : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
        protected override List<SBInfo> SBInfos => m_SBInfos;

        [Constructable]
        public Weaponsmith()
            : base("the weaponsmith")
        {
        }

        public override void InitSBInfo()
        {
            m_SBInfos.Add(new SBWeaponSmith());
        }

        public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Boots : VendorShoeType.ThighBoots;

        public override int GetShoeHue()
        {
            return 0;
        }

        public override void InitOutfit()
        {
            base.InitOutfit();

            AddItem(new Items.HalfApron());
        }

        public Weaponsmith(Serial serial)
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
