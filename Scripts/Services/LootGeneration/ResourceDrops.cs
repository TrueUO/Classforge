using System;
using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Items
{
    public class IngredientDropEntry
    {
        private readonly Type m_CreatureType;
        private readonly bool m_DropMultiples;
        private readonly string m_Region;
        private readonly double m_Chance;
        private readonly Type[] m_Ingredients;

        public Type CreatureType => m_CreatureType;
        public bool DropMultiples => m_DropMultiples;
        public string Region => m_Region;
        public double Chance => m_Chance;
        public Type[] Ingredients => m_Ingredients;

        public IngredientDropEntry(Type creature, bool dropMultiples, double chance, params Type[] ingredients)
            : this(creature, dropMultiples, null, chance, ingredients)
        {
        }

        public IngredientDropEntry(Type creature, bool dropMultiples, string region, double chance, params Type[] ingredients)
        {
            m_CreatureType = creature;
            m_Ingredients = ingredients;
            m_DropMultiples = dropMultiples;
            m_Region = region;
            m_Chance = chance;
        }

        private static List<IngredientDropEntry> m_IngredientTable;
        public static List<IngredientDropEntry> IngredientTable => m_IngredientTable;

        public static void Initialize()
        {
            m_IngredientTable = new List<IngredientDropEntry>();

            // Imbuing Gems
            m_IngredientTable.Add(new IngredientDropEntry(typeof(AncientLichRenowned), true, 0.50, ImbuingGems));
            m_IngredientTable.Add(new IngredientDropEntry(typeof(DevourerRenowned), true, 0.50, ImbuingGems));
            m_IngredientTable.Add(new IngredientDropEntry(typeof(FireElementalRenowned), true, 0.50, ImbuingGems));
            m_IngredientTable.Add(new IngredientDropEntry(typeof(GrayGoblinMageRenowned), true, 0.50, ImbuingGems));
            m_IngredientTable.Add(new IngredientDropEntry(typeof(GreenGoblinAlchemistRenowned), true, 0.5, ImbuingGems));
            m_IngredientTable.Add(new IngredientDropEntry(typeof(PixieRenowned), true, 0.50, ImbuingGems));
            m_IngredientTable.Add(new IngredientDropEntry(typeof(SkeletalDragonRenowned), true, 0.50, ImbuingGems));

            //Crystaline Blackrock
            m_IngredientTable.Add(new IngredientDropEntry(typeof(AgapiteElemental), true, 0.25, typeof(CrystallineBlackrock)));
            m_IngredientTable.Add(new IngredientDropEntry(typeof(BronzeElemental), true, 0.25, typeof(CrystallineBlackrock)));
            m_IngredientTable.Add(new IngredientDropEntry(typeof(CopperElemental), true, 0.25, typeof(CrystallineBlackrock)));
            m_IngredientTable.Add(new IngredientDropEntry(typeof(GoldenElemental), true, 0.25, typeof(CrystallineBlackrock)));
            m_IngredientTable.Add(new IngredientDropEntry(typeof(ShadowIronElemental), true, 0.25, typeof(CrystallineBlackrock)));
            m_IngredientTable.Add(new IngredientDropEntry(typeof(ValoriteElemental), true, 0.25, typeof(CrystallineBlackrock)));
            m_IngredientTable.Add(new IngredientDropEntry(typeof(VeriteElemental), true, 0.25, typeof(CrystallineBlackrock)));

            m_IngredientTable.Add(new IngredientDropEntry(typeof(BaseCreature), false, "Cavern of the Discarded", 0.10,
                typeof(CrystallineBlackrock), typeof(SeedOfRenewal)));
        }

        public static void OnCreatureDeath(Mobile creature, Mobile killer, Container corpse)
        {
            BaseCreature bc = creature as BaseCreature;
            Container c = corpse;

            if (bc != null && c != null && !c.Deleted && !bc.Controlled && !bc.Summoned)
            {
                CheckDrop(bc, c);
            }

            if (killer is BaseVoidCreature vc)
            {
                vc.Mutate(VoidEvolution.Killing);
            }
        }

        public static void CheckDrop(BaseCreature bc, Container c)
        {
            if (m_IngredientTable != null)
            {
                for (int index = 0; index < m_IngredientTable.Count; index++)
                {
                    IngredientDropEntry entry = m_IngredientTable[index];

                    if (entry == null)
                    {
                        continue;
                    }

                    if (entry.Region != null)
                    {
                        string reg = entry.Region;

                        if (reg == "TerMur" && c.Map != Map.TerMur)
                        {
                            continue;
                        }

                        if (reg == "Abyss" && (c.Map != Map.TerMur || c.X < 235 || c.X > 1155 || c.Y < 40 || c.Y > 1040))
                        {
                            continue;
                        }

                        if (reg != "TerMur" && reg != "Abyss")
                        {
                            Region r = Server.Region.Find(c.Location, c.Map);

                            if (r == null || !r.IsPartOf(entry.Region))
                            {
                                continue;
                            }
                        }
                    }

                    if (bc.GetType() != entry.CreatureType && !bc.GetType().IsSubclassOf(entry.CreatureType))
                    {
                        continue;
                    }

                    double toBeat = entry.Chance;
                    List<Item> drops = new List<Item>();

                    if (bc is BaseVoidCreature creature)
                    {
                        toBeat *= creature.Stage + 1;
                    }

                    if (entry.DropMultiples)
                    {
                        for (int i = 0; i < entry.Ingredients.Length; i++)
                        {
                            Type type = entry.Ingredients[i];

                            if (toBeat >= Utility.RandomDouble())
                            {
                                Item drop = Loot.Construct(type);

                                if (drop != null)
                                {
                                    drops.Add(drop);
                                }
                            }
                        }
                    }
                    else if (toBeat >= Utility.RandomDouble())
                    {
                        Item drop = Loot.Construct(entry.Ingredients);

                        if (drop != null)
                        {
                            drops.Add(drop);
                        }
                    }

                    for (int i = 0; i < drops.Count; i++)
                    {
                        Item item = drops[i];

                        c.DropItem(item);
                    }

                    ColUtility.Free(drops);
                }
            }
        }

        public static Type[] ImbuingGems =
        {
            typeof(FireRuby),
            typeof(WhitePearl),
            typeof(BlueDiamond),
            typeof(Turquoise)
        };
    }
}
