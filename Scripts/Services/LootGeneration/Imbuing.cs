using System;
using Server.Items;

namespace Server.SkillHandlers
{
    public class Imbuing
    {
        public static void SetProperty(Item item, int id, int value)
        {
            object prop = ItemPropertyInfo.GetAttribute(id);

            if (item is BaseWeapon wep)
            {
                if (prop is AosAttribute attr)
                {
                    if (attr == AosAttribute.SpellChanneling)
                    {
                        wep.Attributes.SpellChanneling = value;

                        if (value > 0 && wep.Attributes.CastSpeed >= 0)
                        {
                            wep.Attributes.CastSpeed -= 1;
                        }
                    }
                    else if (attr == AosAttribute.CastSpeed && wep.Attributes[AosAttribute.CastSpeed]<1)
                    {
                        wep.Attributes.CastSpeed += value;
                    }
                    else
                    {
                        wep.Attributes[attr] = value;
                    }
                }
                else if (prop is AosWeaponAttribute attribute)
                {
                    wep.WeaponAttributes[attribute] = value;
                }

                else if (prop is SlayerName name)
                {
                    wep.Slayer = name;
                }
                else if (prop is SAAbsorptionAttribute absorptionAttribute)
                {
                    wep.AbsorptionAttributes[absorptionAttribute] = value;
                }
                else if (prop is AosElementAttribute elementAttribute)
                {
                    int fire, phys, cold, nrgy, pois, chaos, direct;

                    wep.GetDamageTypes(null, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);

                    value = Math.Min(phys, value);

                    wep.AosElementDamages[elementAttribute] = value;
                    wep.Hue = wep.GetElementalDamageHue();
                }
                else if (prop is string propString && wep is BaseRanged ranged && propString == "WeaponVelocity")
                {
                    ranged.Velocity = value;
                }
            }
            else if (item is BaseShield shield)
            {
                if (prop is AosAttribute aosAttribute)
                {
                    if (aosAttribute == AosAttribute.SpellChanneling)
                    {
                        shield.Attributes.SpellChanneling = value;

                        if (value > 0 && shield.Attributes.CastSpeed >= 0)
                        {
                            shield.Attributes.CastSpeed -= 1;
                        }
                    }
                    else if (aosAttribute == AosAttribute.CastSpeed && shield.Attributes[AosAttribute.CastSpeed] < 1)
                    {
                        shield.Attributes.CastSpeed += value;
                    }
                    else
                    {
                        shield.Attributes[aosAttribute] = value;
                    }
                }
                else if (prop is AosElementAttribute elementAttribute)
                {
                    switch (elementAttribute)
                    {
                        case AosElementAttribute.Physical:
                        {
                            shield.PhysicalBonus = value; break;
                        }
                        case AosElementAttribute.Fire:
                        {
                            shield.FireBonus = value; break;
                        }
                        case AosElementAttribute.Cold:
                        {
                            shield.ColdBonus = value; break;
                        }
                        case AosElementAttribute.Poison:
                        {
                            shield.PoisonBonus = value; break;
                        }
                        case AosElementAttribute.Energy:
                        {
                            shield.EnergyBonus = value; break;
                        }
                    }
                }
                else if (prop is SAAbsorptionAttribute absorptionAttribute)
                {
                    shield.AbsorptionAttributes[absorptionAttribute] = value;
                }
                else if (prop is AosArmorAttribute attribute)
                {
                    shield.ArmorAttributes[attribute] = value;
                }
            }
            else if (item is BaseArmor arm)
            {
                if (prop is AosAttribute attribute)
                {
                    arm.Attributes[attribute] = value;
                }
                else if (prop is AosElementAttribute attr)
                {
                    switch (attr)
                    {
                        case AosElementAttribute.Physical:
                        {
                            arm.PhysicalBonus = value; break;
                        }
                        case AosElementAttribute.Fire:
                        {
                            arm.FireBonus = value; break;
                        }
                        case AosElementAttribute.Cold:
                        {
                            arm.ColdBonus = value; break;
                        }
                        case AosElementAttribute.Poison:
                        {
                            arm.PoisonBonus = value; break;
                        }
                        case AosElementAttribute.Energy:
                        {
                            arm.EnergyBonus = value; break;
                        }
                    }
                }
                else if (prop is SAAbsorptionAttribute absorptionAttribute)
                {
                    arm.AbsorptionAttributes[absorptionAttribute] = value;
                }
                else if (prop is AosArmorAttribute armorAttribute)
                {
                    arm.ArmorAttributes[armorAttribute] = value;
                }
            }
            else if (item is BaseClothing clothing)
            {
                if (prop is AosAttribute attribute)
                {
                    clothing.Attributes[attribute] = value;
                }
                else if (prop is SAAbsorptionAttribute absorptionAttribute)
                {
                    clothing.SAAbsorptionAttributes[absorptionAttribute] = value;
                }
                else if (prop is AosElementAttribute attr)
                {
                    switch (attr)
                    {
                        case AosElementAttribute.Physical:
                        {
                            clothing.Resistances.Physical = value; break;
                        }
                        case AosElementAttribute.Fire:
                        {
                            clothing.Resistances.Fire = value; break;
                        }
                        case AosElementAttribute.Cold:
                        {
                            clothing.Resistances.Cold = value; break;
                        }
                        case AosElementAttribute.Poison:
                        {
                            clothing.Resistances.Poison = value; break;
                        }
                        case AosElementAttribute.Energy:
                        {
                            clothing.Resistances.Energy = value; break;
                        }
                    }
                }
            }
            else if (item is BaseJewel jewel)
            {
                if (prop is AosAttribute attribute)
                {
                    jewel.Attributes[attribute] = value;
                }
                else if (prop is SAAbsorptionAttribute absorptionAttribute)
                {
                    jewel.AbsorptionAttributes[absorptionAttribute] = value;
                }
                else if (prop is AosElementAttribute attr)
                {
                    switch (attr)
                    {
                        case AosElementAttribute.Physical:
                        {
                            jewel.Resistances.Physical = value; break;
                        }
                        case AosElementAttribute.Fire:
                        {
                            jewel.Resistances.Fire = value; break;
                        }
                        case AosElementAttribute.Cold:
                        {
                            jewel.Resistances.Cold = value; break;
                        }
                        case AosElementAttribute.Poison:
                        {
                            jewel.Resistances.Poison = value; break;
                        }
                        case AosElementAttribute.Energy:
                        {
                            jewel.Resistances.Energy = value; break;
                        }
                    }
                }
                else if (prop is SkillName skill)
                {
                    AosSkillBonuses bonuses = jewel.SkillBonuses;

                    int index = GetAvailableSkillIndex(bonuses);

                    if (index >= 0 && index <= 4)
                    {
                        bonuses.SetValues(index, skill, value);
                    }
                }
            }

            item.InvalidateProperties();
        }

        public static int GetMaxWeight(object item)
        {
            int maxWeight = 450;

            if (item is IQuality quality && quality.Quality == ItemQuality.Exceptional)
            {
                maxWeight += 50;
            }

            if (item is BaseWeapon itemToImbue)
            {
                if (itemToImbue is BaseRanged)
                {
                    maxWeight += 50;
                }
                else if (itemToImbue.Layer == Layer.TwoHanded)
                {
                    maxWeight += 100;
                }
            }
            else if (item is BaseJewel)
            {
                maxWeight = 500;
            }

            return maxWeight;
        }

        public static int GetTotalMods(Item item, int id = -1)
        {
            int total = 0;
            object prop = ItemPropertyInfo.GetAttribute(id);

            if (item is BaseWeapon wep)
            {
                foreach (int i in Enum.GetValues(typeof(AosAttribute)))
                {
                    AosAttribute attr = (AosAttribute)i;

                    if (!ItemPropertyInfo.ValidateProperty(attr))
                    {
                        continue;
                    }

                    if (wep.Attributes[attr] > 0)
                    {
                        if (!(prop is AosAttribute) || (AosAttribute)prop != attr)
                        {
                            total++;
                        }
                    }
                    else if (wep.Attributes[attr] == 0 && attr == AosAttribute.CastSpeed && wep.Attributes[AosAttribute.SpellChanneling] > 0)
                    {
                        if (!(prop is AosAttribute) || (AosAttribute)prop != attr)
                        {
                            total++;
                        }
                    }
                }

                total += GetSkillBonuses(wep.SkillBonuses, prop);

                foreach (int i in Enum.GetValues(typeof(AosWeaponAttribute)))
                {
                    AosWeaponAttribute attr = (AosWeaponAttribute)i;

                    if (!ItemPropertyInfo.ValidateProperty(attr))
                    {
                        continue;
                    }

                    if (wep.WeaponAttributes[attr] > 0)
                    {
                        if (IsHitAreaOrSpell(attr, id))
                        {
                            continue;
                        }

                        if (!(prop is AosWeaponAttribute) || (AosWeaponAttribute)prop != attr)
                        {
                            total++;
                        }
                    }
                }

                foreach (int i in Enum.GetValues(typeof(ExtendedWeaponAttribute)))
                {
                    ExtendedWeaponAttribute attr = (ExtendedWeaponAttribute)i;

                    if (!ItemPropertyInfo.ValidateProperty(attr))
                    {
                        continue;
                    }

                    if (wep.ExtendedWeaponAttributes[attr] > 0 && (!(prop is ExtendedWeaponAttribute) || (ExtendedWeaponAttribute)prop != attr))
                    {
                        total++;
                    }
                }

                if (wep.Slayer != SlayerName.None && (!(prop is SlayerName) || (SlayerName)prop != wep.Slayer))
                {
                    total++;
                }

                if (wep.Slayer2 != SlayerName.None)
                {
                    total++;
                }

                if (wep.Slayer3 != TalismanSlayerName.None)
                {
                    total++;
                }

                foreach (int i in Enum.GetValues(typeof(SAAbsorptionAttribute)))
                {
                    SAAbsorptionAttribute attr = (SAAbsorptionAttribute)i;

                    if (!ItemPropertyInfo.ValidateProperty(attr))
                    {
                        continue;
                    }

                    if (wep.AbsorptionAttributes[attr] > 0 && (!(prop is SAAbsorptionAttribute) || (SAAbsorptionAttribute) prop != attr))
                    {
                        total++;
                    }
                }

                if (wep is BaseRanged ranged && !(prop is string) && ranged.Velocity > 0 && id != 60)
                {
                    total++;
                }
            }
            else if (item is BaseArmor armor)
            {
                foreach (int i in Enum.GetValues(typeof(AosAttribute)))
                {
                    AosAttribute attr = (AosAttribute)i;

                    if (!ItemPropertyInfo.ValidateProperty(attr))
                    {
                        continue;
                    }

                    if (armor.Attributes[attr] > 0)
                    {
                        if (!(prop is AosAttribute) || (AosAttribute)prop != attr)
                        {
                            total++;
                        }
                    }
                    else if (armor.Attributes[attr] == 0 && attr == AosAttribute.CastSpeed && armor.Attributes[AosAttribute.SpellChanneling] > 0)
                    {
                        if (!(prop is AosAttribute) || (AosAttribute)prop == attr)
                        {
                            total++;
                        }
                    }
                }

                total += GetSkillBonuses(armor.SkillBonuses, prop);

                foreach (int i in Enum.GetValues(typeof(AosArmorAttribute)))
                {
                    AosArmorAttribute attr = (AosArmorAttribute)i;

                    if (!ItemPropertyInfo.ValidateProperty(attr))
                    {
                        continue;
                    }

                    if (armor.ArmorAttributes[attr] > 0 && (!(prop is AosArmorAttribute) || (AosArmorAttribute) prop != attr))
                    {
                        total++;
                    }
                }


                foreach (int i in Enum.GetValues(typeof(SAAbsorptionAttribute)))
                {
                    SAAbsorptionAttribute attr = (SAAbsorptionAttribute)i;

                    if (!ItemPropertyInfo.ValidateProperty(attr))
                    {
                        continue;
                    }

                    if (armor.AbsorptionAttributes[attr] > 0 && (!(prop is SAAbsorptionAttribute) || (SAAbsorptionAttribute) prop != attr))
                    {
                        total++;
                    }
                }
            }
            else if (item is BaseJewel jewel)
            {
                foreach (int i in Enum.GetValues(typeof(AosAttribute)))
                {
                    AosAttribute attr = (AosAttribute)i;

                    if (!ItemPropertyInfo.ValidateProperty(attr))
                    {
                        continue;
                    }

                    if (jewel.Attributes[attr] > 0 && (!(prop is AosAttribute) || (AosAttribute) prop != attr))
                    {
                        total++;
                    }
                }

                foreach (int i in Enum.GetValues(typeof(SAAbsorptionAttribute)))
                {
                    SAAbsorptionAttribute attr = (SAAbsorptionAttribute)i;

                    if (!ItemPropertyInfo.ValidateProperty(attr))
                    {
                        continue;
                    }

                    if (jewel.AbsorptionAttributes[attr] > 0 && (!(prop is SAAbsorptionAttribute) || (SAAbsorptionAttribute) prop != attr))
                    {
                        total++;
                    }
                }

                total += GetSkillBonuses(jewel.SkillBonuses, prop);

                if (jewel.Resistances.Physical > 0 && id != 51)
                {
                    total++;
                }
                if (jewel.Resistances.Fire > 0 && id != 52)
                {
                    total++;
                }
                if (jewel.Resistances.Cold > 0 && id != 53)
                {
                    total++;
                }
                if (jewel.Resistances.Poison > 0 && id != 54)
                {
                    total++;
                }
                if (jewel.Resistances.Energy > 0 && id != 55)
                {
                    total++;
                }
            }
            else if (item is BaseClothing clothing)
            {
                foreach (int i in Enum.GetValues(typeof(AosAttribute)))
                {
                    AosAttribute attr = (AosAttribute)i;

                    if (!ItemPropertyInfo.ValidateProperty(attr))
                    {
                        continue;
                    }

                    if (clothing.Attributes[attr] > 0 && (!(prop is AosAttribute) || (AosAttribute) prop != attr))
                    {
                        total++;
                    }
                }

                foreach (int i in Enum.GetValues(typeof(SAAbsorptionAttribute)))
                {
                    SAAbsorptionAttribute attr = (SAAbsorptionAttribute)i;

                    if (!ItemPropertyInfo.ValidateProperty(attr))
                    {
                        continue;
                    }

                    if (clothing.SAAbsorptionAttributes[attr] > 0 && (!(prop is SAAbsorptionAttribute) || (SAAbsorptionAttribute) prop != attr))
                    {
                        total++;
                    }
                }

                total += GetSkillBonuses(clothing.SkillBonuses, prop);
            }

            return total;
        }

        private static bool IsHitAreaOrSpell(AosWeaponAttribute attr, int id)
        {
            if (attr >= AosWeaponAttribute.HitMagicArrow && attr <= AosWeaponAttribute.HitDispel)
            {
                return id >= 35 && id <= 39;
            }

            if (attr >= AosWeaponAttribute.HitColdArea && attr <= AosWeaponAttribute.HitPhysicalArea)
            {
                return id >= 30 && id <= 34;
            }

            return false;
        }

        private static int GetSkillBonuses(AosSkillBonuses bonus, object prop)
        {
            int id = 0;

            for (int j = 0; j < 5; j++)
            {
                if (bonus.GetBonus(j) > 0)
                {
                    if (!(prop is SkillName) || !IsInSkillGroup(bonus.GetSkill(j), (SkillName)prop))
                    {
                        id += 1;
                    }
                }
            }

            return id;
        }

        public static int GetTotalWeight(Item item, int id, bool trueWeight, bool imbuing)
        {
            double weight = 0;

            AosAttributes aosAttrs = RunicReforging.GetAosAttributes(item);
            AosWeaponAttributes wepAttrs = RunicReforging.GetAosWeaponAttributes(item);
            SAAbsorptionAttributes saAttrs = RunicReforging.GetSAAbsorptionAttributes(item);
            AosArmorAttributes armorAttrs = RunicReforging.GetAosArmorAttributes(item);
            AosElementAttributes resistAttrs = RunicReforging.GetElementalAttributes(item);
            ExtendedWeaponAttributes extattrs = RunicReforging.GetExtendedWeaponAttributes(item);

            if (item is BaseWeapon weapon)
            {
                if (weapon.Slayer != SlayerName.None)
                {
                    weight += GetIntensityForAttribute(weapon, weapon.Slayer, id, 1, trueWeight, imbuing);
                }

                if (weapon.Slayer2 != SlayerName.None)
                {
                    weight += GetIntensityForAttribute(weapon, weapon.Slayer2, id, 1, trueWeight, imbuing);
                }

                if (weapon.Slayer3 != TalismanSlayerName.None)
                {
                    weight += GetIntensityForAttribute(weapon, weapon.Slayer3, id, 1, trueWeight, imbuing);
                }

                if (weapon is BaseRanged ranged && ranged.Velocity > 0)
                {
                    weight += GetIntensityForAttribute(weapon, "WeaponVelocity", id, ranged.Velocity, trueWeight, imbuing);
                }
            }

            if (aosAttrs != null)
            {
                foreach (int i in Enum.GetValues(typeof(AosAttribute)))
                {
                    weight += GetIntensityForAttribute(item, (AosAttribute)i, id, aosAttrs[(AosAttribute)i], trueWeight, imbuing);
                }
            }

            if (wepAttrs != null)
            {
                foreach (int i in Enum.GetValues(typeof(AosWeaponAttribute)))
                {
                    weight += GetIntensityForAttribute(item, (AosWeaponAttribute)i, id, wepAttrs[(AosWeaponAttribute)i], trueWeight, imbuing);
                }
            }

            if (saAttrs != null)
            {
                foreach (int i in Enum.GetValues(typeof(SAAbsorptionAttribute)))
                {
                    weight += GetIntensityForAttribute(item, (SAAbsorptionAttribute)i, id, saAttrs[(SAAbsorptionAttribute)i], trueWeight, imbuing);
                }
            }

            if (armorAttrs != null)
            {
                foreach (int i in Enum.GetValues(typeof(AosArmorAttribute)))
                {
                    weight += GetIntensityForAttribute(item, (AosArmorAttribute)i, id, armorAttrs[(AosArmorAttribute)i], trueWeight, imbuing);
                }
            }

            if (resistAttrs != null && !(item is BaseWeapon))
            {
                foreach (int i in Enum.GetValues(typeof(AosElementAttribute)))
                {
                    weight += GetIntensityForAttribute(item, (AosElementAttribute)i, id, resistAttrs[(AosElementAttribute)i], trueWeight, imbuing);
                }
            }

            if (extattrs != null)
            {
                foreach (int i in Enum.GetValues(typeof(ExtendedWeaponAttribute)))
                {
                    weight += GetIntensityForAttribute(item, (ExtendedWeaponAttribute)i, id, extattrs[(ExtendedWeaponAttribute)i], trueWeight, imbuing);
                }
            }

            weight += CheckSkillBonuses(item, id, trueWeight, imbuing);

            return (int)weight;
        }

        private static int CheckSkillBonuses(Item item, int check, bool trueWeight, bool imbuing)
        {
            double weight = 0;

            AosSkillBonuses skills = RunicReforging.GetAosSkillBonuses(item);

            if (skills != null)
            {
                int id = -1;

                if (item is BaseJewel)
                {
                    id = check;
                }

                // Place Holder. THis is in case the skill weight/max intensity every changes
                int totalWeight = trueWeight ? 100 : ItemPropertyInfo.GetWeight(151);
                int maxInt = ItemPropertyInfo.GetMaxIntensity(item, 151, imbuing);

                for (int i = 0; i < 5; i++)
                {
                    double bonus = skills.GetBonus(i);

                    if (bonus > 0)
                    {
                        object attr = ItemPropertyInfo.GetAttribute(id);

                        if (id < 151 && id > 183 || !(attr is SkillName) || !IsInSkillGroup(skills.GetSkill(i), (SkillName)attr))
                        {
                            weight += totalWeight / maxInt * bonus;
                        }
                    }
                }
            }

            return (int)weight;
        }

        private static readonly SkillName[][] m_SkillGroups =
        {
            new[] { SkillName.Fencing, SkillName.Macing, SkillName.Swords, SkillName.Musicianship, SkillName.Magery },
            new[] { SkillName.Wrestling, SkillName.AnimalTaming, SkillName.SpiritSpeak, SkillName.Tactics, SkillName.Provocation },
            new[] { SkillName.Focus, SkillName.Parry, SkillName.Stealth, SkillName.Meditation, SkillName.AnimalLore, SkillName.Discordance },
            new[] { SkillName.Mysticism, SkillName.Bushido, SkillName.Necromancy, SkillName.Veterinary, SkillName.Stealing, SkillName.EvalInt, SkillName.Anatomy },
            new[] { SkillName.Peacemaking, SkillName.Ninjitsu, SkillName.Chivalry, SkillName.Archery, SkillName.MagicResist, SkillName.Healing }
        };

        public static int GetAvailableSkillIndex(AosSkillBonuses skills)
        {
            for (int i = 0; i < 5; i++)
            {
                if (skills.GetBonus(i) == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        public static int GetSkillGroupIndex(SkillName skill)
        {
            for (int i = 0; i < m_SkillGroups.Length; i++)
            {
                for (int index = 0; index < m_SkillGroups[i].Length; index++)
                {
                    SkillName sk = m_SkillGroups[i][index];

                    if (sk == skill)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public static bool IsInSkillGroup(SkillName one, SkillName two)
        {
            int skillGroup1 = GetSkillGroupIndex(one);
            int skillGroup2 = GetSkillGroupIndex(two);

            return skillGroup1 != -1 && skillGroup2 != -1 && skillGroup1 == skillGroup2;
        }

        public static int GetValueForID(Item item, int id)
        {
            object attr = ItemPropertyInfo.GetAttribute(id);

            if (item is BaseWeapon w)
            {
                if (id == 16 && w.Attributes.SpellChanneling > 0)
                {
                    return w.Attributes[AosAttribute.CastSpeed] + 1;
                }

                if (attr is AosAttribute attribute)
                {
                    return w.Attributes[attribute];
                }

                if (attr is AosWeaponAttribute weaponAttribute)
                {
                    return w.WeaponAttributes[weaponAttribute];
                }

                if (attr is ExtendedWeaponAttribute extendedWeaponAttribute)
                {
                    return w.ExtendedWeaponAttributes[extendedWeaponAttribute];
                }

                if (attr is SAAbsorptionAttribute absorptionAttribute)
                {
                    return w.AbsorptionAttributes[absorptionAttribute];
                }

                if (attr is SlayerName name && w.Slayer == name)
                {
                    return 1;
                }

                if (id == 60 && w is BaseRanged ranged)
                {
                    return ranged.Velocity;
                }

                if (attr is AosElementAttribute ele)
                {
                    switch (ele)
                    {
                        case AosElementAttribute.Physical:
                        {
                            return w.WeaponAttributes.ResistPhysicalBonus;
                        }
                        case AosElementAttribute.Fire:
                        {
                            return w.WeaponAttributes.ResistFireBonus;
                        }
                        case AosElementAttribute.Cold:
                        {
                            return w.WeaponAttributes.ResistColdBonus;
                        }
                        case AosElementAttribute.Poison:
                        {
                            return w.WeaponAttributes.ResistPoisonBonus;
                        }
                        case AosElementAttribute.Energy:
                        {
                            return w.WeaponAttributes.ResistEnergyBonus;
                        }
                    }
                }
            }
            else if (item is BaseArmor a)
            {
                if (a is BaseShield && id == 16 && a.Attributes.SpellChanneling > 0)
                {
                    return a.Attributes[AosAttribute.CastSpeed] + 1;
                }

                if (attr is AosAttribute attribute)
                {
                    return a.Attributes[attribute];
                }

                if (attr is AosArmorAttribute armorAttribute)
                {
                    return a.ArmorAttributes[armorAttribute];
                }

                if (attr is SAAbsorptionAttribute absorptionAttribute)
                {
                    return a.AbsorptionAttributes[absorptionAttribute];
                }

                if (attr is AosElementAttribute ele)
                {
                    int value = 0;

                    switch (ele)
                    {
                        case AosElementAttribute.Physical:
                        {
                            value = a.PhysicalBonus; break;
                        }
                        case AosElementAttribute.Fire:
                        {
                            value = a.FireBonus; break;
                        }
                        case AosElementAttribute.Cold:
                        {
                            value = a.ColdBonus; break;
                        }
                        case AosElementAttribute.Poison:
                        {
                            value = a.PoisonBonus; break;
                        }
                        case AosElementAttribute.Energy:
                        {
                            value = a.EnergyBonus; break;
                        }
                    }

                    if (value > 0)
                    {
                        return value;
                    }
                }
            }
            else if (item is BaseClothing c)
            {
                if (attr is AosAttribute attribute)
                {
                    return c.Attributes[attribute];
                }

                if (attr is AosElementAttribute elementAttribute)
                {
                    int value = c.Resistances[elementAttribute];

                    if (value > 0)
                    {
                        return value;
                    }
                }

                else if (attr is AosArmorAttribute armorAttribute)
                {
                    return c.ClothingAttributes[armorAttribute];
                }

                else if (attr is SAAbsorptionAttribute absorptionAttribute)
                {
                    return c.SAAbsorptionAttributes[absorptionAttribute];
                }
            }
            else if (item is BaseJewel j)
            {
                if (attr is AosAttribute attribute)
                {
                    return j.Attributes[attribute];
                }

                if (attr is AosElementAttribute elementAttribute)
                {
                    return j.Resistances[elementAttribute];
                }

                if (attr is SAAbsorptionAttribute absorptionAttribute)
                {
                    return j.AbsorptionAttributes[absorptionAttribute];
                }

                if (attr is SkillName sk)
                {
                    if (j.SkillBonuses.Skill_1_Name == sk)
                    {
                        return (int)j.SkillBonuses.Skill_1_Value;
                    }

                    if (j.SkillBonuses.Skill_2_Name == sk)
                    {
                        return (int)j.SkillBonuses.Skill_2_Value;
                    }

                    if (j.SkillBonuses.Skill_3_Name == sk)
                    {
                        return (int)j.SkillBonuses.Skill_3_Value;
                    }

                    if (j.SkillBonuses.Skill_4_Name == sk)
                    {
                        return (int)j.SkillBonuses.Skill_4_Value;
                    }

                    if (j.SkillBonuses.Skill_5_Name == sk)
                    {
                        return (int)j.SkillBonuses.Skill_5_Value;
                    }
                }
            }

            return 0;
        }

        public static int GetIntensityForAttribute(Item item, object attr, int checkID, int value, bool trueWeight, bool imbuing)
        {
            return GetIntensityForID(item, ItemPropertyInfo.GetID(attr), checkID, value, trueWeight, imbuing);
        }

        public static int GetIntensityForID(Item item, int id, int checkID, int value)
        {
            return GetIntensityForID(item, id, checkID, value, false);
        }

        public static int GetIntensityForID(Item item, int id, int checkID, int value, bool trueWeight)
        {
            return GetIntensityForID(item, id, checkID, value, trueWeight, true);
        }

        public static int GetIntensityForID(Item item, int id, int checkID, int value, bool trueWeight, bool imbuing)
        {
            // This is terribly clunky, however we're accomidating 1 out of 50+ attributes that acts differently
            if (value <= 0 && id != 16)
            {
                return 0;
            }

            if (id == 61 && !(item is BaseRanged))
            {
                id = 63;
            }

            if ((item is BaseWeapon || item is BaseShield) && id == 16)
            {
                AosAttributes attrs = RunicReforging.GetAosAttributes(item);

                if (attrs != null && attrs.SpellChanneling > 0)
                {
                    value++;
                }
            }

            if (value <= 0)
            {
                return 0;
            }

            if (id != checkID)
            {
                int weight = trueWeight ? 100 : ItemPropertyInfo.GetWeight(id);

                if (weight == 0)
                {
                    return 0;
                }

                int max = ItemPropertyInfo.GetMaxIntensity(item, id, imbuing);

                return (int)((double)weight / max * value);
            }

            return 0;
        }
    }
}
