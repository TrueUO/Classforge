using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Engines.VoidPool
{
    public class WaveInfo
    {
        public int Wave { get; }
        public bool Cleared { get; set; }
        public List<BaseCreature> Creatures { get; }
        public List<Mobile> Credit { get; }

        public WaveInfo(int index, List<BaseCreature> list)
        {
            Wave = index;
            Creatures = list;

            Credit = new List<Mobile>();
        }
    }
}
