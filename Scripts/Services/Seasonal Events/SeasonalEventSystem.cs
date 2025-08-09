using System;
using System.Collections.Generic;
using System.IO;
using Server.Commands;
using Server.Engines.JollyRoger;
using Server.Engines.RisingTide;
using Server.Engines.SorcerersDungeon;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;

namespace Server.Engines.SeasonalEvents
{
    public enum EventType
    {
        TreasuresOfTokuno,
        VirtueArtifacts,
        SorcerersDungeon,
        KrampusEncounter,
        RisingTide,
        JollyRoger
    }

    public enum EventStatus
    {
        Inactive,
        Active,
        Seasonal,
        Dynamic
    }

    public interface ISeasonalEventObject
    {
        EventType EventType { get; }
        bool EventActive { get; }
    }

    public class SeasonalEventSystem
    {
        public static string FilePath = Path.Combine("Saves/Misc", "SeasonalEvents.bin");

        public static List<SeasonalEvent> Entries { get; } = new List<SeasonalEvent>();

        public static void Configure()
        {
            LoadEntries();

            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
            EventSink.AfterWorldSave += AfterSave;

            CommandSystem.Register("SeasonSystemGump", AccessLevel.Administrator, SendGump);
        }

        public static void LoadEntries()
        {
            Entries.Add(new SeasonalEvent(EventType.TreasuresOfTokuno, "Treasures of Tokuno", EventStatus.Inactive));
            Entries.Add(new SeasonalEvent(EventType.VirtueArtifacts, "Virtue Artifacts", EventStatus.Inactive));
            Entries.Add(new SorcerersDungeonEvent(EventType.SorcerersDungeon, "Sorcerer's Dungeon", EventStatus.Inactive, 10, 1, 60));
            Entries.Add(new KrampusEvent(EventType.KrampusEncounter, "Krampus Encounter", EventStatus.Inactive, 12, 1, 60));
            Entries.Add(new RisingTideEvent(EventType.RisingTide, "Rising Tide", EventStatus.Inactive));
            Entries.Add(new JollyRogerEvent(EventType.JollyRoger, "Jolly Roger", EventStatus.Inactive));
        }

        public static void ClearEntries()
        {
            Entries.Clear();
        }

        [Usage("SeasonSystemGump")]
        [Description("Displays a menu to configure various seasonal systems.")]
        public static void SendGump(CommandEventArgs e)
        {
            if (e.Mobile is PlayerMobile mobile)
            {
                BaseGump.SendGump(new SeasonalEventGump(mobile));
            }
        }

        public static bool IsActive(EventType type)
        {
            SeasonalEvent entry = GetEvent(type);

            if (entry != null)
            {
                return entry.IsActive();
            }

            return false;
        }

        public static bool IsRunning(EventType type)
        {
            SeasonalEvent entry = GetEvent(type);

            if (entry != null)
            {
                return entry.Running;
            }

            return false;
        }

        public static TEvent GetEvent<TEvent>() where TEvent : SeasonalEvent
        {
            SeasonalEvent first = null;

            for (int index = 0; index < Entries.Count; index++)
            {
                SeasonalEvent e = Entries[index];

                if (e.GetType() == typeof(TEvent))
                {
                    first = e;
                    break;
                }
            }

            return first as TEvent;
        }

        public static SeasonalEvent GetEvent(EventType type)
        {
            for (int index = 0; index < Entries.Count; index++)
            {
                SeasonalEvent e = Entries[index];

                if (e.EventType == type)
                {
                    return e;
                }
            }

            return null;
        }

        public static void OnToTDeactivated(Mobile from)
        {
            SeasonalEvent entry = GetEvent(EventType.TreasuresOfTokuno);

            if (entry != null)
            {
                entry.Status = EventStatus.Inactive;

                if (from is PlayerMobile mobile)
                {
                    BaseGump.SendGump(new SeasonalEventGump(mobile));
                }
            }
        }

        public static void OnSave(WorldSaveEventArgs e)
        {
            Persistence.Serialize(
                FilePath,
                writer =>
                {
                    writer.Write(0);

                    writer.Write(Entries.Count);

                    for (int i = 0; i < Entries.Count; i++)
                    {
                        writer.Write((int)Entries[i].EventType);
                        Entries[i].Serialize(writer);
                    }
                });
        }

        public static void OnLoad()
        {
            Persistence.Deserialize(
                FilePath,
                reader =>
                {
                    reader.ReadInt(); // version

                    int count = reader.ReadInt();

                    for (int i = 0; i < count; i++)
                    {
                        EventType type = (EventType)reader.ReadInt();

                        SeasonalEvent entry = GetEvent(type);
                        entry.Deserialize(reader);
                    }
                });
        }

        public static void AfterSave(AfterWorldSaveEventArgs e)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                Entries[i].CheckEnabled();
            }
        }
    }

    [PropertyObject]
    public class SeasonalEvent
    {
        private EventStatus _Status;
        private int _Duration;

        [CommandProperty(AccessLevel.Administrator)]
        public virtual EventStatus Status
        {
            get => _Status;
            set
            {
                EventStatus old = _Status;

                _Status = value;

                if (old != _Status)
                {
                    CheckEnabled();
                }
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public string Name { get; }

        [CommandProperty(AccessLevel.Administrator)]
        public EventType EventType { get; }

        [CommandProperty(AccessLevel.Administrator)]
        public int MonthStart { get; set; }

        [CommandProperty(AccessLevel.Administrator)]
        public int DayStart { get; set; }

        [CommandProperty(AccessLevel.Administrator)]
        public int Duration
        {
            get => _Duration;
            set
            {
                if (!FreezeDuration)
                {
                    _Duration = value;
                }
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public bool Running { get; protected set; }

        public virtual bool FreezeDuration => false;

        public override string ToString()
        {
            return "...";
        }

        public SeasonalEvent(EventType type, string name, EventStatus status)
        {
            EventType = type;
            Name = name;
            _Status = status;
            MonthStart = 1;
            DayStart = 1;
            _Duration = 365;
        }

        public SeasonalEvent(EventType type, string name, EventStatus status, int month, int day, int duration)
        {
            EventType = type;
            Name = name;
            _Status = status;
            MonthStart = month;
            DayStart = day;
            _Duration = duration;
        }

        /// <summary>
        /// Dynamically checks if this event is active or not, based on time of year/override
        /// </summary>
        /// <returns></returns>
        public virtual bool IsActive()
        {
            // ToT uses its own system, this just reads it
            if (EventType == EventType.TreasuresOfTokuno)
            {
                return TreasuresOfTokuno.DropEra != TreasuresOfTokunoEra.None;
            }

            switch (Status)
            {
                default:
                    {
                        return false;
                    }
                case EventStatus.Active:
                    {
                        return true;
                    }
                case EventStatus.Seasonal:
                    {
                        if (Duration >= 365)
                        {
                            return true;
                        }

                        DateTime now = DateTime.Now;
                        DateTime starts = new DateTime(now.Year, MonthStart, DayStart, 0, 0, 0);

                        if (Duration == -1)
                        {
                            return now.Month == MonthStart && now.Day == DayStart;
                        }

                        return now > starts && now < starts + TimeSpan.FromDays(Duration);
                    }
            }
        }

        public virtual void CheckEnabled()
        {
            if (Running && !IsActive())
            {
                Utility.WriteConsoleColor(ConsoleColor.Green, $"Disabling {Name}");

                Remove();
            }
            else if (!Running && IsActive())
            {
                Utility.WriteConsoleColor(ConsoleColor.Green, $"Enabling {Name}");

                Generate();
            }

            Running = IsActive();
        }

        protected virtual void Generate()
        {
        }

        protected virtual void Remove()
        {
        }

        public virtual void Serialize(GenericWriter writer)
        {
            writer.Write(1);

            writer.Write(Running);

            writer.Write((int)_Status);

            writer.Write(MonthStart);
            writer.Write(DayStart);
            writer.Write(Duration);
        }

        public virtual void Deserialize(GenericReader reader)
        {
            int v = reader.ReadInt(); // version

            switch (v)
            {
                case 1:
                {
                    Running = reader.ReadBool();
                    goto case 0;
                }
                case 0:
                {
                    _Status = (EventStatus)reader.ReadInt();

                    MonthStart = reader.ReadInt();
                    DayStart = reader.ReadInt();
                    _Duration = reader.ReadInt();
                    break;
                }
            }

            if (v == 0)
            {
                Running = IsActive();
                InheritInsertion = true;
            }
        }

        protected bool InheritInsertion = false;
    }
}
