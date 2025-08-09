using Server.Items;
using Server.Mobiles;

namespace Server.Services.ClassSystem
{
    public static class MageClass
    {
        public const string Name = "Mage";

        public static readonly SkillName[] PrimarySkills =
        [
            SkillName.Magery, SkillName.EvalInt
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
            // +20 Int bonus to start
            pm.RawInt += 20;

            // starting gear
            pm.EquipItem(new Spellbook((ulong)0x70));
            pm.EquipItem(new WizardsHat());
            pm.EquipItem(new Robe(Utility.RandomBlueHue()));
            pm.EquipItem(new Shoes());

            pm.AddToBackpack(new BagOfReagents(50));
        }
    }
}
