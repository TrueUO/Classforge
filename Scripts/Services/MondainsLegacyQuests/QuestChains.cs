using System;

namespace Server.Engines.Quests
{
    public enum QuestChain
    {
        None = 0,

        UnfadingMemories = 1,
        DoughtyWarriors = 2,
        FlintTheQuartermaster = 3
    }

    public class BaseChain(Type currentQuest, Type quester)
    {
        public Type CurrentQuest { get; set; } = currentQuest;
        public Type Quester { get; set; } = quester;

        public static Type[][] Chains { get; }

        static BaseChain()
        {
            Chains = new Type[4][];

            Chains[(int)QuestChain.None] = [];

            Chains[(int)QuestChain.UnfadingMemories] = [typeof(UnfadingMemoriesOneQuest), typeof(UnfadingMemoriesTwoQuest), typeof(UnfadingMemoriesThreeQuest)];
            Chains[(int)QuestChain.DoughtyWarriors] = [typeof(DoughtyWarriorsQuest), typeof(DoughtyWarriors2Quest), typeof(DoughtyWarriors3Quest)];
            Chains[(int)QuestChain.FlintTheQuartermaster] = [typeof(ThievesBeAfootQuest), typeof(BibliophileQuest)];
        }
    }
}
