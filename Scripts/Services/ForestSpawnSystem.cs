using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Server.Commands;
using Server.Mobiles;
using Server.Regions;

namespace Server.ForestSpawn
{
    public sealed class ForestSpawnSystem
    {
        // on startup load system
        public static void Initialize()
        {
            if (_IsEnabled)
            {
                LoadSystem();
            }
        }

        // System Controls
        private static bool _IsEnabled = false; // Turn ON and OFF here.

        private static Map ForestSpawnMap => Map.Trammel;

        private const int _ScanStep = 2;        // scan and check every 2 tiles
        private const int _MinSpacing = 4;      // minimum tile spacing between accepted points
        private const int _MaxPoints = 150000; // absolute safety cap (150,000)

        // Stratification bucket size (tiles). Bigger bucket = fewer, broader buckets.
        // 128x128 works well for even coverage without being too fine.
        private const int _BucketSize = 128;

        private static Timer _MaintenanceTimer;

        // Check world spawns and levels every 10 seconds.
        private static readonly TimeSpan _MaintenanceTick = TimeSpan.FromSeconds(10); 

        private static readonly string _CachePath = Path.Combine(Core.BaseDirectory, "Saves", "ForestSpawns.trammel.bin");

        private static readonly Lock _Sync = new();

        private static List<Point3D> _ForestPoints = new(64000); 

        // Live tracking per animal type
        private static readonly Dictionary<Type, HashSet<BaseCreature>> _Live = new();
        private static readonly Dictionary<Type, int> _DesiredCounts = new();

        private static readonly AnimalToSpawn[] _AnimalsToSpawn =
        [
            new(typeof(Rabbit), maxToSpawn: 900, homeRange: 8),
            new(typeof(Bird), maxToSpawn: 900, homeRange: 8),
            new(typeof(Hind), maxToSpawn: 800, homeRange: 8),
            new(typeof(GreatHart), maxToSpawn: 700, homeRange: 8),
            new(typeof(TimberWolf),  maxToSpawn: 240, homeRange: 8),
            new(typeof(BlackBear),  maxToSpawn: 180, homeRange: 8),
            new(typeof(GrizzlyBear),  maxToSpawn: 120, homeRange: 8)
        ];

        public static void LoadSystem()
        {
            RegisterCommands();

            if (!LoadCache())
            {
                Console.WriteLine("[ForestSpawn] Cache missing or invalid; scanning Trammel for forest tiles...");
                ScanMapForForestPoints();
                SaveCache();
            }
            else
            {
                Console.WriteLine($"[ForestSpawn] Loaded cached forest spawn points: {_ForestPoints.Count}");
            }

            ComputeDesiredCounts();
            PrimePopulation();

            _MaintenanceTimer = new MaintenanceTimer();
            _MaintenanceTimer.Start();
        }

        private static void RegisterCommands()
        {
            CommandSystem.Register("ForestRescan", AccessLevel.Administrator, e =>
            {
                e.Mobile.SendMessage("Rescanning Trammel for forest points, please wait...");

                ScanMapForForestPoints();
                SaveCache();
                ComputeDesiredCounts();

                e.Mobile.SendMessage($"Forest scan complete. Points: {_ForestPoints.Count}");
            });

            CommandSystem.Register("ForestSpawnStats", AccessLevel.Administrator, e =>
            {
                e.Mobile.SendMessage($"Forest points: {_ForestPoints.Count}");

                foreach (AnimalToSpawn def in _AnimalsToSpawn)
                {
                    _Live.TryGetValue(def.Type, out HashSet<BaseCreature> set);

                    int live = set?.Count ?? 0;
                    int want = _DesiredCounts.GetValueOrDefault(def.Type, 0);

                    e.Mobile.SendMessage($"{def.Type.Name}: {live}/{want} live");
                }
            });

            CommandSystem.Register("ForestDisable", AccessLevel.Administrator, e =>
            {
                _IsEnabled = false;

                e.Mobile.SendMessage("Forest spawns disabled (existing mobs remain).");
            });

            CommandSystem.Register("ForestEnable", AccessLevel.Administrator, e =>
            {
                _IsEnabled = true;

                e.Mobile.SendMessage("Forest spawns enabled.");
            });
        }

        // Scanning
        private static void ScanMapForForestPoints()
        {
            lock (_Sync)
            {
                _ForestPoints.Clear();

                Map map = ForestSpawnMap;
                if (map == null || map.Tiles == null)
                {
                    Console.WriteLine("[ForestSpawn] Trammel map not available.");
                    return;
                }

                // Trammel overland only (avoid Lost Lands/dungeons)
                int mapWidth = 5120;
                int mapHeight = 4096;

                List<Point3D> candidates = new List<Point3D>(128_000);
                int examined = 0;

                for (int x = 0; x < mapWidth; x += _ScanStep)
                {
                    for (int y = 0; y < mapHeight; y += _ScanStep)
                    {
                        examined++;

                        if (!IsForestTile(map, x, y))
                        {
                            continue;
                        }

                        int z = map.GetAverageZ(x, y);

                        if (!map.CanFit(x, y, z, 16, false, false))
                        {
                            continue;
                        }

                        if (!IsAllowedRegion(new Point3D(x, y, z), map))
                        {
                            continue;
                        }

                        candidates.Add(new Point3D(x, y, z));
                    }
                }

                // Enforce true minimum spacing everywhere (blue-noise)
                _ForestPoints = BlueNoiseSelect(candidates, _MinSpacing, mapWidth, mapHeight);

                Console.WriteLine($"[ForestSpawn] Examined ~{examined:N0}; blue-noise selected {_ForestPoints.Count:N0} points before thinning.");

                // Keep geographic fairness if we still exceed the cap
                if (_ForestPoints.Count > _MaxPoints)
                {
                    Console.WriteLine($"[ForestSpawn] Thinning {_ForestPoints.Count:N0} → cap {_MaxPoints:N0} with stratified sampling...");
                    ThinToCapEvenly(mapWidth, mapHeight, _MaxPoints);
                    Console.WriteLine($"[ForestSpawn] Final spawn points: {_ForestPoints.Count:N0}");
                }
            }
        }

        private static List<Point3D> BlueNoiseSelect(List<Point3D> candidates, int minDist, int mapWidth, int mapHeight)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return new List<Point3D>();
            }

            // Shuffle to remove scan-order bias (Fisher–Yates)
            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = Utility.Random(i + 1);
                Point3D tmp = candidates[i];
                candidates[i] = candidates[j];
                candidates[j] = tmp;
            }

            int cell = Math.Max(1, minDist);
            int cellsX = (mapWidth + cell - 1) / cell;
            int cellsY = (mapHeight + cell - 1) / cell;

            // Spatial hash of selected points (by coarse cell)
            Dictionary<int, List<Point3D>> grid = new Dictionary<int, List<Point3D>>(cellsX * cellsY / 4);
            List<Point3D> selected = new List<Point3D>(candidates.Count / 2);
            int minDistSq = minDist * minDist;

            foreach (Point3D p in candidates)
            {
                int cx = p.X / cell;
                int cy = p.Y / cell;

                bool ok = true;

                // Check 3x3 neighbor cells
                for (int ny = cy - 1; ny <= cy + 1 && ok; ny++)
                {
                    if (ny < 0 || ny >= cellsY)
                    {
                        continue;
                    }

                    for (int nx = cx - 1; nx <= cx + 1 && ok; nx++)
                    {
                        if (nx < 0 || nx >= cellsX)
                        {
                            continue;
                        }

                        int key = (ny << 16) ^ nx;
                        if (!grid.TryGetValue(key, out List<Point3D> list) || list == null)
                        {
                            continue;
                        }

                        for (int k = 0; k < list.Count; k++)
                        {
                            Point3D q = list[k];
                            int dx = q.X - p.X;
                            int dy = q.Y - p.Y;
                            if ((dx * dx + dy * dy) < minDistSq)
                            {
                                ok = false;
                                break;
                            }
                        }
                    }
                }

                if (!ok)
                {
                    continue;
                }

                // Accept this point
                selected.Add(p);

                int selfKey = (cy << 16) ^ cx;
                if (!grid.TryGetValue(selfKey, out List<Point3D> selfList))
                {
                    selfList = new List<Point3D>(4);
                    grid[selfKey] = selfList;
                }
                selfList.Add(p);
            }

            return selected;
        }

        // Evenly thin _ForestPoints down to 'cap' by allocating per-bucket quotas
        // using Hamilton's (largest remainder) method, then sampling uniformly inside buckets.
        // Final count will be <= cap (exactly cap when enough points exist).
        private static void ThinToCapEvenly(int mapWidth, int mapHeight, int cap)
        {
            if (_ForestPoints.Count <= cap)
            {
                return;
            }

            int bucketsX = (mapWidth + _BucketSize - 1) / _BucketSize;
            int bucketsY = (mapHeight + _BucketSize - 1) / _BucketSize;

            // Group points into buckets
            Dictionary<int, List<Point3D>> buckets = new Dictionary<int, List<Point3D>>(bucketsX * bucketsY);
            foreach (Point3D p in _ForestPoints)
            {
                int bx = p.X / _BucketSize;
                int by = p.Y / _BucketSize;
                int key = by * bucketsX + bx;
                if (!buckets.TryGetValue(key, out List<Point3D> list))
                {
                    list = new List<Point3D>();
                    buckets[key] = list;
                }
                list.Add(p);
            }

            // Build indexable arrays for quotas/remainders
            List<int> keys = new List<int>(buckets.Count);
            foreach (KeyValuePair<int, List<Point3D>> kv in buckets)
                if (kv.Value.Count > 0)
                {
                    keys.Add(kv.Key);
                }

            int n = keys.Count;
            if (n == 0)
            {
                _ForestPoints.Clear();
                return;
            }

            int[] quotas = new int[n];
            double[] remainder = new double[n];

            int total = _ForestPoints.Count;
            int sum = 0;

            // 1) Floor quotas + remainders
            for (int i = 0; i < n; i++)
            {
                int cnt = buckets[keys[i]].Count;
                double share = (double)cnt * cap / total;
                int q = (int)Math.Floor(share);

                quotas[i] = Math.Min(q, cnt); // never over-ask a bucket
                remainder[i] = share - q;

                sum += quotas[i];
            }

            int remaining = cap - sum;

            // 2) Distribute remaining seats to largest remainders
            if (remaining > 0)
            {
                List<int> order = new List<int>(n);
                for (int i = 0; i < n; i++)
                {
                    order.Add(i);
                }

                order.Sort((a, b) => remainder[b].CompareTo(remainder[a])); // largest remainder first

                int idx = 0;
                while (remaining > 0 && idx < order.Count)
                {
                    int i = order[idx++];
                    int cnt = buckets[keys[i]].Count;
                    if (quotas[i] < cnt)
                    {
                        quotas[i]++;
                        remaining--;
                    }
                }

                // If some buckets were full, randomly top up from any with spare
                int safety = 0;
                while (remaining > 0 && safety < n * 4)
                {
                    int i = Utility.Random(n);
                    int cnt = buckets[keys[i]].Count;
                    if (quotas[i] < cnt)
                    {
                        quotas[i]++;
                        remaining--;
                    }
                    safety++;
                }
            }
            else if (remaining < 0)
            {
                // (Very rare with flooring - keep for robustness)
                int needRemove = -remaining;
                List<int> order = new List<int>(n);

                for (int i = 0; i < n; i++)
                {
                    order.Add(i);
                }

                order.Sort((a, b) => remainder[a].CompareTo(remainder[b])); // smallest remainder first

                int idx = 0;
                while (needRemove > 0 && idx < order.Count)
                {
                    int i = order[idx++];
                    if (quotas[i] > 0)
                    {
                        quotas[i]--;
                        needRemove--;
                    }
                }
            }

            // 3) Sample from buckets according to quotas
            List<Point3D> result = new List<Point3D>(cap);
            for (int i = 0; i < n; i++)
            {
                List<Point3D> list = buckets[keys[i]];
                int take = Math.Min(quotas[i], list.Count);
                if (take > 0)
                {
                    SampleWithoutReplacement(list, take, result);
                }
            }

            // Guarantee we don't exceed cap even if something weird happened
            if (result.Count > cap)
            {
                result.RemoveRange(cap, result.Count - cap);
            }

            _ForestPoints = result;
        }

        // Fisher–Yates partial shuffle to get 'take' random, unique items from 'src'
        private static void SampleWithoutReplacement(List<Point3D> src, int take, List<Point3D> dst)
        {
            if (take >= src.Count)
            {
                dst.AddRange(src);
                src.Clear();
                return;
            }

            for (int i = 0; i < take; i++)
            {
                int j = i + Utility.Random(src.Count - i);
                // swap src[i] and src[j]
                Point3D tmp = src[i];
                src[i] = src[j];
                src[j] = tmp;

                dst.Add(src[i]);
            }

            // Remove the first 'take' items we just consumed (keeps leftover list small)
            src.RemoveRange(0, take);
        }

        private static bool IsForestTile(Map map, int x, int y)
        {
            LandTile lt = map.Tiles.GetLandTile(x, y);
            int id = lt.ID & 0x3FFF;
            string name = null;

            try
            {
                name = TileData.LandTable[id].Name;

            }
            catch
            {
                /* ignore malformed id */ 
            }

            return !string.IsNullOrEmpty(name) && name.IndexOf("forest", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsAllowedRegion(Point3D p, Map m)
        {
            Region r = Region.Find(p, m);
            if (r == null)
            {
                return true;
            }

            if (r.IsPartOf(typeof(GuardedRegion)) || r.IsPartOf(typeof(HouseRegion)))
            {
                return false;
            }

            return true;
        }

        // Caching
        private static bool LoadCache()
        {
            try
            {
                if (!File.Exists(_CachePath))
                {
                    return false;
                }

                using (FileStream fs = new(_CachePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader br = new(fs))
                {
                    int magic = br.ReadInt32();
                    int version = br.ReadInt32();

                    if (magic != 0xF05E57 || version != 1)
                    {
                        return false;
                    }

                    int mapW = br.ReadInt32();
                    int mapH = br.ReadInt32();

                    if (ForestSpawnMap.Tiles == null || ForestSpawnMap.Width != mapW || ForestSpawnMap.Height != mapH)
                    {
                        return false;
                    }

                    int count = br.ReadInt32();
                    _ForestPoints = new(count);

                    for (int i = 0; i < count; i++)
                    {
                        int x = br.ReadInt32();
                        int y = br.ReadInt32();
                        int z = br.ReadInt32();
                        _ForestPoints.Add(new(x, y, z));
                    }
                }

                return _ForestPoints.Count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ForestSpawn] LoadCache failed: {0}", ex);
                return false;
            }
        }

        public static void SaveCache()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_CachePath));

                using (FileStream fs = new(_CachePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (BinaryWriter bw = new(fs))
                {
                    bw.Write(0xF05E57); // magic
                    bw.Write(1);         // version
                    bw.Write(ForestSpawnMap?.Width ?? 0);
                    bw.Write(ForestSpawnMap?.Height ?? 0);

                    bw.Write(_ForestPoints.Count);
                    foreach (Point3D p in _ForestPoints)
                    {
                        bw.Write(p.X);
                        bw.Write(p.Y);
                        bw.Write(p.Z);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ForestSpawn] SaveCache failed: {0}", ex);
            }
        }

        // Population Management
        private static void ComputeDesiredCounts()
        {
            _DesiredCounts.Clear();

            foreach (AnimalToSpawn def in _AnimalsToSpawn)
            {
                _DesiredCounts[def.Type] = def.MaxToSpawn;

                if (!_Live.ContainsKey(def.Type))
                {
                    _Live[def.Type] = [];
                }
            }
        }

        private static void PrimePopulation()
        {
            if (!_IsEnabled || _ForestPoints.Count == 0)
            {
                return;
            }

            foreach (AnimalToSpawn def in _AnimalsToSpawn)
            {
                int desired = _DesiredCounts[def.Type];
                int have = _Live[def.Type].Count;
                int need = Math.Max(0, desired - have);

                for (int i = 0; i < need; i++)
                {
                    Point3D p = _ForestPoints[Utility.Random(_ForestPoints.Count)];
                    TrySpawn(def, p);
                }
            }
        }

        private static bool TrySpawn(AnimalToSpawn def, Point3D basePoint)
        {
            if (!_IsEnabled)
            {
                return false;
            }

            Map map = ForestSpawnMap;

            int dx = Utility.RandomMinMax(-def.HomeRange, def.HomeRange);
            int dy = Utility.RandomMinMax(-def.HomeRange, def.HomeRange);
            int x = basePoint.X + dx;
            int y = basePoint.Y + dy;
            int z = map.GetAverageZ(x, y);

            if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
            {
                return false;
            }

            if (!map.CanFit(x, y, z, 16, false, false))
            {
                return false;
            }

            if (!IsAllowedRegion(new(x, y, z), map))
            {
                return false;
            }

            try
            {
                if (Activator.CreateInstance(def.Type) is not BaseCreature mob)
                {
                    return false;
                }

                mob.MoveToWorld(new(x, y, z), map);
                mob.Home = basePoint;
                mob.RangeHome = Math.Max(4, def.HomeRange);

                RegisterMobile(def.Type, mob);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ForestSpawn] Spawn failed for {def.Type.Name}: {ex.Message}");
                return false;
            }
        }

        private static void RegisterMobile(Type t, BaseCreature creature)
        {
            HashSet<BaseCreature> set = _Live[t];

            set.Add(creature);
        }

        private class MaintenanceTimer()
            : Timer(TimeSpan.FromSeconds(1.0), _MaintenanceTick)
        {
            protected override void OnTick()
            {
                if (!_IsEnabled || _ForestPoints.Count == 0)
                {
                    return;
                }

                // Prune deleted/dead mobs since we don't have direct hooks
                foreach (KeyValuePair<Type, HashSet<BaseCreature>> kv in _Live)
                {
                    HashSet<BaseCreature> set = kv.Value;
                    if (set == null)
                    {
                        continue;
                    }

                    // Remove null, deleted, or internal-map/tamed entries
                    set.RemoveWhere(m => m == null || m.Deleted || m.Controlled || m.Map == Map.Internal);
                }

                // Top up each species a little each tick, not all at once
                int perTypeCap = 20;

                foreach (AnimalToSpawn def in _AnimalsToSpawn)
                {
                    _Live.TryGetValue(def.Type, out HashSet<BaseCreature> set);

                    int have = set?.Count ?? 0;

                    int want = _DesiredCounts.GetValueOrDefault(def.Type, 0);

                    int deficit = want - have;
                    if (deficit <= 0)
                    {
                        continue;
                    }

                    int toSpawn = Math.Min(perTypeCap, deficit);

                    for (int i = 0; i < toSpawn; i++)
                    {
                        Point3D p = _ForestPoints[Utility.Random(_ForestPoints.Count)];

                        if (!TrySpawn(def, p))
                        {
                            break;
                        }
                    }
                }
            }
        }

        private readonly struct AnimalToSpawn(Type type, int maxToSpawn, int homeRange)
        {
            public Type Type { get; } = type;
            public int MaxToSpawn { get; } = Math.Max(0, maxToSpawn);
            public int HomeRange { get; } = Math.Max(0, homeRange);
        }
    }
}
