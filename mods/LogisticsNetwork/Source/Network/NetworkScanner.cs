using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LogisticsNetwork.Blocks;

namespace LogisticsNetwork.Network
{
    public static class NetworkScanner
    {
        private const int DefaultMaxDepth = 64;
        private const int BootstrapThrottleMs = 3000;
        private const int BootstrapSlowThrottleMs = 30000;
        private const int BootstrapSlowThreshold = 8;

        private static readonly Vector3i[] Directions = new Vector3i[]
        {
            new Vector3i( 1, 0, 0),
            new Vector3i(-1, 0, 0),
            new Vector3i( 0, 1, 0),
            new Vector3i( 0,-1, 0),
            new Vector3i( 0, 0, 1),
            new Vector3i( 0, 0,-1),
        };

        private static World lastBootstrapWorld;
        private static int lastBootstrapTick;
        private static int consecutiveEmptyBootstraps;

        private struct SearchNode
        {
            public SearchNode(Vector3i position, int depth)
            {
                Position = position;
                Depth = depth;
            }

            public Vector3i Position { get; }
            public int Depth { get; }
        }

        public static List<NetworkGraph> ScanAll(World world, int maxDepth = DefaultMaxDepth)
        {
            List<NetworkGraph> graphs = new List<NetworkGraph>();
            if (world == null)
                return graphs;

            if (!ReferenceEquals(world, lastBootstrapWorld))
            {
                lastBootstrapWorld = world;
                lastBootstrapTick = 0;
                consecutiveEmptyBootstraps = 0;
            }

            NetworkRegistry.PruneStaleEntries(world);

            HashSet<Vector3i> assignedNodes = new HashSet<Vector3i>();
            HashSet<Vector3i> seedSet = BuildSeedSet(world);

            List<Vector3i> seeds = new List<Vector3i>(seedSet);
            seeds.Sort(CompareVector3i);

            for (int i = 0; i < seeds.Count; i++)
            {
                Vector3i seed = seeds[i];
                if (assignedNodes.Contains(seed))
                    continue;

                if (!IsLogisticsNetworkBlock(world, seed))
                    continue;

                NetworkGraph graph = ScanFromOrigin(world, seed, maxDepth);
                if (graph.IsEmpty)
                    continue;

                foreach (Vector3i conduit in graph.Conduits)
                    assignedNodes.Add(conduit);

                foreach (Vector3i connector in graph.Connectors)
                    assignedNodes.Add(connector);

                graphs.Add(graph);
            }

            return graphs;
        }

        private static HashSet<Vector3i> BuildSeedSet(World world)
        {
            HashSet<Vector3i> seedSet = new HashSet<Vector3i>();

            foreach (Vector3i position in NetworkRegistry.GetConduitPositions())
                seedSet.Add(position);

            foreach (Vector3i position in NetworkRegistry.GetConnectorPositions())
                seedSet.Add(position);

            if (seedSet.Count == 0 && ShouldAttemptBootstrap())
            {
                lastBootstrapTick = Environment.TickCount;
                int found = 0;

                foreach (Vector3i bootstrapSeed in FindBootstrapSeeds(world))
                {
                    if (seedSet.Add(bootstrapSeed))
                        found++;
                }

                if (found == 0)
                    consecutiveEmptyBootstraps++;
                else
                    consecutiveEmptyBootstraps = 0;
            }

            return seedSet;
        }

        private static bool ShouldAttemptBootstrap()
        {
            if (consecutiveEmptyBootstraps >= BootstrapSlowThreshold)
            {
                return ElapsedSince(lastBootstrapTick) >= BootstrapSlowThrottleMs;
            }

            return lastBootstrapTick == 0 || ElapsedSince(lastBootstrapTick) >= BootstrapThrottleMs;
        }

        private static int ElapsedSince(int previousTick)
        {
            return unchecked(Environment.TickCount - previousTick);
        }

        public static void ResetBootstrapState()
        {
            lastBootstrapWorld = null;
            lastBootstrapTick = 0;
            consecutiveEmptyBootstraps = 0;
        }

        private static IEnumerable<Vector3i> FindBootstrapSeeds(World world)
        {
            HashSet<Vector3i> seeds = new HashSet<Vector3i>();

            foreach (TileEntity tileEntity in EnumerateTileEntities(world))
            {
                Vector3i tileEntityPosition;
                if (!TryGetTileEntityPosition(tileEntity, out tileEntityPosition))
                    continue;

                for (int i = 0; i < Directions.Length; i++)
                {
                    Vector3i neighbor = tileEntityPosition + Directions[i];
                    if (!IsLogisticsNetworkBlock(world, neighbor))
                        continue;

                    if (seeds.Add(neighbor))
                        RegisterBlockFromWorld(world, neighbor);
                }
            }

            return seeds;
        }

        private static void RegisterBlockFromWorld(World world, Vector3i position)
        {
            Block block = world.GetBlock(position).Block;
            if (block is LogisticsConduitBlock)
            {
                NetworkRegistry.RegisterConduit(position);
                return;
            }

            if (block is LogisticsConnectorBlock || block is LogisticsImporterBlock || block is LogisticsExporterBlock || block is LogisticsFilterBlock)
            {
                NetworkRegistry.RegisterConnector(position);
            }
        }

        private static IEnumerable<TileEntity> EnumerateTileEntities(World world)
        {
            if (world == null)
                yield break;

            Type worldType = world.GetType();
            MethodInfo[] methods = worldType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (!string.Equals(method.Name, "GetTileEntities", StringComparison.Ordinal))
                    continue;

                if (method.GetParameters().Length != 0)
                    continue;

                object result;
                try
                {
                    result = method.Invoke(world, null);
                }
                catch
                {
                    yield break;
                }

                foreach (TileEntity tileEntity in EnumerateTileEntities(result))
                    yield return tileEntity;

                yield break;
            }
        }

        private static IEnumerable<TileEntity> EnumerateTileEntities(object value)
        {
            if (!(value is IEnumerable enumerable))
                yield break;

            foreach (object entry in enumerable)
            {
                if (entry is TileEntity tileEntity)
                {
                    yield return tileEntity;
                    continue;
                }

                if (entry == null)
                    continue;

                Type entryType = entry.GetType();
                PropertyInfo valueProperty = entryType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
                if (valueProperty != null && typeof(TileEntity).IsAssignableFrom(valueProperty.PropertyType))
                {
                    TileEntity nested = valueProperty.GetValue(entry, null) as TileEntity;
                    if (nested != null)
                        yield return nested;
                }
            }
        }

        private static int CompareVector3i(Vector3i left, Vector3i right)
        {
            int result = left.x.CompareTo(right.x);
            if (result != 0)
                return result;

            result = left.y.CompareTo(right.y);
            if (result != 0)
                return result;

            return left.z.CompareTo(right.z);
        }

        private static bool TryGetTileEntityPosition(TileEntity tileEntity, out Vector3i position)
        {
            position = default(Vector3i);
            if (tileEntity == null)
                return false;

            Type tileEntityType = tileEntity.GetType();
            string[] memberNames = { "BlockPosition", "blockPos", "blockPosition", "Position", "pos" };

            for (int i = 0; i < memberNames.Length; i++)
            {
                string memberName = memberNames[i];

                PropertyInfo property = tileEntityType.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property != null && property.PropertyType == typeof(Vector3i))
                {
                    object value = property.GetValue(tileEntity, null);
                    if (value is Vector3i)
                    {
                        position = (Vector3i)value;
                        return true;
                    }
                }

                FieldInfo field = tileEntityType.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null && field.FieldType == typeof(Vector3i))
                {
                    object value = field.GetValue(tileEntity);
                    if (value is Vector3i)
                    {
                        position = (Vector3i)value;
                        return true;
                    }
                }
            }

            return false;
        }

        public static NetworkGraph ScanFromOrigin(World world, Vector3i origin, int maxDepth = 64)
        {
            NetworkGraph graph = new NetworkGraph(origin);
            if (world == null)
                return graph;

            HashSet<Vector3i> visited = new HashSet<Vector3i>();
            Queue<SearchNode> queue = new Queue<SearchNode>();

            visited.Add(origin);
            queue.Enqueue(new SearchNode(origin, 0));

            AddLogisticsBlockToGraph(world, origin, graph);

            while (queue.Count > 0)
            {
                SearchNode current = queue.Dequeue();
                if (current.Depth >= maxDepth)
                {
                    graph.TruncatedByDepthLimit = true;
                    continue;
                }

                int nextDepth = current.Depth + 1;

                for (int i = 0; i < Directions.Length; i++)
                {
                    Vector3i neighbor = current.Position + Directions[i];
                    if (visited.Contains(neighbor))
                        continue;

                    visited.Add(neighbor);

                    if (IsLogisticsNetworkBlock(world, neighbor))
                    {
                        AddLogisticsBlockToGraph(world, neighbor, graph);
                        queue.Enqueue(new SearchNode(neighbor, nextDepth));
                        continue;
                    }

                    NetworkEndpointKind endpointKind;
                    if (TryGetEndpointKind(world, neighbor, out endpointKind))
                    {
                        graph.AddEndpoint(endpointKind, neighbor);
                    }
                }
            }

            return graph;
        }

        private static void AddLogisticsBlockToGraph(World world, Vector3i position, NetworkGraph graph)
        {
            Block block = world.GetBlock(position).Block;
            if (block is LogisticsConduitBlock)
            {
                graph.AddConduit(position);
                return;
            }

            if (block is LogisticsConnectorBlock || block is LogisticsImporterBlock || block is LogisticsExporterBlock || block is LogisticsFilterBlock)
            {
                graph.AddConnector(position);
            }
        }

        private static bool IsLogisticsNetworkBlock(World world, Vector3i position)
        {
            if (NetworkRegistry.IsConduitRegistered(position) || NetworkRegistry.IsConnectorRegistered(position))
                return true;

            Block block = world.GetBlock(position).Block;
            return block is LogisticsConduitBlock ||
                   block is LogisticsConnectorBlock ||
                   block is LogisticsImporterBlock ||
                   block is LogisticsExporterBlock ||
                   block is LogisticsFilterBlock;
        }

        private static bool TryGetEndpointKind(World world, Vector3i position, out NetworkEndpointKind kind)
        {
            kind = NetworkEndpointKind.Storage;

            TileEntity tileEntity = world.GetTileEntity(0, position);
            if (tileEntity is TileEntityLootContainer)
            {
                kind = NetworkEndpointKind.Storage;
                return true;
            }

            if (tileEntity is TileEntityWorkstation)
            {
                kind = NetworkEndpointKind.Workstation;
                return true;
            }

            return false;
        }
    }
}
