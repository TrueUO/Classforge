using System;
using Server.Items;
using Server.Mobiles;
using Server.Services.ClassSystem;

namespace Server.Engines.Quests
{
    public class DoughtyWarriorsQuest : BaseQuest
    {
        public override QuestChain ChainID => QuestChain.DoughtyWarriors;
        public override Type NextQuest => typeof(DoughtyWarriors2Quest);
        public override bool DoneOnce => true;

        public override object Title => "The Classforge";

        public override object Description => "Kill sheep";

        public override object Refuse => "";

        public override object Uncomplete => "";

        public override object Complete => "";

        public DoughtyWarriorsQuest()
        {
            AddObjective(new SlayObjective(typeof(Sheep), "Sheep", 5));

            AddReward(new BaseReward("+25 XP"));
            AddReward(new BaseReward("The famous sword: Life Stealer"));
        }

        public override void GiveRewards()
        {
            base.GiveRewards();

            ClassSystemHelper.AwardXp(Owner, 25, "a quest");

            Broadsword sword = new Broadsword
            {
                Name = "Life Stealer",
                WeaponAttributes =
                {
                    HitLeechHits = 5
                },
                MaxHitPoints = 75,
                HitPoints = 75,
                Owner = Owner
            };

            Owner.AddToBackpack(sword);
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

    public class DoughtyWarriors2Quest : BaseQuest
    {
        public override QuestChain ChainID => QuestChain.DoughtyWarriors;
        public override Type NextQuest => typeof(DoughtyWarriors3Quest);
        public override bool DoneOnce => true;

        /* Doughty Warriors */
        public override object Title => 1075404;

        /* You did great work. Thanks to you, my puppies are training harder than ever.
         * I think they could actually take a mongbat now. I’m still worried they’ll 
         * slack off, though. What say you to slaying some Imps for me now? Show them 
         * how it’s really done? */
        public override object Description => 1075405;

        /* You’re pretty sad. When I was a young warrior, I wasn’t one to walk away from a challenge. */
        public override object Refuse => 1075407;

        /* You won’t find many imps on this continent. Mostly, they’re native to Ilshenar. Might be a
         * few up around the hedge maze east of Skara Brae, though. */
        public override object Uncomplete => 1075408;

        /* Hah! I knew you were up to the challenge. */
        public override object Complete => 1075409;

        public DoughtyWarriors2Quest()
        {
            AddObjective(new SlayObjective(typeof(Imp), "Imps", 10));

            AddReward(new BaseReward(1075406)); // Show them what a real warrior is REALLY made of!
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

    public class DoughtyWarriors3Quest : BaseQuest
    {
        public override QuestChain ChainID => QuestChain.DoughtyWarriors;
        public override bool DoneOnce => true;

        /* Doughty Warriors */
        public override object Title => 1075410;

        /* You’ve really inspired my trainees. They’re working harder than ever. I’ve got 
         * one more request for you, and it’s a big one. Those mongbats didn’t pose a problem.
         * You mowed down the imps with no trouble. But do you dare take on a couple of daemons? */
        public override object Description => 1075411;

        /* What? You’re not scared of a couple of lousy daemons, are you? */
        public override object Refuse => 1075413;

        /* There’s all kinds of daemons you can kill. There are some near the hedge maze,
         * and some really powerful ones in Dungeon Doom. */
        public override object Uncomplete => 1075414;

        /* Thanks for helping me out. You’re a real hero to my guards. Valor is its own reward,
         * but maybe you wouldn’t mind wearing this sash. We don’t give these out to just 
         * anyone, you know! */
        public override object Complete => 1075415;

        public DoughtyWarriors3Quest()
        {
            AddObjective(new SlayObjective(typeof(Daemon), "Daemons", 10));

            AddReward(new BaseReward(typeof(SashOfMight), 1075412)); // Sash of Might
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

    public class Kane : MondainQuester
    {
        public override Type[] Quests => new[] { typeof( DoughtyWarriorsQuest ) };

        [Constructable]
        public Kane()
            : base("Kane", "the Master of Arms")
        {
            Name = "Kane";
            Title = "the Master of Arms";
        }

        public Kane(Serial serial)
            : base(serial)
        {
        }

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = false;
            Race = Race.Human;

            HairItemID = 0x203C;
            HairHue = 0x3B3;
        }

        public override void InitOutfit()
        {
            AddItem(new PlateArms());
            AddItem(new PlateChest());
            AddItem(new PlateGloves());
            AddItem(new StuddedGorget());
            AddItem(new PlateLegs());

            switch (Utility.Random(4))
            {
                case 0:
                {
                    AddItem(new PlateHelm()); break;
                }
                case 1:
                {
                    AddItem(new NorseHelm()); break;
                }
                case 2:
                {
                    AddItem(new CloseHelm()); break;
                }
                case 3:
                {
                    AddItem(new Helmet()); break;
                }
            }

            switch (Utility.Random(3))
            {
                case 0:
                {
                    AddItem(new BodySash(0x482)); break;
                }
                case 1:
                {
                    AddItem(new Doublet(0x482)); break;
                }
                case 2:
                {
                    AddItem(new Tunic(0x482)); break;
                }
            }

            AddItem(new Broadsword());

            Item shield = new MetalKiteShield
            {
                Hue = Utility.RandomNondyedHue()
            };

            AddItem(shield);

            switch (Utility.Random(2))
            {
                case 0:
                {
                    AddItem(new Boots()); break;
                }
                case 1:
                {
                    AddItem(new ThighBoots()); break;
                }
            }

            Blessed = true;
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
