using Server.Commands;
using Server.Engines.Shadowguard;

namespace Server
{
    public static class GenerateShadowguardCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("GenerateShadowguard", AccessLevel.Administrator, GenerateShadowguard_OnCommand);
        }

        [Usage("GenerateShadowguard")]
        [Description("Generates the Shadowguard dungeon system.")]
        private static void GenerateShadowguard_OnCommand(CommandEventArgs e)
        {
            ShadowguardController.SetupShadowguard(e.Mobile);

            e.Mobile.SendMessage("Shadowguard generating complete.");
        }
    }
}
