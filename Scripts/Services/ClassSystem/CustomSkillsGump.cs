using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Services.ClassSystem
{
    public class CustomSkillGump : BaseGump
    {
        private readonly PlayerMobile _Pm;

        public const int Width = 510;
        public const int Height = 510;

        public static int White => 1152;
        public static int Yellow => 53;

        public CustomSkillGump(PlayerMobile user)
            : base(user, 100, 100)
        {
            _Pm = user;

            AddGumpLayout();
        }

        public override void AddGumpLayout()
        {
            AddPage(0);
            AddBackground(0, 0, Width, Height, 9200);

            // Title
            AddImageTiled(10, 10, Width - 20, 25, 2624);
            AddAlphaRegion(10, 10, Width - 20, 25);
            AddLabel(205, 12, 0xD7, "Skills & Stats");

            // Remaining Points
            AddLabel(20, 50, Yellow, "Stat Points To Spend:");
            AddLabel(170, 50, White, $"{_Pm.StatPointsToSpend}");

            AddLabel(270, 50, Yellow, $"Skill Points To Spend:");
            AddLabel(420, 50, White, $"{_Pm.SkillPointsToSpend}");

            // ——— Stats ———
            AddLabel(20, 90, Yellow, "STATS:");
            AddLabel(20, 120, Yellow, "Strength:");
            AddLabel(20, 150, Yellow, "Dexterity:");
            AddLabel(20, 180, Yellow, "Intelligence:");

            AddLabel(120, 120, White, $"{_Pm.RawStr}");
            AddLabel(120, 150, White, $"{_Pm.RawDex}");
            AddLabel(120, 180, White, $"{_Pm.RawInt}");

            // Stat + buttons
            AddButton(180, 120, 4014, 4015, 1, GumpButtonType.Reply, 0);  // Str+
            AddButton(180, 150, 4014, 4015, 2, GumpButtonType.Reply, 0);  // Dex+
            AddButton(180, 180, 4014, 4015, 3, GumpButtonType.Reply, 0);  // Int+

            // ——— Skills ———
            AddLabel(270, 90, Yellow, "PRIMARY SKILLS:"); // PRIMARY SKILLS
            SkillName[] primarySkills = ClassSystemHelper.GetPrimarySkills(_Pm.CharacterClass); 
            for (int i = 0; i < primarySkills.Length; i++)
            {
                SkillName sk = primarySkills[i];

                int y = 120 + i * 30;
                double cur = _Pm.Skills[sk].Base;

                AddLabel(270, y, Yellow, $"{sk}"); // skill name
                AddLabel(340, y, White, $"{cur:F1}"); // skill value
                AddLabel(370, y, White, $" / {_Pm.Skills[sk].Cap}"); // skill cap
                AddButton(445, y, 4014, 4015, 10 + i, GumpButtonType.Reply, 0); // skill+
            }

            AddLabel(270, 230, Yellow, "CRAFTING SKILLS:"); // CRAFTING SKILLS
            SkillName[] craftingSkills = ClassSystemHelper.GetCraftSkills(_Pm.CharacterClass);
            for (int i = 0; i < craftingSkills.Length; i++)
            {
                SkillName sk = craftingSkills[i];

                int y = 260 + i * 30;
                double cur = _Pm.Skills[sk].Base;

                AddLabel(270, y, Yellow, $"{sk}"); // skill name
                AddLabel(340, y, White, $"{cur:F1}"); // skill value
                AddLabel(370, y, White, $" / {_Pm.Skills[sk].Cap}"); // skill cap
                AddButton(445, y, 4014, 4015, 20 + i, GumpButtonType.Reply, 0); // skill+
            }

            // button to access the old skills menu for now as well to access some skills directly.
            // need to change in the future to be able to do everything from new menu
            AddButton(20, 390, 0x7E1, 0x7E0, 90, GumpButtonType.Reply, 0);

            AddLabel(20, 420, Yellow, $"Class: {ClassSystemHelper.GetClassName(_Pm.CharacterClass)}");
            AddLabel(20, 445, Yellow, $"Level: {_Pm.Level}/{ClassSystemHelper.MaxLevel}");
            AddLabel(20, 470, Yellow, $"Progress: {_Pm.Xp}/{ClassSystemHelper.XpNeededForNextLevel(_Pm.Level)} XP toward Level {_Pm.Level + 1}");

            // Close Button
            AddButton(Width - 50, Height - 40, 4017, 4018, 0, GumpButtonType.Reply, 0);
            AddLabel(Width - 85, Height - 38, White, "Close");
        }

        public override void OnResponse(RelayInfo info)
        {
            int id = info.ButtonID;

            if (id == 0)
            {
                return;  // Close
            }

            // show older skills menu
            if (id == 90)
            {
                _Pm.Send(new SkillUpdate(_Pm.Skills));
            }

            // Stats
            if (id == 1 && _Pm.StatPointsToSpend > 0)
            {
                _Pm.RawStr++;
                _Pm.StatPointsToSpend--;
            }
            else if (id == 2 && _Pm.StatPointsToSpend > 0)
            {
                _Pm.RawDex++;
                _Pm.StatPointsToSpend--;
            }
            else if (id == 3 && _Pm.StatPointsToSpend > 0)
            {
                _Pm.RawInt++;
                _Pm.StatPointsToSpend--;
            }

            // Skills
            SkillName[] allowedPrimary = ClassSystemHelper.GetPrimarySkills(_Pm.CharacterClass);
            if (_Pm.SkillPointsToSpend > 0 && id >= 10 && id < 20)
            {
                SkillName ap = allowedPrimary[id - 10]; // ids 10 to 19

                _Pm.Skills[ap].Base += 0.1;  // +0.1 per point
                _Pm.SkillPointsToSpend--;
            }

            SkillName[] allowedCraft = ClassSystemHelper.GetCraftSkills(_Pm.CharacterClass);
            if (_Pm.SkillPointsToSpend > 0 && id >= 20 && id < 30)
            {
                SkillName ac = allowedCraft[id - 20]; // ids 20 to 29

                _Pm.Skills[ac].Base += 0.1;  // +0.1 per point
                _Pm.SkillPointsToSpend--;
            }

            // Refresh to show new values
            _Pm.CloseGump(typeof(CustomSkillGump));
            _Pm.SendGump(new CustomSkillGump(_Pm));
        }
    }
}
