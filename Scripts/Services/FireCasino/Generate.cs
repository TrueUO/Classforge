using Server.Commands;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.ResortAndCasino
{
    public static class FireCasinoGenerator
    {
        public static readonly string EntityName = "casino";

        public static void Initialize()
        {
            CommandSystem.Register("GenerateCasino", AccessLevel.Administrator, Generate);
            CommandSystem.Register("DeleteCasino", AccessLevel.GameMaster, Delete);
        }

        public static void Generate(CommandEventArgs e)
        {
            Delete(e);

            Point3D[] list = GetTramPoints();
            Map map = Map.Trammel;

            Item item = new tent_whiteAddon();
            WeakEntityCollection.Add(EntityName, item);
            item.MoveToWorld(list[0], map);

            item = new tent_brownAddon();
            WeakEntityCollection.Add(EntityName, item);
            item.MoveToWorld(list[1], map);

            Mobile mob = new CasinoCashier();
            WeakEntityCollection.Add(EntityName, mob);
            mob.MoveToWorld(list[2], map);

            mob = new CasinoCashier();
            WeakEntityCollection.Add(EntityName, mob);
            mob.MoveToWorld(list[3], map);

            mob = new CasinoCashier();
            WeakEntityCollection.Add(EntityName, mob);
            mob.MoveToWorld(list[4], map);

            mob = new ChucklesLuckDealer();
            WeakEntityCollection.Add(EntityName, mob);
            mob.MoveToWorld(list[5], map);

            mob = new HiMiddleLowDealer();
            WeakEntityCollection.Add(EntityName, mob);
            mob.MoveToWorld(list[6], map);

            mob = new DiceRiderDealer();
            WeakEntityCollection.Add(EntityName, mob);
            mob.MoveToWorld(list[7], map);

            XmlSpawner xmlspawner = new XmlSpawner(8, 1, 2, 0, 25, "CasinoWaitress");
            WeakEntityCollection.Add(EntityName, xmlspawner);
            xmlspawner.MoveToWorld(list[8], map);
            xmlspawner.MaxCount = 8;
            xmlspawner.SpawnRange = 25;
            xmlspawner.SpawnRange = 25;
            xmlspawner.DoRespawn = true;

            e.Mobile.SendMessage("Fortune Fire Casino Generated in {0}!", map);
        }

        private static Point3D[] GetTramPoints()
        {
            return new Point3D[]
            {
                new Point3D(4062, 3313, 1),
                new Point3D(4050, 3332, 0),

                new Point3D(4047, 3329, 0),
                new Point3D(4049, 3329, 0),
                new Point3D(4049, 3327, 0),

                new Point3D(4060, 3313, 4),
                new Point3D(4062, 3310, 4),
                new Point3D(4065, 3312, 4),

                new Point3D(4055, 3319, 0)
            };
        }

        public static void Delete(CommandEventArgs e)
        {
            WeakEntityCollection.Delete(EntityName);
        }
    }
}
