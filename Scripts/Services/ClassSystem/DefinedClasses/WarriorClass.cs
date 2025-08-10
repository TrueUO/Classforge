using Server.Items;
using Server.Mobiles;

namespace Server.Services.ClassSystem
{
    public static class WarriorClass
    {
        public const string Name = "Warrior";

        public static readonly SkillName[] PrimarySkills =
        [
            SkillName.Swords, SkillName.Parry
        ];

        public static readonly SkillName[] SecondarySkills =
        [
           SkillName.Tactics
        ];

        public static readonly SkillName[] CraftSkills =
        [
           SkillName.Blacksmith
        ];

        // Called one time when the class is selected.
        public static void OnClassSelected(PlayerMobile pm)
        {
            // +20 Str bonus to start
            pm.RawStr += 20;

            // starting gear
            pm.EquipItem(new Cutlass());
            pm.EquipItem(new WoodenShield());
            pm.EquipItem(new StuddedChest());
            pm.EquipItem(new LongPants());
            pm.EquipItem(new Shoes());
        }
    }
}
