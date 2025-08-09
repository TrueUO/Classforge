using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.CleanUpBritannia
{
    public static class CleanUpBritanniaRewards
    {
        public static List<CollectionItem> Rewards { get; set; }

        public static void Initialize()
        {
            Rewards = new List<CollectionItem>();

            Rewards.Add(new CollectionItem(typeof(Mailbox), 0x4142, 1113927, 0, 1000));
            Rewards.Add(new CollectionItem(typeof(HumansAndElvesRobe), 0x1F03, 1151202, 0, 1000));
            Rewards.Add(new CollectionItem(typeof(GargoylesAreOurFriendsRobe), 0x1F03, 1151203, 0, 1000));
            Rewards.Add(new CollectionItem(typeof(WeArePiratesRobe), 0x1F03, 1151204, 0, 1000));
            Rewards.Add(new CollectionItem(typeof(FollowerOfBaneRobe), 0x1F03, 1151205, 0, 1000));
            Rewards.Add(new CollectionItem(typeof(QueenDawnForeverRobe), 0x1F03, 1151206, 0, 1000));

            Rewards.Add(new CollectionItem(typeof(LillyPad), 0xDBC, 1023516, 0, 5000));
            Rewards.Add(new CollectionItem(typeof(LillyPads), 0xDBE, 1023518, 0, 5000));
            Rewards.Add(new CollectionItem(typeof(Mushrooms1), 0x0D0F, 1023340, 0, 5000));
            Rewards.Add(new CollectionItem(typeof(Mushrooms2), 0x0D12, 1023340, 0, 5000));
            Rewards.Add(new CollectionItem(typeof(Mushrooms3), 0x0D10, 1023340, 0, 5000));
            Rewards.Add(new CollectionItem(typeof(Mushrooms4), 0x0D13, 1023340, 0, 5000));
            Rewards.Add(new CollectionItem(typeof(NocturneEarrings), 0x1F07, 1151243, 0x3E5, 5000));

            Rewards.Add(new CollectionItem(typeof(SherryTheMouseStatue), 0x20D0, 1080171, 0, 10000));
            Rewards.Add(new CollectionItem(typeof(ChaosTileDeed), 0x14EF, 1080490, 0, 10000));
            Rewards.Add(new CollectionItem(typeof(HonestyVirtueTileDeed), 0x14EF, 1080488, 0, 10000));
            Rewards.Add(new CollectionItem(typeof(CompassionVirtueTileDeed), 0x14EF, 1080481, 0, 10000));
            Rewards.Add(new CollectionItem(typeof(ValorVirtueTileDeed), 0x14EF, 1080486, 0, 10000));
            Rewards.Add(new CollectionItem(typeof(SpiritualityVirtueTileDeed), 0x14EF, 1080484, 0, 10000));
            Rewards.Add(new CollectionItem(typeof(HonorVirtueTileDeed), 0x14EF, 1080485, 0, 10000));
            Rewards.Add(new CollectionItem(typeof(HumilityVirtueTileDeed), 0x14EF, 1080483, 0, 10000));
            Rewards.Add(new CollectionItem(typeof(SacrificeVirtueTileDeed), 0x14EF, 1080482, 0, 10000));
            Rewards.Add(new CollectionItem(typeof(JusticeVirtueTileDeed), 0x14EF, 1080487, 0, 10000));
            Rewards.Add(new CollectionItem(typeof(StewardDeed), 0x14F0, 1153344, 0, 10000));

            Rewards.Add(new CollectionItem(typeof(KnightsBascinet), 0x140C, 1151247, 1150, 10000));
            Rewards.Add(new CollectionItem(typeof(KnightsCloseHelm), 0x1408, 1151244, 1150, 10000));
            Rewards.Add(new CollectionItem(typeof(KnightsFemalePlateChest), 0x1C04, 1151253, 1150, 10000));
            Rewards.Add(new CollectionItem(typeof(KnightsNorseHelm), 0x140E, 1151245, 1150, 10000));
            Rewards.Add(new CollectionItem(typeof(KnightsPlateArms), 0x1410, 1151250, 1150, 10000));
            Rewards.Add(new CollectionItem(typeof(KnightsPlateChest), 0x1415, 1151252, 1150, 10000));
            Rewards.Add(new CollectionItem(typeof(KnightsPlateGloves), 0x1414, 1151249, 1150, 10000));
            Rewards.Add(new CollectionItem(typeof(KnightsPlateGorget), 0x1413, 1151248, 1150, 10000));
            Rewards.Add(new CollectionItem(typeof(KnightsPlateHelm), 0x1412, 1151246, 1150, 10000));
            Rewards.Add(new CollectionItem(typeof(KnightsPlateLegs), 0x1411, 1151251, 1150, 10000));

            Rewards.Add(new CollectionItem(typeof(ScoutArms), 0x13DC, 1151257, 1148, 10000));
            Rewards.Add(new CollectionItem(typeof(ScoutBustier), 0x1C0C, 1151262, 1148, 10000));
            Rewards.Add(new CollectionItem(typeof(ScoutChest), 0x13DB, 1151258, 1148, 10000));
            Rewards.Add(new CollectionItem(typeof(ScoutCirclet), 0x2B6E, 1151254, 1148, 10000));
            Rewards.Add(new CollectionItem(typeof(ScoutFemaleChest), 0x1C02, 1151261, 1148, 10000));
            Rewards.Add(new CollectionItem(typeof(ScoutGloves), 0x13D5, 1151259, 1148, 10000));
            Rewards.Add(new CollectionItem(typeof(ScoutGorget), 0x13D6, 1151256, 1148, 10000));
            Rewards.Add(new CollectionItem(typeof(ScoutLegs), 0x13DA, 1151260, 1148, 10000));
            Rewards.Add(new CollectionItem(typeof(ScoutSmallPlateJingasa), 0x2784, 1151255, 1148, 10000));

            Rewards.Add(new CollectionItem(typeof(SorcererArms), 0x13CD, 1151265, 1165, 10000));
            Rewards.Add(new CollectionItem(typeof(SorcererChest), 0x13CC, 1151266, 1165, 10000));
            Rewards.Add(new CollectionItem(typeof(SorcererFemaleChest), 0x1C06, 1151267, 1165, 10000));
            Rewards.Add(new CollectionItem(typeof(SorcererGloves), 0x13C6, 1151268, 1165, 10000));
            Rewards.Add(new CollectionItem(typeof(SorcererGorget), 0x13C7, 1151264, 1165, 10000));
            Rewards.Add(new CollectionItem(typeof(SorcererHat), 0x1718, 1151263, 1165, 10000));
            Rewards.Add(new CollectionItem(typeof(SorcererLegs), 0x13CB, 1151270, 1165, 10000));
            Rewards.Add(new CollectionItem(typeof(SorcererSkirt), 0x1C08, 1151269, 1165, 10000));

            Rewards.Add(new CollectionItem(typeof(YuccaTree), 0x0D37, 1023383, 0, 15000));
            Rewards.Add(new CollectionItem(typeof(TableLamp), 0x49C1, 1151220, 0, 15000));
            Rewards.Add(new CollectionItem(typeof(Bamboo), 0x246D, 1029324, 0, 15000));

            Rewards.Add(new CollectionItem(typeof(HorseBardingDeed), 0x14EF, 1080212, 0, 20000));
            
            Rewards.Add(new CollectionItem(typeof(SnakeSkinBoots), 0x170B, 1151224, 0x7D9, 20000));
            Rewards.Add(new CollectionItem(typeof(BootsOfTheLavaLizard), 0x170B, 1151223, 0x674, 20000));
            Rewards.Add(new CollectionItem(typeof(BootsOfTheIceWyrm), 0x170B, 1151225, 0x482, 20000));
            Rewards.Add(new CollectionItem(typeof(BootsOfTheCrystalHydra), 0x170B, 1151226, 0x47E, 20000));
            Rewards.Add(new CollectionItem(typeof(BootsOfTheThrasher), 0x170B, 1151227, 0x497, 20000));

            Rewards.Add(new CollectionItem(typeof(NaturesTears), 0x0E9C, 1154374, 2075, 20000));
            Rewards.Add(new CollectionItem(typeof(PrimordialDecay), 0x0E9C, 1154737, 1927, 20000));
            Rewards.Add(new CollectionItem(typeof(ArachnidDoom), 0x0E9C, 1154738, 1944, 20000));

            Rewards.Add(new CollectionItem(typeof(SophisticatedElvenTapestry), 0x2D70, 1151222, 0, 50000));
            Rewards.Add(new CollectionItem(typeof(OrnateElvenTapestry), 0x2D72, 1031633, 0, 50000));
            Rewards.Add(new CollectionItem(typeof(ChestOfDrawers), 0x0A2C, 1022604, 0, 50000));
            Rewards.Add(new CollectionItem(typeof(FootedChestOfDrawers), 0x0A30, 1151221, 0, 50000));

            Rewards.Add(new CollectionItem(typeof(DragonHeadAddonDeed), 0x2234, 1028756, 0, 50000));
            Rewards.Add(new CollectionItem(typeof(NestWithEggs), 0x1AD4, 1026868, 2415, 50000));

            Rewards.Add(new CollectionItem(typeof(FishermansHat), 0x1716, 1151238, 2578, 50000));
            Rewards.Add(new CollectionItem(typeof(FishermansTrousers), 0x13DA, 1151239, 2578, 50000));
            Rewards.Add(new CollectionItem(typeof(FishermansVest), 0x13CC, 1151240, 2578, 50000));
            Rewards.Add(new CollectionItem(typeof(FishermansEelskinGloves), 0x13C6, 1151237, 2578, 50000));
            
            Rewards.Add(new CollectionItem(typeof(BestialGloves), 0x13C6, 1151230, 2010, 50000));
            Rewards.Add(new CollectionItem(typeof(BestialGorget), 0x13D6, 1151232, 2010, 50000));
            Rewards.Add(new CollectionItem(typeof(BestialHelm), 0x1545, 1151229, 2010, 50000));
            Rewards.Add(new CollectionItem(typeof(BestialLegs), 0x13CB, 1151231, 2010, 50000));
            
            Rewards.Add(new CollectionItem(typeof(VirtuososCap), 0x171C, 1151324, 1374, 50000));
            Rewards.Add(new CollectionItem(typeof(VirtuososCollar), 0x13C7, 1151323, 1374, 50000));
            Rewards.Add(new CollectionItem(typeof(VirtuososKidGloves), 0x13C6, 1151560, 1374, 50000));
            Rewards.Add(new CollectionItem(typeof(VirtuososTunic), 0x13CC, 1151325, 1374, 50000));

            Rewards.Add(new CollectionItem(typeof(FirePitDeed), 0x29FD, 1080206, 0, 75000));
            Rewards.Add(new CollectionItem(typeof(PresentationStone), 0x32F2, 1154745, 0, 75000));
            Rewards.Add(new CollectionItem(typeof(Beehive), 0x091A, 1080263, 0, 80000));
            Rewards.Add(new CollectionItem(typeof(ArcheryButteDeed), 0x100B, 1024106, 0, 80000));

            Rewards.Add(new CollectionItem(typeof(VollemHeldInCrystal), 0x1f19, 1113629, 1154, 500000));
        }
    }
}
