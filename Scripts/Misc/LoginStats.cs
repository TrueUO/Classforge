using Server.Accounting;
using Server.Gumps;
using Server.Mobiles;
using System;

namespace Server.Misc
{
    public class LoginStats
    {
        public static void OnLogin(Mobile m)
        {
            m.SendMessage($"Welcome, {m.Name}!");

            // move this somewhere else eventually.
            m.SkillsCap = 7200;

            // 15 days of game time = +5 stat reward that used to be
            // rewarded from the Veteran Reward System.
            if (m is PlayerMobile pm && pm.Alive && !pm.HasStatReward && HasEarnedStatReward(pm))
            {
                BaseGump.SendGump(new StatRewardGump(pm));
            }
        }

        public static bool HasEarnedStatReward(Mobile mob)
        {
            Account acct = mob.Account as Account;

            if (acct == null)
            {
                return false;
            }

            TimeSpan totalTime = DateTime.UtcNow - acct.Created;

            if (totalTime < TimeSpan.FromDays(15))
            {
                return false;
            }

            return true;
        }
    }
}
