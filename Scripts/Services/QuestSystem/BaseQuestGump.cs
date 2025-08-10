using System.Collections;
using Server.Gumps;

namespace Server.Engines.Quests
{
    public abstract class BaseQuestGump : Gump
    {
        public const int Black = 0x0000;
        public const int White = 0x7FFF;
        public const int DarkGreen = 10000;
        public const int LightGreen = 90000;
        public const int Blue = 19777215;
        public BaseQuestGump(int x, int y)
            : base(x, y)
        {
        }

        public static int C16232(int c16)
        {
            c16 &= 0x7FFF;

            int r = ((c16 >> 10) & 0x1F) << 3;
            int g = ((c16 >> 05) & 0x1F) << 3;
            int b = ((c16 >> 00) & 0x1F) << 3;

            return (r << 16) | (g << 8) | (b << 0);
        }

        public static int C16216(int c16)
        {
            return c16 & 0x7FFF;
        }

        public static int C32216(int c32)
        {
            c32 &= 0xFFFFFF;

            int r = ((c32 >> 16) & 0xFF) >> 3;
            int g = ((c32 >> 08) & 0xFF) >> 3;
            int b = ((c32 >> 00) & 0xFF) >> 3;

            return (r << 10) | (g << 5) | (b << 0);
        }

        public static string Color(string text, int color)
        {
            return $"<BASEFONT COLOR=#{color:X6}>{text}</BASEFONT>";
        }

        public static ArrayList BuildList(object obj)
        {
            ArrayList list = new ArrayList();

            list.Add(obj);

            return list;
        }

        public void AddHtmlObject(int x, int y, int width, int height, object message, int color, bool back, bool scroll)
        {
            if (message is string stringMessage)
            {
                AddHtml(x, y, width, height, Color(stringMessage, C16232(color)), back, scroll);
            }
            else if (message is int intMessage)
            {
                AddHtmlLocalized(x, y, width, height, intMessage, C16216(color), back, scroll);
            }
        }
    }
}
