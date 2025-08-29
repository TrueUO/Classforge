using System;

namespace Server.Engines.Quests
{
    public enum QuestChain
    {
        None = 0,

        NewClassQuest = 1,
        UnfadingMemories = 2
    }

    public class BaseChain(Type currentQuest, Type quester)
    {
        public Type CurrentQuest { get; set; } = currentQuest;
        public Type Quester { get; set; } = quester;

        public static Type[][] Chains { get; }

        static BaseChain()
        {
            Chains = new Type[3][];

            Chains[(int)QuestChain.None] = [];

            Chains[(int)QuestChain.NewClassQuest] = [typeof(NewClassQuest1), typeof(NewClassQuest2), typeof(NewClassQuest3)];
            Chains[(int)QuestChain.UnfadingMemories] = [typeof(UnfadingMemoriesOneQuest), typeof(UnfadingMemoriesTwoQuest), typeof(UnfadingMemoriesThreeQuest)];
        }
    }
}
