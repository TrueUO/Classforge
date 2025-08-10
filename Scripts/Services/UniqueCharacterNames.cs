using Server.Misc;
using Server.Mobiles;

namespace Server.Gumps
{
    public static class UniqueCharacterNames
    {
        public static void CheckNameOnLogin(Mobile m)
        {
            m.SendMessage(33, $"Your character name '{m.Name}' is already in use by another player or it has been marked as invalid. Please choose a new one. You will remain frozen until a new name is picked.");
            m.RawName = "Generic Player";
            BaseGump.SendGump(new NameChangeGump((PlayerMobile)m));
            m.Frozen = true;
        }

        public static bool HasValidName(Mobile m)
        {
            if (m.AccessLevel != AccessLevel.Player || m.RawName == null)
            {
                return true;
            }

            if (m.RawName == "Generic Player" || !NameVerification.Validate(m.RawName, 2, 16, true, false, true, 1, NameVerification.SpaceDashPeriodQuote))
            {
                return false;
            }

            bool any = false;

            foreach (Mobile value in World.Mobiles.Values)
            {
                if (value is PlayerMobile pm && pm != m && pm.RawName != null && pm.RawName.ToLower() == m.RawName.ToLower())
                {
                    any = true;
                    break;
                }
            }

            return !any;
        }

        private class NameChangeGump : BaseGump
        {
            public NameChangeGump(PlayerMobile pm)
                : base(pm, 250, 250)
            {
            }

            public override void AddGumpLayout()
            {
                Closable = false;
                Dragable = false;
                Resizable = false;

                AddPage(0);
                AddBackground(0, 0, 424, 200, 9500);
                AddImage(5, 10, 5411);

                AddHtml(0, 10, 424, 20, ColorAndCenter("#7FFFD4", "Invalid Name"), false, false);
                AddHtml(10, 40, 404, 100, $"<BASEFONT COLOR=#1E90FF>{_Message}</BASEFONT>", true, true);

                AddLabel(10, 145, 0x480, "New Name:");
                AddTextField(90, 145, 150, 20, 0);
                AddButtonLabeled(10, 170, 1, "Submit");
            }

            public override void OnResponse(RelayInfo info)
            {
                if (info.ButtonID != 1)
                    return;

                TextRelay nameEntry = info.GetTextEntry(0);

                User.RawName = nameEntry != null ? nameEntry.Text.Trim() : "Generic Player";

                if (HasValidName(User) && !string.IsNullOrEmpty(User.RawName))
                {
                    User.SendMessage(66, "Your name has been changed! You are now known as '{0}'.", User.RawName);
                    User.Frozen = false;
                }
                else
                {
                    User.SendMessage(33, "You can't use that name as another player has already claimed it. Please choose another one.");
                    User.RawName = "Generic Player";

                    Refresh();
                }
            }

            private void AddTextField(int x, int y, int width, int height, int index)
            {
                AddBackground(x - 2, y - 2, width + 4, height + 4, 0x2486);
                AddTextEntry(x + 2, y + 2, width - 4, height - 4, 0, index, "");
            }

            private void AddButtonLabeled(int x, int y, int buttonID, string text)
            {
                AddButton(x, y - 1, 4005, 4007, buttonID, GumpButtonType.Reply, 0);
                AddHtml(x + 35, y, 240, 20, Color(0xFFFFFF, text), false, false);
            }

            private const string _Message = "The name you chose is already in use or is invalid. Please choose another name and click submit. You " + "will remain frozen until you have chosen a valid name. Keep in mind, any name with foul language or certain special characters " + "will be deemed invalid.";
        }
    }
}
