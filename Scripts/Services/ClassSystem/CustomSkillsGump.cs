using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Services.ClassSystem
{
    public class CustomSkillGump : BaseGump
    {
        private readonly PlayerMobile _Pm;

        private const int _Width = 510;
        private const int _Height = 530;

        private static int White => 1152;
        private static int Yellow => 53;

        public CustomSkillGump(PlayerMobile user)
            : base(user, 100, 100)
        {
            _Pm = user;

            AddGumpLayout();
        }

        public override void AddGumpLayout()
        {
            AddPage(0);
            AddBackground(0, 0, _Width, _Height, 9200);

            // Title
            AddImageTiled(10, 10, _Width - 20, 25, 2624);
            AddAlphaRegion(10, 10, _Width - 20, 25);
            AddLabel(210, 12, 0xD7, "Skills & Stats");

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
                SkillName primary = primarySkills[i];

                int y = 120 + i * 30;
                double cur = _Pm.Skills[primary].Base;

                AddLabel(270, y, Yellow, $"{primary}"); // skill name
                AddLabel(340, y, White, $"{cur:F1}"); // skill value
                AddLabel(370, y, White, $" / {_Pm.Skills[primary].Cap}"); // skill cap

                // disable button if Skill value is at cap.
                if (_Pm.Skills[primary].Value < _Pm.Skills[primary].Cap)
                {
                    AddButton(445, y, 4014, 4015, 10 + i, GumpButtonType.Reply, 0); // skill+
                }
            }

            AddLabel(270, 230, Yellow, "SECONDARY SKILLS:"); // SECONDARY SKILLS
            SkillName[] secondarySkills = ClassSystemHelper.GetSecondarySkills(_Pm.CharacterClass);
            for (int i = 0; i < secondarySkills.Length; i++)
            {
                SkillName secondary = secondarySkills[i];

                int y = 260 + i * 30;
                double cur = _Pm.Skills[secondary].Base;

                AddLabel(270, y, Yellow, $"{secondary}"); // skill name
                AddLabel(340, y, White, $"{cur:F1}"); // skill value
                AddLabel(370, y, White, $" / {_Pm.Skills[secondary].Cap}"); // skill cap

                // disable button if Skill value is at cap.
                if (_Pm.Skills[secondary].Value < _Pm.Skills[secondary].Cap)
                {
                    AddButton(445, y, 4014, 4015, 20 + i, GumpButtonType.Reply, 0); // skill+
                }
            }

            AddLabel(270, 360, Yellow, "CRAFTING SKILLS:"); // CRAFTING SKILLS
            SkillName[] craftingSkills = ClassSystemHelper.GetCraftSkills(_Pm.CharacterClass);
            for (int i = 0; i < craftingSkills.Length; i++)
            {
                SkillName craft = craftingSkills[i];

                int y = 390 + i * 30;
                double cur = _Pm.Skills[craft].Base;

                AddLabel(270, y, Yellow, $"{craft}"); // skill name
                AddLabel(340, y, White, $"{cur:F1}"); // skill value
                AddLabel(370, y, White, $" / {_Pm.Skills[craft].Cap}"); // skill cap

                // disable button if Skill value is at cap.
                if (_Pm.Skills[craft].Value < _Pm.Skills[craft].Cap)
                {
                    AddButton(445, y, 4014, 4015, 30 + i, GumpButtonType.Reply, 0); // skill+
                }
            }

            // button to access the old skills menu for now as well to access some skills directly.
            // need to change in the future to be able to do everything from new menu
            AddButton(20, 405, 0x7E1, 0x7E0, 90, GumpButtonType.Reply, 0);

            AddLabel(20, 440, Yellow, $"Class: {ClassSystemHelper.GetClassName(_Pm.CharacterClass)}");
            AddLabel(20, 465, Yellow, $"Level: {_Pm.Level}/{ClassSystemHelper.MaxLevel}");
            AddLabel(20, 490, Yellow, $"Progress: {_Pm.Xp}/{ClassSystemHelper.XpNeededForNextLevel(_Pm.Level)} XP toward Level {_Pm.Level + 1}");

            // Close Button
            AddButton(_Width - 50, _Height - 40, 4017, 4018, 0, GumpButtonType.Reply, 0);
            AddLabel(_Width - 85, _Height - 38, White, "Close");
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
                SkillName primary = allowedPrimary[id - 10]; // ids 10 to 19

                _Pm.Skills[primary].Base += 0.1;  // +0.1 per point
                _Pm.SkillPointsToSpend--;
            }

            SkillName[] allowedSecondary = ClassSystemHelper.GetSecondarySkills(_Pm.CharacterClass);
            if (_Pm.SkillPointsToSpend > 0 && id >= 20 && id < 30)
            {
                SkillName secondary = allowedSecondary[id - 20]; // ids 20 to 29

                _Pm.Skills[secondary].Base += 0.1;  // +0.1 per point
                _Pm.SkillPointsToSpend--;
            }

            SkillName[] allowedCraft = ClassSystemHelper.GetCraftSkills(_Pm.CharacterClass);
            if (_Pm.SkillPointsToSpend > 0 && id >= 30 && id < 40)
            {
                SkillName craft = allowedCraft[id - 30]; // ids 30 to 39

                _Pm.Skills[craft].Base += 0.1;  // +0.1 per point
                _Pm.SkillPointsToSpend--;
            }

            // Refresh to show new values
            _Pm.CloseGump(typeof(CustomSkillGump));
            _Pm.SendGump(new CustomSkillGump(_Pm));
        }
    }
}
