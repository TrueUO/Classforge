namespace Server.Misc
{
    public class TreasuresOfTokunoPersistence : Item
    {
        private static TreasuresOfTokunoPersistence m_Instance;
        public TreasuresOfTokunoPersistence()
            : base(1)
        {
            Movable = false;

            if (m_Instance == null || m_Instance.Deleted)
            {
                m_Instance = this;
            }
            else
            {
                base.Delete();
            }
        }

        public TreasuresOfTokunoPersistence(Serial serial)
            : base(serial)
        {
            m_Instance = this;
        }

        public static TreasuresOfTokunoPersistence Instance => m_Instance;
        public override string DefaultName => "TreasuresOfTokuno Persistance - Internal";
        public static void Initialize()
        {
            if (m_Instance == null)
            {
                new TreasuresOfTokunoPersistence();
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.WriteEncodedInt((int)TreasuresOfTokuno.RewardEra);
            writer.WriteEncodedInt((int)TreasuresOfTokuno.DropEra);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            TreasuresOfTokuno.RewardEra = (TreasuresOfTokunoEra)reader.ReadEncodedInt();
            TreasuresOfTokuno.DropEra = (TreasuresOfTokunoEra)reader.ReadEncodedInt();
        }

        public override void Delete()
        {
        }
    }
}
