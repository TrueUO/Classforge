using Server.Items;
using Server.Mobiles;

namespace Server.Services.ClassSystem
{
    public static class RangerClass
    {
        public const string Name = "Ranger";

        public static readonly SkillName[] PrimarySkills =
        [
            SkillName.Archery, SkillName.Tracking
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
            // +10 Str/Dex bonus to start
            pm.RawStr += 10;
            pm.RawDex += 10;

            // starting gear
            pm.EquipItem(new Bow());
            pm.EquipItem(new Shoes());

            pm.AddToBackpack(new Arrow(50));
        }
    }
}
