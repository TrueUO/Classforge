using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class Monk : BaseVendor
    {
        public override bool IsActiveVendor => false;

        private readonly List<SBInfo> _SBInfos = [];

        private DateTime m_Spoken;

        [Constructable]
        public Monk()
            : base("the Monk")
        {
            m_Spoken = DateTime.UtcNow;
        }

        public Monk(Serial serial)
            : base(serial)
        {
        }

        protected override List<SBInfo> SBInfos => _SBInfos;
        public override void InitSBInfo()
        {
        }

        public override void InitOutfit()
        {
            AddItem(new Sandals());
            AddItem(new MonkRobe());
        }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (m.Alive && m is PlayerMobile)
            {
                int range = 3;

                if (InRange(m, range) && !InRange(oldLocation, range) && DateTime.UtcNow >= m_Spoken + TimeSpan.FromMinutes(1))
                {
                    Say("Seek out the Rune Obelisk to select your class and begin your journey.");

                    m_Spoken = DateTime.UtcNow;
                }
            }
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

            m_Spoken = DateTime.UtcNow;
        }
    }
}
