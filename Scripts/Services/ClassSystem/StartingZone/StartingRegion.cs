namespace Server.Regions
{
    public class CathedralRegion : BaseRegion
    {
        public static void Initialize()
        {
            _ = new CathedralRegion();
        }

        public CathedralRegion()
            : base("The Avatar's Cathedral", Map.Malas, DefaultPriority, new Rectangle2D(64, 204, 99, 37))
        {
            GoLocation = new Point3D(79, 223, -1);

            Register();
        }

        public override bool CanUseStuckMenu(Mobile m)
        {
            return false;
        }
    }
}
