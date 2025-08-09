using System;
using System.Collections.Generic;
using System.Linq;
using Server.Engines.Points;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.JollyRoger
{
    public class JollyRogerData : PointsSystem
    {
        private static List<RewardArray> _List = new List<RewardArray>();

        public override PointsType Loyalty => PointsType.JollyRogerData;
        public override TextDefinition Name => "Jolly Roger";
        public override bool AutoAdd => true;
        public override double MaxPoints => double.MaxValue;
        public override bool ShowOnLoyaltyGump => false;

        public static void Initialize()
        {
            EventSink.Speech += EventSink_Speech;
        }

        public static void EventSink_Speech(SpeechEventArgs e)
        {
            string speech = e.Speech;
            Mobile m = e.Mobile;

            if (m.Region.Name == "Chaos Shrine" && !m.HasGump(typeof(RenounceVirtueGump)) && m is PlayerMobile pm && ShrineTitles.ContainsKey(pm) &&
                speech.IndexOf("I renounce virtue", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                m.SendGump(new RenounceVirtueGump());
            }
        }

        public static Dictionary<PlayerMobile, int> ShrineTitles { get; } = new Dictionary<PlayerMobile, int>();

        private static readonly List<ShrineDef> ShrineDef = new List<ShrineDef>
        {
            new ShrineDef(Shrine.Spirituality, 2500, 1159321),
            new ShrineDef(Shrine.Compassion, 1912, 1159327),
            new ShrineDef(Shrine.Honor, 1918, 1159325),
            new ShrineDef(Shrine.Honesty, 1916, 1159326),
            new ShrineDef(Shrine.Humility, 1910, 1159324),
            new ShrineDef(Shrine.Justice, 1914 , 1159323),
            new ShrineDef(Shrine.Valor, 1920, 1159320),
            new ShrineDef(Shrine.Sacrifice, 1922, 1159322)
        };

        public static void DisplayTitle(PlayerMobile pm, ObjectPropertyList list)
        {
            int title = GetShrineTitle(pm);

            if (title > 0)
            {
                list.Add(title);
            }
        }

        public static int GetShrineTitle(PlayerMobile pm)
        {
            if (ShrineTitles.TryGetValue(pm, out int value))
            {
                return value;
            }

            return 0;
        }

        public static void SetShrineTitle(PlayerMobile pm, int title)
        {
            ShrineTitles[pm] = title;
            pm.InvalidateProperties();
        }

        public static RewardArray GetList(Mobile m)
        {
            return _List.FirstOrDefault(x => x.Mobile == m);
        }

        public static void SetCloak(Mobile m, bool b)
        {
            GetList(m).Cloak = b;
        }

        public static void SetTabard(Mobile m, bool t)
        {
            GetList(m).Tabard = t;
        }

        public static int FragmentRandomHue()
        {
            List<int> list = new List<int>();

            for (int index = 0; index < ShrineDef.Count; index++)
            {
                ShrineDef x = ShrineDef[index];

                list.Add(x.Hue);
            }

            return Utility.RandomList(list.ToArray());
        }

        public static int GetShrineHue(Shrine shrine)
        {
            ShrineDef shrineDef = null;

            for (int index = 0; index < ShrineDef.Count; index++)
            {
                ShrineDef x = ShrineDef[index];

                if (x.Shrine == shrine)
                {
                    shrineDef = x;
                    break;
                }
            }

            if (shrineDef != null)
            {
                return shrineDef.Hue;
            }

            return default;
        }

        public static Shrine GetShrine(int cliloc)
        {
            return ShrineDef.Find(x => x.TitleCliloc == cliloc).Shrine;
        }

        public static Shrine GetShrine(Item item)
        {
            ShrineDef first = null;

            for (int index = 0; index < ShrineDef.Count; index++)
            {
                ShrineDef x = ShrineDef[index];

                if (x.Hue == item.Hue)
                {
                    first = x;
                    break;
                }
            }

            if (first != null)
            {
                return first.Shrine;
            }

            return default;
        }

        public static int GetTitle(Shrine shrine)
        {
            ShrineDef first = null;

            for (int index = 0; index < ShrineDef.Count; index++)
            {
                ShrineDef x = ShrineDef[index];

                if (x.Shrine == shrine)
                {
                    first = x;
                    break;
                }
            }

            if (first != null)
            {
                return first.TitleCliloc;
            }

            return default;
        }

        public static void AddMasterKill(Mobile m, Shrine shrine)
        {
            RewardArray list = null;

            for (int index = 0; index < _List.Count; index++)
            {
                RewardArray x = _List[index];

                if (x.Mobile == m)
                {
                    list = x;
                    break;
                }
            }

            if (list != null && list.Shrine != null)
            {
                if (list.Shrine.Any(y => y.Shrine == shrine))
                {
                    RewardArray reward = null;

                    for (int index = 0; index < _List.Count; index++)
                    {
                        RewardArray x = _List[index];

                        if (x.Mobile == m)
                        {
                            reward = x;
                            break;
                        }
                    }

                    ShrineArray shrineArray = null;

                    if (reward != null)
                    {
                        for (int index = 0; index < reward.Shrine.Count; index++)
                        {
                            ShrineArray y = reward.Shrine[index];

                            if (y.Shrine == shrine)
                            {
                                shrineArray = y;
                                break;
                            }
                        }
                    }

                    if (shrineArray != null)
                    {
                        shrineArray.MasterDeath++;
                    }
                }
                else
                {
                    _List.FirstOrDefault(x => x.Mobile == m).Shrine.Add(new ShrineArray { Shrine = shrine, MasterDeath = 1 });
                }
            }
            else
            {
                List<ShrineArray> sa = new List<ShrineArray>
                {
                    new ShrineArray {Shrine = shrine, MasterDeath = 1 }
                };

                RewardArray ra = new RewardArray(m, sa);

                _List.Add(ra);
            }
        }

        public static void FragmentIncrease(Mobile m, Shrine shrine)
        {
            if (m == null)
            {
                return;
            }

            RewardArray list = null;

            for (int index = 0; index < _List.Count; index++)
            {
                RewardArray x = _List[index];

                if (x.Mobile == m)
                {
                    list = x;
                    break;
                }
            }

            if (list != null && list.Shrine != null)
            {
                if (list.Shrine.Any(y => y.Shrine == shrine))
                {
                    _List.FirstOrDefault(x => x.Mobile == m).Shrine.FirstOrDefault(y => y.Shrine == shrine).FragmentCount++;
                }
                else
                {
                    _List.FirstOrDefault(x => x.Mobile == m).Shrine.Add(new ShrineArray { Shrine = shrine, FragmentCount = 1 });
                }
            }
            else
            {
                List<ShrineArray> sa = new List<ShrineArray>
                {
                    new ShrineArray {Shrine = shrine, FragmentCount = 1 }
                };

                RewardArray ra = new RewardArray(m, sa);

                _List.Add(ra);
            }

            TitleCheck(m, shrine);
        }

        public static void TitleCheck(Mobile m, Shrine shrine)
        {
            RewardArray list = null;

            for (int index = 0; index < _List.Count; index++)
            {
                RewardArray x = _List[index];

                if (x.Mobile == m)
                {
                    list = x;
                    break;
                }
            }

            if (m is PlayerMobile pm && list != null && list.Shrine != null)
            {
                ShrineArray first = null;

                for (int index = 0; index < list.Shrine.Count; index++)
                {
                    ShrineArray x = list.Shrine[index];

                    if (x.Shrine == shrine)
                    {
                        first = x;
                        break;
                    }
                }

                if (first != null)
                {
                    int count = first.FragmentCount;
                    int playerTitle = GetShrineTitle(pm);
                    int shrineTitle = GetTitle(shrine);

                    bool hasTitle = false;

                    for (int index = 0; index < list.Shrine.Count; index++)
                    {
                        ShrineArray x = list.Shrine[index];

                        if (x.FragmentCount < count && x.Shrine != shrine)
                        {
                            hasTitle = true;
                            break;
                        }
                    }

                    if (playerTitle == 0 || playerTitle != shrineTitle && hasTitle)
                    {
                        SetShrineTitle(pm, shrineTitle);
                    }
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);

            writer.Write(_List.Count);

            for (int index = 0; index < _List.Count; index++)
            {
                RewardArray l = _List[index];

                writer.Write(l.Mobile);
                writer.Write(l.Tabard);
                writer.Write(l.Cloak);

                writer.Write(l.Shrine.Count);

                for (int i = 0; i < l.Shrine.Count; i++)
                {
                    ShrineArray s = l.Shrine[i];

                    writer.Write((int) s.Shrine);
                    writer.Write(s.FragmentCount);
                    writer.Write(s.MasterDeath);
                }
            }

            writer.Write(ShrineTitles.Count);

            foreach (KeyValuePair<PlayerMobile, int> kvp in ShrineTitles)
            {
                writer.WriteMobile(kvp.Key);
                writer.Write(kvp.Value);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                case 0:
                {
                    if (version == 0)
                    {
                        reader.ReadBool();
                        bool questGenerated = reader.ReadBool();

                        Timer.DelayCall(() =>
                        {
                            JollyRogerEvent jolly = SeasonalEvents.SeasonalEventSystem.GetEvent<JollyRogerEvent>();

                            if (jolly != null)
                            {
                                jolly.QuestContentGenerated = questGenerated;
                            }
                        });
                    }

                    int count = reader.ReadInt();

                    for (int i = count; i > 0; i--)
                    {
                        Mobile m = reader.ReadMobile();
                        bool t = reader.ReadBool();
                        bool c = reader.ReadBool();

                        List<ShrineArray> temp = new List<ShrineArray>();

                        int scount = reader.ReadInt();

                        for (int s = scount; s > 0; s--)
                        {
                            Shrine sh = (Shrine)reader.ReadInt();
                            int fc = reader.ReadInt();
                            int md = reader.ReadInt();

                            temp.Add(new ShrineArray { Shrine = sh, FragmentCount = fc, MasterDeath = md });
                        }

                        if (m != null)
                        {
                            _List.Add(new RewardArray(m, temp, t, c));
                        }
                    }

                    count = reader.ReadInt();

                    for (int i = 0; i < count; i++)
                    {
                        PlayerMobile pm = reader.ReadMobile<PlayerMobile>();
                        int title = reader.ReadInt();

                        if (pm != null)
                        {
                            ShrineTitles[pm] = title;
                        }
                    }
                    break;
                }
            }
        }
    }

    public class ShrineDef
    {
        public Shrine Shrine { get; }
        public int Hue { get; }
        public int TitleCliloc { get; }

        public ShrineDef(Shrine s, int h, int tc)
        {
            Shrine = s;
            Hue = h;
            TitleCliloc = tc;
        }
    }

    public class RewardArray
    {
        public Mobile Mobile { get; }
        public List<ShrineArray> Shrine { get; }
        public bool Tabard { get; set; }
        public bool Cloak { get; set; }

        public RewardArray(Mobile m, List<ShrineArray> s)
        {
            Mobile = m;
            Shrine = s;
        }

        public RewardArray(Mobile m, List<ShrineArray> s, bool tabard, bool cloak)
        {
            Mobile = m;
            Shrine = s;
            Tabard = tabard;
            Cloak = cloak;
        }
    }

    public class ShrineArray
    {
        public Shrine Shrine { get; set; }
        public int FragmentCount { get; set; }
        public int MasterDeath { get; set; }
    }

    public class RenounceVirtueGump : Gump
    {
        public RenounceVirtueGump()
            : base(100, 100)
        {
            AddPage(0);

            AddBackground(0, 0, 320, 245, 0x6DB);
            AddHtmlLocalized(65, 10, 200, 20, 1114513, "#1159452", 0x67D5, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
            AddHtmlLocalized(15, 50, 295, 100, 1159453, 0x72ED, false, false); // You are about to renounce your Shrine Battle virtue title. You may recover the title by placing a mysterious fragment at any shrine. Do you wish to proceed?
            AddButton(30, 200, 0x867, 0x869, 1, GumpButtonType.Reply, 0);
            AddButton(265, 200, 0x867, 0x869, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(33, 180, 100, 50, 1046362, 0x7FFF, false, false); // Yes
            AddHtmlLocalized(273, 180, 100, 50, 1046363, 0x7FFF, false, false); // No
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            switch (info.ButtonID)
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {

                        if (sender.Mobile is PlayerMobile pm && JollyRogerData.ShrineTitles.ContainsKey(pm))
                        {
                            JollyRogerData.ShrineTitles.Remove(pm);
                            pm.InvalidateProperties();
                        }

                        break;
                    }
            }
        }
    }
}
