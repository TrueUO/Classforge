using Server.Accounting;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;

namespace Server.Misc
{
    public class LoginStats
    {
        public static void OnLogin(PlayerMobile pm)
        {
            // this is needed since we override the normal OnSkillsQuery in PlayerMobile
            // to display the new skills gump. This packet makes sure the skills update
            // to their values on first login automatically.
            // TODO: better way to do this?
            pm.Send(new SkillUpdate(pm.Skills));

            // Move this somewhere else eventually. Was originally called in the removed VeteranReward
            // system. Allows for "dynamic" changing of the skills cap on each login a PlayerMobile
            // has their skill cap re-set.
            pm.SkillsCap = 7200;

            // 15 days of game time = +5 stat reward that used to be
            // rewarded from the removed Veteran Reward System.
            if (pm.Alive && !pm.HasStatReward && HasEarnedStatReward(pm))
            {
                BaseGump.SendGump(new StatRewardGump(pm));
            }

            pm.SendMessage($"Welcome, {pm.Name}!");
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
