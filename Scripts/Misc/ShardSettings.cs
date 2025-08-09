using Server.Accounting;
using Server.Services.TownCryer;

namespace Server
{
    public static class ShardSettings
    {
        public const string ShardName = "Classforge";

        [CallPriority(int.MinValue)]
        public static void Configure()
        {
            AccountGold.ConvertOnBank = true;
            VirtualCheck.UseEditGump = true;

            TownCryerSystem.Enabled = true;

            Mobile.VisibleDamageType = VisibleDamageType.Related;

            AOS.DisableStatInfluences();

            Mobile.AOSStatusHandler = AOS.GetStatus;
        }
    }
}
