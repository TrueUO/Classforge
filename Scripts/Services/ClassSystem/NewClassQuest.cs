using System;
using Server.Items;
using Server.Mobiles;
using Server.Services.ClassSystem;

namespace Server.Engines.Quests
{
    public class NewClassQuest1 : BaseQuest
    {
        public override QuestChain ChainID => QuestChain.NewClassQuest;
        public override Type NextQuest => typeof(NewClassQuest2);
        public override bool DoneOnce => true;

        public override object Title => "Kill sheep";

        public override object Description => "";

        public override object Refuse => "";

        public override object Uncomplete => "";

        public override object Complete => "";

        public NewClassQuest1()
        {
            AddObjective(new SlayObjective(typeof(Sheep), "Sheep", 5));

            AddReward(new BaseReward("+25 XP"));
            AddReward(new BaseReward("A reward based on your Class"));
        }

        public override void GiveRewards()
        {
            base.GiveRewards();

            ClassSystemHelper.AwardXp(Owner, 25, "a quest");

            if (Owner.CharacterClass == CharacterClass.Warrior)
            {
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

    public class NewClassQuest2 : BaseQuest
    {
        public override QuestChain ChainID => QuestChain.NewClassQuest;
        public override Type NextQuest => typeof(NewClassQuest3);
        public override bool DoneOnce => true;

        public override object Title => "Kill mongbats";

        public override object Description => "";

        public override object Refuse => "";

        public override object Uncomplete => "";

        public override object Complete => "";

        public NewClassQuest2()
        {
            AddObjective(new SlayObjective(typeof(Mongbat), "Mongbats", 10));

            AddReward(new BaseReward("+25 XP"));
            AddReward(new BaseReward("A reward based on your Class"));
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

    public class NewClassQuest3 : BaseQuest
    {
        public override QuestChain ChainID => QuestChain.NewClassQuest;
        public override bool DoneOnce => true;

        public override object Title => "Kill skeletons";

        public override object Description => "";

        public override object Refuse => "";

        public override object Uncomplete => "";

        public override object Complete => "";

        public NewClassQuest3()
        {
            AddObjective(new SlayObjective(typeof(Skeleton), "Skeletons", 10));

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
        public override Type[] Quests => [typeof(NewClassQuest1)];

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
