using System;
using System.Collections.Generic;
using System.Xml;
using Server.Engines.Quests;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Spells;

namespace Server.Regions
{
    public class SeaMarketRegion : BaseRegion
    {
        private static SeaMarketRegion _Region1;
        private static SeaMarketRegion _Region2;

        private static Timer m_BlabTimer;

        public static Rectangle2D[] Bounds => _Bounds;

        private static readonly Rectangle2D[] _Bounds =
        {
            new Rectangle2D(4529, 2296, 45, 112)
        };

        public SeaMarketRegion(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
        }

        public override void OnRegister()
        {
            if (_Region1 == null)
            {
                _Region1 = this;
            }
            else if (_Region2 == null)
            {
                _Region2 = this;
            }
        }

        public override bool CheckTravel(Mobile traveller, Point3D p, TravelCheckType type)
        {
            switch (type)
            {
                case TravelCheckType.RecallTo:
                case TravelCheckType.GateTo:
                    {
                        return BaseBoat.FindBoatAt(p, Map) != null;
                    }
                case TravelCheckType.Mark:
                    {
                        return false;
                    }
            }

            return base.CheckTravel(traveller, p, type);
        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            return false;
        }

        public static Dictionary<Mobile, DateTime> m_PirateBlabTable = new Dictionary<Mobile, DateTime>();
        private static readonly TimeSpan _BlabDuration = TimeSpan.FromMinutes(1);

        public static void TryPirateBlab(Mobile from, Mobile npc)
        {
            if (m_PirateBlabTable.ContainsKey(from) && m_PirateBlabTable[from] > DateTime.UtcNow || BountyQuestSpawner.Bounties.Count <= 0)
            {
                return;
            }

            //Make of list of bounties on their map
            List<Mobile> bounties = new List<Mobile>();
            foreach (Mobile mob in BountyQuestSpawner.Bounties.Keys)
            {
                if (mob.Map == from.Map && mob is PirateCaptain && !bounties.Contains(mob))
                {
                    bounties.Add(mob);
                }
            }

            if (bounties.Count > 0)
            {
                Mobile bounty = bounties[Utility.Random(bounties.Count)];

                if (bounty != null)
                {
                    PirateCaptain capt = (PirateCaptain)bounty;

                    int xLong = 0, yLat = 0;
                    int xMins = 0, yMins = 0;
                    bool xEast = false, ySouth = false;
                    Point3D loc = capt.Location;
                    Map map = capt.Map;

                    string locArgs;

                    if (Sextant.Format(loc, map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth))
                    {
                        locArgs = $"{yLat}°{yMins}'{(ySouth ? "S" : "N")},{xLong}°{xMins}'{(xEast ? "E" : "W")}";
                    }
                    else
                    {
                        locArgs = "?????";
                    }

                    string combine = $"{(capt.PirateName > -1 ? $"#{capt.PirateName}" : capt.Name)}\t{locArgs}";

                    int cliloc = Utility.RandomMinMax(1149856, 1149865);
                    npc.SayTo(from, cliloc, combine);

                    m_PirateBlabTable[from] = DateTime.UtcNow + _BlabDuration;
                }
            }

            ColUtility.Free(bounties);
        }

        public static void CheckBlab_Callback()
        {
            CheckBabble(_Region1);
            CheckBabble(_Region2);
        }

        public static void CheckBabble(Region r)
        {
            if (r == null)
            {
                return;
            }

            foreach (Mobile player in r.GetEnumeratedMobiles())
            {
                if (player is PlayerMobile && player.Alive)
                {
                    IPooledEnumerable eable = player.GetMobilesInRange(4);

                    foreach (Mobile mob in eable)
                    {
                        if (mob is BaseVendor || mob is GalleonPilot)
                        {
                            TryPirateBlab(player, mob);
                            break;
                        }
                    }

                    eable.Free();
                }
            }
        }

        public static void StartTimers_Callback()
        {
            m_BlabTimer = Timer.DelayCall(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), CheckBlab_Callback);
            m_BlabTimer.Start();
        }

        public static void Save(GenericWriter writer)
        {
            writer.Write(0);
        }

        public static void Load(GenericReader reader)
        {
            reader.ReadInt();

            Timer.DelayCall(TimeSpan.FromSeconds(30), StartTimers_Callback);
        }
    }
}
