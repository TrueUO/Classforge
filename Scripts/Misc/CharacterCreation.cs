using System;
using Server.Accounting;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Misc
{
    public class CharacterCreationArguments(
        NetState state,
        IAccount a,
        string name,
        bool female,
        int hue,
        int str,
        int dex,
        int intel,
        CityInfo city,
        SkillNameValue[] skills,
        int shirtHue,
        int pantsHue,
        int hairId,
        int hairHue,
        int beardId,
        int beardHue,
        int profession,
        Race race,
        int faceId,
        int faceHue)
    {
        public NetState State { get; } = state;
        public IAccount Account { get; } = a;
        public Mobile Mobile { get; set; }
		public string Name { get; } = name;
        public bool Female { get; } = female;
        public int Hue { get; } = hue;
        public int Str { get; } = str;
        public int Dex { get; } = dex;
        public int Int { get; } = intel;
        public CityInfo City { get; } = city;
        public SkillNameValue[] Skills { get; } = skills;
        public int ShirtHue { get; } = shirtHue;
        public int PantsHue { get; } = pantsHue;
        public int HairID { get; } = hairId;
        public int HairHue { get; } = hairHue;
        public int BeardID { get; } = beardId;
        public int BeardHue { get; } = beardHue;
        public int Profession { get; set; } = profession;
        public Race Race { get; } = race;
        public int FaceID { get; } = faceId;
        public int FaceHue { get; } = faceHue;

        public CharacterCreationArguments(
			NetState state,
			IAccount a,
			string name,
			bool female,
			int hue,
			int str,
			int dex,
			int intel,
			CityInfo city,
			SkillNameValue[] skills,
			int shirtHue,
			int pantsHue,
			int hairID,
			int hairHue,
			int beardID,
			int beardHue,
			int profession,
			Race race)
			: this(state, a, name, female, hue, str, dex, intel, city, skills, shirtHue, pantsHue, hairID, hairHue, beardID, beardHue, profession, race, 0, 0)
		{
		}
    }

    public class CharacterCreation
    {
        private static Mobile m_Mobile;

        private static void AddBackpackAndStartingRobe(Mobile m)
        {
            Container pack = m.Backpack;

            if (pack == null)
            {
                pack = new Backpack
                {
                    Movable = false
                };

                m.AddItem(pack);
            }

            MonkRobe monkRobe = new MonkRobe
            {
                Movable = false
            };
            EquipItem(monkRobe);

            Sandals sandals = new Sandals
            {
                Movable = false
            };
            EquipItem(sandals);
        }

        private static Mobile CreateMobile(Account a)
        {
            if (a.Count >= a.Limit)
            {
                return null;
            }

            for (int i = 0; i < a.Length; ++i)
            {
                if (a[i] == null)
                {
                    return a[i] = new PlayerMobile();
                }
            }

            return null;
        }

        public static void OnCharacterCreation(CharacterCreationArguments args)
        {
            NetState state = args.State;

            if (state == null)
            {
                return;
            }

            Mobile newChar = CreateMobile(args.Account as Account);

            if (newChar == null)
            {
                Utility.PushColor(ConsoleColor.Red);
                Console.WriteLine("Login: {0}: Character creation failed, account full", state);
                Utility.PopColor();
                return;
            }

            args.Mobile = newChar;
            m_Mobile = newChar;

            newChar.Player = true;
            newChar.AccessLevel = args.Account.AccessLevel;
            newChar.Female = args.Female;

            newChar.Race = Race.Human; // always Human

            newChar.Hue = args.Hue | 0x8000;

            newChar.Hunger = 20;

            if (newChar is PlayerMobile pm)
            {
                // Set new players to level to 1 for Custom Classes system
                pm.Level = 1;

                double skillcap = Config.Get("PlayerCaps.SkillCap", 1000.0d) / 10;

                if (skillcap != 100.0)
                {
                    for (int i = 0; i < Enum.GetNames(typeof(SkillName)).Length; ++i)
                    {
                        pm.Skills[i].Cap = skillcap;
                    }
                }
            }

            SetName(newChar, args.Name);

            newChar.InitStats(20, 20, 20); // STR DEX INT

            AddBackpackAndStartingRobe(newChar);

            Race race = newChar.Race;

            if (race.ValidateHair(newChar, args.HairID))
            {
                newChar.HairItemID = args.HairID;
                newChar.HairHue = args.HairHue;
            }

            if (race.ValidateFacialHair(newChar, args.BeardID))
            {
                newChar.FacialHairItemID = args.BeardID;
                newChar.FacialHairHue = args.BeardHue;
            }

            int faceID = args.FaceID;

            if (faceID > 0 && race.ValidateFace(newChar.Female, faceID))
            {
                newChar.FaceItemID = faceID;
                newChar.FaceHue = args.FaceHue;
            }
            else
            {
                newChar.FaceItemID = race.RandomFace(newChar.Female);
                newChar.FaceHue = newChar.Hue;
            }

            if (TestCenter.Enabled)
            {
                TestCenter.FillBankbox(newChar);
            }

            CityInfo city = args.City;
            Map map = city.Map;

            newChar.MoveToWorld(city.Location, map);

            // so when sitting in the church they face the right direction
            newChar.Direction = Direction.West;

            Utility.PushColor(ConsoleColor.Green);
            Console.WriteLine("Login: {0}: New character being created (account={1})", state, args.Account.Username);
            Utility.PopColor();
            Utility.PushColor(ConsoleColor.DarkGreen);
            Console.WriteLine(" - Character: {0} (serial={1})", newChar.Name, newChar.Serial);
            Console.WriteLine(" - Started: {0} {1} in {2}", city.City, city.Location, city.Map);
            Utility.PopColor();
        }

        private static void SetName(Mobile m, string name)
        {
            name = name.Trim();

            if (!NameVerification.Validate(name, 2, 16, true, false, true, 1, NameVerification.SpaceDashPeriodQuote))
            {
                name = "Generic Player";
            }

            m.Name = name;
        }

        private static void EquipItem(Item item)
        {
            EquipItem(item, false);
        }

        private static void EquipItem(Item item, bool mustEquip)
        {
            if (m_Mobile != null && m_Mobile.EquipItem(item))
            {
                return;
            }

            Container pack = m_Mobile.Backpack;

            if (!mustEquip && pack != null)
            {
                pack.DropItem(item);
            }
            else
            {
                item.Delete();
            }
        }

        private static void PackItem(Item item)
        {
            Container pack = m_Mobile.Backpack;

            if (pack != null)
            {
                pack.DropItem(item);
            }
            else
            {
                item.Delete();
            }
        }
    }
}
