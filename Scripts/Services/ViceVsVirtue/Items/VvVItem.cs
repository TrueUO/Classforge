namespace Server.Items
{
    public interface IVvVItem
    {
        bool IsVvVItem { get; set; }
    }

    public static class VvVEquipment
    {
        public static void CheckProperties(Item item)
        {
            if (item is PrimerOnArmsTalisman talisman && talisman.Attributes.AttackChance != 10)
            {
                talisman.Attributes.AttackChance = 10;
            }

            if (item is ClaininsSpellbook spellbook && spellbook.Attributes.LowerManaCost != 10)
            {
                spellbook.Attributes.LowerManaCost = 10;
            }

            if (item is CrimsonCincture cincture && cincture.Attributes.BonusDex != 10)
            {
                cincture.Attributes.BonusDex = 10;
            }

            if (item is CrystallineRing ring && ring.Attributes.CastRecovery != 3)
            {
                ring.Attributes.CastRecovery = 3;
            }

            if (item is HeartOfTheLion lion)
            {
                if (lion.PhysicalBonus != 5)
                {
                    lion.PhysicalBonus = 5;
                }

                if (lion.FireBonus != 5)
                {
                    lion.FireBonus = 5;
                }

                if (lion.ColdBonus != 5)
                {
                    lion.ColdBonus = 5;
                }

                if (lion.PoisonBonus != 5)
                {
                    lion.PoisonBonus = 5;
                }

                if (lion.EnergyBonus != 5)
                {
                    lion.EnergyBonus = 5;
                }
            }

            if (item is HuntersHeaddress hunters)
            {
                if (hunters.Resistances.Physical != 8)
                {
                    hunters.Resistances.Physical = 8;
                }

                if (hunters.Resistances.Fire != 4)
                {
                    hunters.Resistances.Fire = 4;
                }

                if (hunters.Resistances.Cold != -8)
                {
                    hunters.Resistances.Cold = -8;
                }

                if (hunters.Resistances.Poison != 9)
                {
                    hunters.Resistances.Poison = 9;
                }

                if (hunters.Resistances.Energy != 3)
                {
                    hunters.Resistances.Energy = 3;
                }
            }

            if (item is KasaOfTheRajin kasa && kasa.Attributes.DefendChance != 10)
            {
                kasa.Attributes.DefendChance = 10;
            }

            if (item is OrnamentOfTheMagician magician && magician.Attributes.RegenMana != 3)
            {
                magician.Attributes.RegenMana = 3;
            }

            if (item is RingOfTheVile vile && vile.Attributes.AttackChance != 25)
            {
                vile.Attributes.AttackChance = 25;
            }

            if (item is RuneBeetleCarapace carapace)
            {
                if (carapace.PhysicalBonus != 3)
                {
                    carapace.PhysicalBonus = 3;
                }

                if (carapace.FireBonus != 3)
                {
                    carapace.FireBonus = 3;
                }

                if (carapace.ColdBonus != 3)
                {
                    carapace.ColdBonus = 3;
                }

                if (carapace.PoisonBonus != 3)
                {
                    carapace.PoisonBonus = 3;
                }

                if (carapace.EnergyBonus != 3)
                {
                    carapace.EnergyBonus = 3;
                }
            }

            if (item is SpiritOfTheTotem totem)
            {
                if (totem.Resistances.Fire != 7)
                {
                    totem.Resistances.Fire = 7;
                }

                if (totem.Resistances.Cold != 2)
                {
                    totem.Resistances.Cold = 2;
                }

                if (totem.Resistances.Poison != 6)
                {
                    totem.Resistances.Poison = 6;
                }

                if (totem.Resistances.Energy != 6)
                {
                    totem.Resistances.Energy = 6;
                }
            }

            if (item is Stormgrip stormgrip && stormgrip.Attributes.AttackChance != 10)
            {
                stormgrip.Attributes.AttackChance = 10;
            }

            if (item is InquisitorsResolution inquis)
            {
                if (inquis.PhysicalBonus != 5)
                {
                    inquis.PhysicalBonus = 5;
                }

                if (inquis.FireBonus != 7)
                {
                    inquis.FireBonus = 7;
                }

                if (inquis.ColdBonus != -2)
                {
                    inquis.ColdBonus = -2;
                }

                if (inquis.PoisonBonus != 7)
                {
                    inquis.PoisonBonus = 7;
                }

                if (inquis.EnergyBonus != -7)
                {
                    inquis.EnergyBonus = -7;
                }
            }

            if (item is TomeOfLostKnowledge knowledge && knowledge.Attributes.RegenMana != 3)
            {
                knowledge.Attributes.RegenMana = 3;
            }
        }
    }
}
