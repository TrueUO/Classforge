using System;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Regions;
using Server.Spells.SkillMasteries;

namespace Server.Misc
{
    public class SkillCheck
    {
        private static readonly TimeSpan _StatGainDelay;
        private static readonly TimeSpan _PetStatGainDelay;

        private static readonly int _PlayerChanceToGainStats;
        private static readonly int _PetChanceToGainStats;

        static SkillCheck()
        {
            _PlayerChanceToGainStats = Config.Get("PlayerCaps.PlayerChanceToGainStats", 5);
            _PetChanceToGainStats = Config.Get("PlayerCaps.PetChanceToGainStats", 5);

            _StatGainDelay = TimeSpan.FromSeconds(0.5);
            _PetStatGainDelay = TimeSpan.FromSeconds(0.5);
        }

        public static void Initialize()
        {
            Mobile.SkillCheckLocationHandler = Mobile_SkillCheckLocation;
            Mobile.SkillCheckDirectLocationHandler = Mobile_SkillCheckDirectLocation;

            Mobile.SkillCheckTargetHandler = Mobile_SkillCheckTarget;
            Mobile.SkillCheckDirectTargetHandler = Mobile_SkillCheckDirectTarget;
        }

        public static bool Mobile_SkillCheckLocation(Mobile from, SkillName skillName, double minSkill, double maxSkill)
        {
            Skill skill = from.Skills[skillName];

            if (skill == null)
            {
                return false;
            }

            double value = skill.Value;

            //TODO: Is there any other place this can go?
            if (skillName == SkillName.Fishing && BaseGalleon.FindGalleonAt(from, from.Map) is TokunoGalleon)
            {
                value += 1;
            }

            double chance = (value - minSkill) / (maxSkill - minSkill);

            CrystalBallOfKnowledge.TellSkillDifficulty(from, skillName, chance);

            if (value < minSkill)
            {
                return false; // Too difficult
            }

            if (value >= maxSkill)
            {
                return true; // No challenge
            }

            return CheckSkill(from, skill, chance);
        }

        public static bool Mobile_SkillCheckDirectLocation(Mobile from, SkillName skillName, double chance)
        {
            Skill skill = from.Skills[skillName];

            if (skill == null)
            {
                return false;
            }

            CrystalBallOfKnowledge.TellSkillDifficulty(from, skillName, chance);

            if (chance < 0.0)
            {
                return false; // Too difficult
            }

            if (chance >= 1.0)
            {
                return true; // No challenge
            }

            return CheckSkill(from, skill, chance);
        }

        /// <summary>
        /// This should be a successful skill check, where a system can register several skill gains at once. Only system
        /// using this currently is UseAllRes for CraftItem.cs
        /// </summary>
        /// <param name="from"></param>
        /// <param name="maxSkill"></param>
        /// <param name="amount"></param>
        /// <param name="sk"></param>
        /// <param name="minSkill"></param>
        /// <returns></returns>
        public static bool CheckSkill(Mobile from, SkillName sk, double minSkill, double maxSkill, int amount)
        {
            if (from.Skills.Cap == 0)
            {
                return false;
            }

            Skill skill = from.Skills[sk];
            double value = skill.Value;
            int gains = 0;

            for (int i = 0; i < amount; i++)
            {
                double chance = (value - minSkill) / (maxSkill - minSkill);
                double gc = GetGainChance(from, skill, chance, Utility.Random(100) <= (int)(chance * 100)) / (value / 4);

                if (AllowGain(from, skill))
                {
                    if (from.Alive && (skill.Base + (value - skill.Value) < 10.0 || Utility.RandomDouble() <= gc))
                    {
                        gains++;
                        value += 0.1;
                    }
                }

            }

            if (gains > 0)
            {
                Gain(from, skill, gains);
                return true;
            }

            return false;
        }

        public static bool CheckSkill(Mobile from, Skill skill, double chance)
        {
            if (from.Skills.Cap == 0)
            {
                return false;
            }

            bool success = Utility.Random(100) <= (int)(chance * 100);
            double gc = GetGainChance(from, skill, chance, success);

            if (AllowGain(from, skill))
            {
                if (from.Alive && (skill.Base < 10.0 || Utility.RandomDouble() <= gc))
                {
                    Gain(from, skill);
                }
            }

            return success;
        }

        private static double GetGainChance(Mobile from, Skill skill, double chance, bool success)
        {
            double gc = (double)(from.Skills.Cap - from.Skills.Total) / from.Skills.Cap;

            gc += (skill.Cap - skill.Base) / skill.Cap;
            gc /= 2;

            gc += (1.0 - chance) * (success ? 0.5 : 0.0);
            gc /= 2;

            gc *= skill.Info.GainFactor;

            if (gc < 0.01)
            {
                gc = 0.01;
            }

            // Pets get a 100% bonus
            if (from is BaseCreature bc && bc.Controlled)
            {
                gc += gc * 1.00;
            }

            if (gc > 1.00)
            {
                gc = 1.00;
            }

            return gc;
        }

        public static bool Mobile_SkillCheckTarget(
            Mobile from,
            SkillName skillName,
            object target,
            double minSkill,
            double maxSkill)
        {
            Skill skill = from.Skills[skillName];

            if (skill == null)
            {
                return false;
            }

            double value = skill.Value;

            if (value < minSkill)
            {
                return false; // Too difficult
            }

            if (value >= maxSkill)
            {
                return true; // No challenge
            }

            double chance = (value - minSkill) / (maxSkill - minSkill);

            CrystalBallOfKnowledge.TellSkillDifficulty(from, skillName, chance);

            return CheckSkill(from, skill, chance);
        }

        public static bool Mobile_SkillCheckDirectTarget(Mobile from, SkillName skillName, object target, double chance)
        {
            Skill skill = from.Skills[skillName];

            if (skill == null)
            {
                return false;
            }

            CrystalBallOfKnowledge.TellSkillDifficulty(from, skillName, chance);

            if (chance < 0.0)
            {
                return false; // Too difficult
            }

            if (chance >= 1.0)
            {
                return true; // No challenge
            }

            return CheckSkill(from, skill, chance);
        }

        /*
         * For now shutting off player gains here.
         * Still allows gains for creatures and monsters.
         * Will refactor this whole file at a later date.
         */
        private static bool AllowGain(Mobile from, Skill skill)
        {
            /*if (Engines.VvV.ViceVsVirtueSystem.InSkillLoss(from))
            {
                return false;
            }*/

            if (from is PlayerMobile)
            {
                return false;
            }

            return true;
        }

        public enum Stat
        {
            Str,
            Dex,
            Int
        }

        public static void Gain(Mobile from, Skill skill)
        {
            Gain(from, skill, (int)(from.Region.SkillGain(from) * 10));
        }

        public static void Gain(Mobile from, Skill skill, int toGain)
        {
            if (from.Region.IsPartOf<Jail>())
            {
                return;
            }

            if (from is BaseCreature creature && creature.IsDeadPet)
            {
                return;
            }

            if (skill.SkillName == SkillName.Focus && from is BaseCreature baseCreature && !baseCreature.Controlled)
            {
                return;
            }

            if (skill.Base < skill.Cap && skill.Lock == SkillLock.Up)
            {
                Skills skills = from.Skills;

                if (toGain == 1 && skill.Base <= 10.0)
                {
                    toGain = Utility.Random(4) + 1;
                }

                else if (from is BaseCreature bc && (bc.Controlled || bc.Summoned))
                {
                    Mobile master = bc.GetMaster();

                    if (master != null && SkillMasterySpell.GetSpell(master, typeof(WhisperingSpell)) is WhisperingSpell spell &&
                        master.InRange(bc.Location, spell.PartyRange) && master.Map == bc.Map && spell.EnhancedGainChance >= Utility.Random(100))
                    {
                        toGain = Utility.RandomMinMax(2, 5);
                    }
                }

                if (from is PlayerMobile)
                {
                    CheckReduceSkill(skills, toGain, skill);
                }

                if (!from.Player || (skills.Total + toGain <= skills.Cap))
                {
                    skill.BaseFixedPoint = Math.Min(skill.CapFixedPoint, skill.BaseFixedPoint + toGain);
                }
            }

            if (skill.Lock == SkillLock.Up)
            {
                TryStatGain(skill.Info, from);
            }
        }

        private static void CheckReduceSkill(Skills skills, int toGain, Skill gainSKill)
        {
            if (skills.Total / skills.Cap >= Utility.RandomDouble())
            {
                for (int index = 0; index < skills.Length; index++)
                {
                    Skill toLower = skills[index];

                    if (toLower != gainSKill && toLower.Lock == SkillLock.Down && toLower.BaseFixedPoint >= toGain)
                    {
                        toLower.BaseFixedPoint -= toGain;
                        break;
                    }
                }
            }
        }

        public static void TryStatGain(SkillInfo info, Mobile from)
        {
            // Chance roll
            double chance;

            if (from is BaseCreature creature && creature.Controlled)
            {
                chance = _PetChanceToGainStats / 100.0;
            }
            else
            {
                chance = _PlayerChanceToGainStats / 100.0;
            }

            if (Utility.RandomDouble() >= chance)
            {
                return;
            }

            // Selection
            StatLockType primaryLock = StatLockType.Locked;
            StatLockType secondaryLock = StatLockType.Locked;

            switch (info.Primary)
            {
                case StatCode.Str:
                {
                    primaryLock = from.StrLock;
                    break;
                }
                case StatCode.Dex:
                {
                    primaryLock = from.DexLock;
                    break;
                }
                case StatCode.Int:
                {
                    primaryLock = from.IntLock;
                    break;
                }
            }

            switch (info.Secondary)
            {
                case StatCode.Str:
                {
                    secondaryLock = from.StrLock;
                    break;
                }
                case StatCode.Dex:
                {
                    secondaryLock = from.DexLock;
                    break;
                }
                case StatCode.Int:
                {
                    secondaryLock = from.IntLock;
                    break;
                }
            }

            // Gain
            // Decision block of both are selected to gain
            if (primaryLock == StatLockType.Up && secondaryLock == StatLockType.Up)
            {
                if (Utility.Random(4) == 0)
                {
                    GainStat(from, (Stat)info.Secondary);
                }
                else
                {
                    GainStat(from, (Stat)info.Primary);
                }
            }
            else // Will not do anything if neither are selected to gain
            {
                if (primaryLock == StatLockType.Up)
                {
                    GainStat(from, (Stat)info.Primary);
                }
                else if (secondaryLock == StatLockType.Up)
                {
                    GainStat(from, (Stat)info.Secondary);
                }
            }
        }

        public static bool CanLower(Mobile from, Stat stat)
        {
            switch (stat)
            {
                case Stat.Str:
                {
                    return from.StrLock == StatLockType.Down && from.RawStr > 10;
                }

                case Stat.Dex:
                {
                    return from.DexLock == StatLockType.Down && from.RawDex > 10;
                }

                case Stat.Int:
                {
                    return from.IntLock == StatLockType.Down && from.RawInt > 10;
                }
            }

            return false;
        }

        public static bool CanRaise(Mobile from, Stat stat, bool atTotalCap)
        {
            switch (stat)
            {
                case Stat.Str:
                {
                    if (from.RawStr < from.StrCap)
                    {
                        if (atTotalCap && from is PlayerMobile)
                        {
                            return CanLower(from, Stat.Dex) || CanLower(from, Stat.Int);
                        }

                        return true;
                    }

                    return false;
                }

                case Stat.Dex:
                {
                    if (from.RawDex < from.DexCap)
                    {
                        if (atTotalCap && from is PlayerMobile)
                        {
                            return CanLower(from, Stat.Str) || CanLower(from, Stat.Int);
                        }

                        return true;
                    }

                    return false;
                }

                case Stat.Int:
                {
                    if (from.RawInt < from.IntCap)
                    {
                        if (atTotalCap && from is PlayerMobile)
                        {
                            return CanLower(from, Stat.Str) || CanLower(from, Stat.Dex);
                        }

                        return true;
                    }

                    return false;
                }
            }

            return false;
        }

        public static void IncreaseStat(Mobile from, Stat stat)
        {
            bool atTotalCap = from.RawStatTotal >= from.StatCap;

            switch (stat)
            {
                case Stat.Str:
                    {
                        if (CanRaise(from, Stat.Str, atTotalCap))
                        {
                            if (atTotalCap)
                            {
                                if (CanLower(from, Stat.Dex) && (from.RawDex < from.RawInt || !CanLower(from, Stat.Int)))
                                {
                                    --from.RawDex;
                                }
                                else if (CanLower(from, Stat.Int))
                                {
                                    --from.RawInt;
                                }
                            }

                            ++from.RawStr;

                            if (from is BaseCreature creature && creature.HitsMaxSeed > -1 && creature.HitsMaxSeed < creature.StrCap)
                            {
                                creature.HitsMaxSeed++;
                            }
                        }

                        break;
                    }
                case Stat.Dex:
                    {
                        if (CanRaise(from, Stat.Dex, atTotalCap))
                        {
                            if (atTotalCap)
                            {
                                if (CanLower(from, Stat.Str) && (from.RawStr < from.RawInt || !CanLower(from, Stat.Int)))
                                {
                                    --from.RawStr;
                                }
                                else if (CanLower(from, Stat.Int))
                                {
                                    --from.RawInt;
                                }
                            }

                            ++from.RawDex;

                            if (from is BaseCreature creature && creature.StamMaxSeed > -1 && creature.StamMaxSeed < creature.DexCap)
                            {
                                creature.StamMaxSeed++;
                            }
                        }

                        break;
                    }
                case Stat.Int:
                    {
                        if (CanRaise(from, Stat.Int, atTotalCap))
                        {
                            if (atTotalCap)
                            {
                                if (CanLower(from, Stat.Str) && (from.RawStr < from.RawDex || !CanLower(from, Stat.Dex)))
                                {
                                    --from.RawStr;
                                }
                                else if (CanLower(from, Stat.Dex))
                                {
                                    --from.RawDex;
                                }
                            }

                            ++from.RawInt;

                            if (from is BaseCreature creature && creature.ManaMaxSeed > -1 && creature.ManaMaxSeed < creature.IntCap)
                            {
                                creature.ManaMaxSeed++;
                            }
                        }

                        break;
                    }
            }
        }

        public static void GainStat(Mobile from, Stat stat)
        {
            if (!CheckStatTimer(from, stat))
            {
                return;
            }

            IncreaseStat(from, stat);
        }

        public static bool CheckStatTimer(Mobile from, Stat stat)
        {
            switch (stat)
            {
                case Stat.Str:
                    {
                        if (from is BaseCreature creature && creature.Controlled)
                        {
                            if (creature.LastStrGain + _PetStatGainDelay >= DateTime.UtcNow)
                            {
                                return false;
                            }
                        }
                        else if (from.LastStrGain + _StatGainDelay >= DateTime.UtcNow)
                        {
                            return false;
                        }

                        from.LastStrGain = DateTime.UtcNow;
                        break;
                    }
                case Stat.Dex:
                    {
                        if (from is BaseCreature creature && creature.Controlled)
                        {
                            if (creature.LastDexGain + _PetStatGainDelay >= DateTime.UtcNow)
                            {
                                return false;
                            }
                        }
                        else if (from.LastDexGain + _StatGainDelay >= DateTime.UtcNow)
                        {
                            return false;
                        }

                        from.LastDexGain = DateTime.UtcNow;
                        break;
                    }
                case Stat.Int:
                    {
                        if (from is BaseCreature creature && creature.Controlled)
                        {
                            if (creature.LastIntGain + _PetStatGainDelay >= DateTime.UtcNow)
                            {
                                return false;
                            }
                        }
                        else if (from.LastIntGain + _StatGainDelay >= DateTime.UtcNow)
                        {
                            return false;
                        }

                        from.LastIntGain = DateTime.UtcNow;
                        break;
                    }
            }
            return true;
        }
    }
}
