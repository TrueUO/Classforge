using Server.Items;
using Server.Mobiles;

namespace Server.Services.ClassSystem
{
    public static class RogueClass
    {
        public const string Name = "Rogue";

        public static readonly SkillName[] PrimarySkills =
        [
            SkillName.Fencing, SkillName.Stealth
        ];

        public static readonly SkillName[] SecondarySkills =
        [

        ];

        public static readonly SkillName[] CraftSkills =
        [
           
        ];

        // Called one time when the class is selected.
        public static void OnClassSelected(PlayerMobile pm)
        {
            // +15 Dex +5 Str bonus to start
            pm.RawDex += 15;
            pm.RawStr += 5;

            // starting gear
            pm.EquipItem(new Kryss());
            pm.EquipItem(new LeatherChest());
            pm.EquipItem(new LongPants());
            pm.EquipItem(new Shoes());
        }
    }
}
