using System;
using System.Threading.Tasks;
using Server.Accounting;
using Server.Network;

namespace Server.Misc
{
    internal static class ServerConsole
    {
        private static bool _HearConsole;

        public static void Initialize()
        {
            ListenCommands();

            if (_HearConsole)
            {
                Console.WriteLine("Now listening to the whole shard.");
            }

            EventSink.Speech += args =>
            {
                if (args.Mobile == null || !_HearConsole)
                {
                    return;
                }

                try
                {
                    if (args.Mobile.Region.Name.Length > 0)
                    {
                        Console.WriteLine(args.Mobile.Name + " (" + args.Mobile.Region.Name + "): " + args.Speech);
                    }
                    else
                    {
                        Console.WriteLine("" + args.Mobile.Name + ": " + args.Speech + "");
                    }
                }
                catch (Exception e)
                {
                    Diagnostics.ExceptionLogging.LogException(e);
                }
            };
        }

        private static void ListenCommands()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                string line;

                while ((line = Console.ReadLine()) != null)
                {
                    if (Core.Crashed || Core.Closing || World.Loading || World.Saving)
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(line))
                    {
                        return;
                    }

                    ProcessCommand(line);
                }
            });

            Task.WhenAny(task, Task.Delay(TimeSpan.FromMilliseconds(250)));
        }

        private static void ProcessCommand(string input)
        {
            input = input.Trim();

            if (input.StartsWith("bc", StringComparison.OrdinalIgnoreCase))
            {
                string sub = input.Substring(2).Trim();

                BroadcastMessage(AccessLevel.Player, 0x35, $"[Admin] {sub}");

                Console.WriteLine("[World]: {0}", sub);
                return;
            }

            if (input.StartsWith("sc", StringComparison.OrdinalIgnoreCase))
            {
                string sub = input.Substring(2).Trim();

                BroadcastMessage(AccessLevel.Counselor, 0x32, $"[Admin] {sub}");

                Console.WriteLine("[Staff]: {0}", sub);
                return;
            }

            if (input.StartsWith("ban", StringComparison.OrdinalIgnoreCase))
            {
                string sub = input.Substring(3).Trim();

                System.Collections.Generic.List<NetState> states = NetState.Instances;

                if (states.Count == 0)
                {
                    Console.WriteLine("There are no players online.");
                    return;
                }

                NetState ns = states.Find(o => o.Account != null && o.Mobile != null && Insensitive.StartsWith(sub, o.Mobile.RawName));

                if (ns != null)
                {
                    Console.WriteLine("[Ban]: {0}: Mobile: '{1}' Account: '{2}'", ns, ns.Mobile.RawName, ns.Account.Username);

                    ns.Dispose();
                }

                return;
            }

            if (input.StartsWith("kick", StringComparison.OrdinalIgnoreCase))
            {
                string sub = input.Substring(4).Trim();

                System.Collections.Generic.List<NetState> states = NetState.Instances;

                if (states.Count == 0)
                {
                    Console.WriteLine("There are no players online.");
                    return;
                }

                NetState ns = states.Find(o => o.Account != null && o.Mobile != null && Insensitive.StartsWith(sub, o.Mobile.RawName));

                if (ns != null)
                {
                    Console.WriteLine("[Kick]: {0}: Mobile: '{1}' Account: '{2}'", ns, ns.Mobile.RawName, ns.Account.Username);

                    ns.Dispose();
                }

                return;
            }

            switch (input)
            {
                case "crash":
                    {
                        Timer.DelayCall(() => { throw new Exception("Forced Crash"); });
                    }
                    break;
                case "shutdown":
                    {
                        AutoSave.Save();
                        Core.Kill(false);
                    }
                    break;
                case "shutdown nosave":
                    {
                        Core.Kill(false);
                    }
                    break;
                case "restart":
                    {
                        AutoSave.Save();
                        Core.Kill(true);
                    }
                    break;
                case "restart nosave":
                    {
                        Core.Kill(true);
                    }
                    break;
                case "online":
                    {
                        System.Collections.Generic.List<NetState> states = NetState.Instances;

                        if (states.Count == 0)
                        {
                            Console.WriteLine("There are no users online at this time.");
                        }

                        foreach (NetState t in states)
                        {
                            if (!(t.Account is Account a))
                            {
                                continue;
                            }

                            Mobile m = t.Mobile;

                            if (m != null)
                            {
                                Console.WriteLine("- Account: {0}, Name: {1}, IP: {2}", a.Username, m.Name, t);
                            }
                        }
                    }
                    break;
                case "save":
                {
                    AutoSave.Save();
                    break;
                }
                case "hear": // Credit to Zippy for the HearAll script!
                    {
                        _HearConsole = !_HearConsole;

                        Console.WriteLine("{0} sending speech to the console.", _HearConsole ? "Now" : "No longer");
                    }
                    break;
                default:
                {
                    DisplayHelp();
                    break;
                }
            }
        }

        private static void DisplayHelp()
        {
            Console.WriteLine(" ");
            Console.WriteLine("Commands:");
            Console.WriteLine("crash           - Forces an exception to be thrown.");
            Console.WriteLine("save            - Performs a forced save.");
            Console.WriteLine("shutdown        - Performs a forced save then shuts down the server.");
            Console.WriteLine("shutdown nosave - Shuts down the server without saving.");
            Console.WriteLine("restart         - Sends a message to players informing them that the server is");
            Console.WriteLine("                  restarting, performs a forced save, then shuts down and");
            Console.WriteLine("                  restarts the server.");
            Console.WriteLine("restart nosave  - Restarts the server without saving.");
            Console.WriteLine("online          - Shows a list of every person online:");
            Console.WriteLine("                  Account, Char Name, IP.");
            Console.WriteLine("bc <message>    - Type this command and your message after it.");
            Console.WriteLine("                  It will then be sent to all players.");
            Console.WriteLine("sc <message>    - Type this command and your message after it.");
            Console.WriteLine("                  It will then be sent to all staff.");
            Console.WriteLine("hear            - Copies all local speech to this console:");
            Console.WriteLine("                  Char Name (Region name): Speech.");
            Console.WriteLine("ban <name>      - Kicks and bans the users account.");
            Console.WriteLine("kick <name>     - Kicks the user.");
            Console.WriteLine("help|?          - Shows this list.");
            Console.WriteLine(" ");
        }

        public static void BroadcastMessage(AccessLevel ac, int hue, string message)
        {
            World.Broadcast(hue, false, ac, message);
        }
    }
}
