using System;
using Server.Accounting;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

namespace Server.RemoteAdmin
{
    public static class Statistics
    {
		public static string GetReceiveData()
		{ 
            return $", {ServerList.ServerName}" +
                   $", {GetShardAge()}" +
                   $", {GetUpTime()}" +
                   $", {NetState.Instances.Count}" +
                   $", {World.Items.Count}" +
                   $", {World.Mobiles.Count}" +
				   $", {GetTotalGold()}" +
                   $", {(int) (GC.GetTotalMemory(false) / 1024 / 1024)}" +
                   $", {GetPlayerHouseAndVendor()}";
		}

        private static string GetShardAge()
		{
			DateTime shardCreation = DateTime.UtcNow;

			foreach (IAccount account in Accounts.GetAccounts())
            {
                Account a = (Account) account;

                if (a.Created < shardCreation)
                {
                    shardCreation = a.Created;
                }
            }

			return (DateTime.UtcNow - shardCreation).Days.ToString();
		}

        private static string GetPlayerHouseAndVendor()
		{
			int playerHouses = 0;
			int playerVendors = 0;

			foreach (Item i in World.Items.Values)
			{
				if (i is BaseHouse house)
				{
                    playerHouses++;
                    playerVendors += house.PlayerVendors.Count;
				}
			}

			return playerHouses + ", " + playerVendors;
		}

		public static string GetTotalGold()
        {
            double gold = 0;

            foreach (IAccount account in Accounts.GetAccounts())
            {
                Account a = (Account) account;

                if (a.AccessLevel > AccessLevel.Player)
                {
                    continue;
                }

                a.GetGoldBalance(out int g, out double t);

                gold += t;
            }

            foreach (Item i in World.Items.Values)
            {
                if (!i.Deleted && (i is Gold || i is BankCheck))
                {
                    if (i.RootParent is Mobile p && p.AccessLevel > AccessLevel.Player)
                    {
                        continue;
                    }

                    if (i is Gold)
                    {
                        gold += i.Amount;
                    }
                    else if (i is BankCheck check)
                    {
                        gold += check.Worth;
                    }
                }
            }

            for (int index = 0; index < PlayerVendor.PlayerVendors.Count; index++)
            {
                PlayerVendor vendor = PlayerVendor.PlayerVendors[index];

                gold += vendor.BankAccount + vendor.HoldGold;
            }

            return gold.ToString();
        }

        private static string GetUpTime()
		{
			TimeSpan uptime = DateTime.UtcNow - Clock.ServerStart;

			return $"{uptime.Days:n0}:{uptime.Hours:n0}:{uptime.Minutes:n0}";
		}
    }
}
