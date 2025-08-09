using Server.Network.Packets;

namespace Server.Engines.Chat
{
    public class ChatSystem
    {
        public static readonly bool Enabled = true; // Turn ON and OFF here.

        public static readonly bool AllowCreateChannels = false;

        public const string DefaultChannel = "Global";

        public const long ChatDelay = 5000;

        public static void SendCommandTo(Mobile to, ChatCommand type, string param1 = null, string param2 = null)
        {
            if (to != null)
            {
                to.Send(new ChatMessagePacket(null, (int)type + 20, param1, param2));
            }
        }
    }
}
