using System;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Services.ClassSystem
{
    public enum CharacterClass
    {
        None,
        Cleric,  // 5% IMPLEMENTED
        Mage,    // 10% IMPLEMENTED
        Ranger,  // 5% IMPLEMENTED
        Rogue,   // 10% IMPLEMENTED
        Warrior  // 10% IMPLEMENTED
    }

    public static class ClassSystemHelper
    {
        public static void Initialize()
        {
            // In case we need to start/stop timers/event-sinks/and so on, on server startup/shutdown.
            // Currently unused.
        }

        public const int MaxLevel = 100;
        public const double BaseXp = 50.0;
        public const double GrowthFactor = 1.15;

        // Players start at level 1 and end at level 100?

        // Players start with 80 stats. 60 from character creation and 20 more based on Class selection.
        // Stat cap = 260 with scrolls and buffs
        // if you award 2 stat points every level you’ll hit the 260-point cap upon reaching Level 91.
        public static bool CanGainStatPoints(PlayerMobile pm)
        {
            return pm.Level <= 91;
        }

        // Current skill cap is 12,000. (this could change and be added to)
        // start with 0 skill at level 1. Each level gain skill points to level 100. (99 gain events)
        // first 22 levels award 122 points.
        // Rest of the levels award 121 points = 12,000 points at current max level 100
        public static int SkillPointsToAwardPerLevel(PlayerMobile pm)
        {
            return pm.Level <= 22 ? 122 : 121;
        }

        // Classic exponential XP curve per level
        public static int XpNeededForNextLevel(int n)
        {
            if (n < 1)
            {
                return 0;
            }

            return (int)(BaseXp * Math.Pow(GrowthFactor, n - 1));
        }

        public static class LevelUpHelper
        {
            public static void AwardXp(PlayerMobile pm, int xpGain, string source = "")
            {
                if (pm.Level >= MaxLevel)
                {
                    pm.SendMessage("You have reached the maximum level!");
                    return;
                }

                pm.Xp += xpGain;
                pm.SendMessage($"+{xpGain} XP{(string.IsNullOrEmpty(source) ? "" : $" from {source}")}.");

                CheckLevelUp(pm);

                if (pm.Level < MaxLevel)
                {
                    pm.SendMessage($"Progress: {pm.Xp}/{XpNeededForNextLevel(pm.Level)} XP toward Level {pm.Level + 1}.");
                }
                else
                {
                    pm.SendMessage("You have reached the maximum level!");
                }
            }

            public static void CheckLevelUp(PlayerMobile pm)
            {
                while (pm.Level < MaxLevel)
                {
                    // cost to go from current Level to next
                    if (pm.Xp < XpNeededForNextLevel(pm.Level))
                    {
                        break;
                    }

                    pm.Xp -= XpNeededForNextLevel(pm.Level); // reset player xp to 0 for next level
                    pm.Level++;
                    
                    pm.PrivateOverheadMessage(MessageType.Regular, 52, false, $"Congratulations! You reached Level {pm.Level}.", pm.NetState);

                    Effects.SendLocationParticles(EffectItem.Create(pm.Location, pm.Map, EffectItem.DefaultDuration), 0, 0, 0, 0, 0, 5060, 0);
                    Effects.PlaySound(pm.Location, pm.Map, 0x243);
                    Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(pm.X - 6, pm.Y - 6, pm.Z + 15), pm.Map), pm, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
                    Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(pm.X - 4, pm.Y - 6, pm.Z + 15), pm.Map), pm, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
                    Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(pm.X - 6, pm.Y - 4, pm.Z + 15), pm.Map), pm, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
                    Effects.SendTargetParticles(pm, 0x375A, 35, 90, 0x00, 0x00, 9502, (EffectLayer)255, 0x100);

                    pm.SkillPointsToSpend += SkillPointsToAwardPerLevel(pm);

                    if (CanGainStatPoints(pm))
                    {
                        pm.StatPointsToSpend += 2;
                    }

                    if (pm.SkillPointsToSpend > 0 && pm.StatPointsToSpend > 0)
                    {
                        pm.SendMessage($"You currently have {pm.SkillPointsToSpend} skill points and {pm.StatPointsToSpend} stat points to spend.");
                    }
                    else if (pm.SkillPointsToSpend > 0 && pm.StatPointsToSpend == 0)
                    {
                        pm.SendMessage($"You currently have {pm.SkillPointsToSpend} skill points to spend.");
                    }
                    else if (pm.SkillPointsToSpend == 0 && pm.StatPointsToSpend > 0)
                    {
                        pm.SendMessage($"You currently have {pm.StatPointsToSpend} stat points to spend.");
                    }
                }
            }
        }

        // Called from BaseCreature OnKilledBy
        public static void AwardXpForKill(BaseCreature killed, Mobile killer)
        {
            if (killer is not PlayerMobile pm || pm.CharacterClass == CharacterClass.None)
            {
                return;
            }

            if (killed.XpToGive <= 0)
            {
                return;
            }

            /*// New Taper System: XP slowly drops from 100% to 25% from level 1 → 100
            double taperPercent = 1.0 - 0.75 * (pm.Level - 1) / (MaxLevel - 1); // 1.0 → 0.25
            taperPercent = Math.Max(0.25, taperPercent); // Clamp minimum at 25%*/
           
            LevelUpHelper.AwardXp(pm, killed.XpToGive, $"{killed.Name}");
        }

        public static string GetClassName(CharacterClass characterClass)
        {
            switch (characterClass)
            {
                case CharacterClass.Cleric:
                {
                    return ClericClass.Name;
                }
                case CharacterClass.Mage:
                {
                    return MageClass.Name;
                }
                case CharacterClass.Ranger:
                {
                    return RangerClass.Name;
                }
                case CharacterClass.Rogue:
                {
                    return RogueClass.Name;
                }
                case CharacterClass.Warrior:
                {
                    return WarriorClass.Name;
                }
                default:
                {
                    return "None";
                }
            }
        }

        public static SkillName[] GetPrimarySkills(CharacterClass characterClass)
        {
            switch (characterClass)
            {
                case CharacterClass.Cleric:
                {
                    return ClericClass.PrimarySkills;
                }
                case CharacterClass.Mage:
                {
                    return MageClass.PrimarySkills;
                }
                case CharacterClass.Ranger:
                {
                    return RangerClass.PrimarySkills;
                }
                case CharacterClass.Rogue:
                {
                    return RogueClass.PrimarySkills;
                }
                case CharacterClass.Warrior:
                {
                    return WarriorClass.PrimarySkills;
                }
                default:
                {
                    return [];
                }
            }
        }

        public static SkillName[] GetCraftSkills(CharacterClass characterClass)
        {
            switch (characterClass)
            {
                case CharacterClass.Cleric:
                {
                    return ClericClass.CraftSkills;
                }
                case CharacterClass.Mage:
                {
                    return MageClass.CraftSkills;
                }
                case CharacterClass.Ranger:
                {
                    return RangerClass.CraftSkills;
                }
                case CharacterClass.Rogue:
                {
                    return RogueClass.CraftSkills;
                }
                case CharacterClass.Warrior:
                {
                    return WarriorClass.CraftSkills;
                }
                default:
                {
                    return [];
                }
            }
        }
    }
}
