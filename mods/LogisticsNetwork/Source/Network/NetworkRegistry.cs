using System.Collections.Generic;
using System.Linq;
using LogisticsNetwork.Blocks;

namespace LogisticsNetwork.Network
{
    public static class NetworkRegistry
    {
        private static readonly object SyncRoot = new object();
        private static readonly HashSet<Vector3i> ConduitPositions = new HashSet<Vector3i>();
        private static readonly HashSet<Vector3i> ConnectorPositions = new HashSet<Vector3i>();

        public static void RegisterConduit(Vector3i position)
        {
            lock (SyncRoot)
            {
                ConduitPositions.Add(position);
            }
        }

        public static void UnregisterConduit(Vector3i position)
        {
            lock (SyncRoot)
            {
                ConduitPositions.Remove(position);
            }
        }

        public static void RegisterConnector(Vector3i position)
        {
            lock (SyncRoot)
            {
                ConnectorPositions.Add(position);
            }
        }

        public static void UnregisterConnector(Vector3i position)
        {
            lock (SyncRoot)
            {
                ConnectorPositions.Remove(position);
            }
        }

        public static List<Vector3i> GetConduitPositions()
        {
            lock (SyncRoot)
            {
                return ConduitPositions.ToList();
            }
        }

        public static List<Vector3i> GetConnectorPositions()
        {
            lock (SyncRoot)
            {
                return ConnectorPositions.ToList();
            }
        }

        public static bool IsConduitRegistered(Vector3i position)
        {
            lock (SyncRoot)
            {
                return ConduitPositions.Contains(position);
            }
        }

        public static bool IsConnectorRegistered(Vector3i position)
        {
            lock (SyncRoot)
            {
                return ConnectorPositions.Contains(position);
            }
        }

        public static void PruneStaleEntries(World world)
        {
            if (world == null)
                return;

            List<Vector3i> staleConduits = new List<Vector3i>();
            List<Vector3i> staleConnectors = new List<Vector3i>();

            lock (SyncRoot)
            {
                foreach (Vector3i position in ConduitPositions)
                {
                    if (!IsConduitAtWorldPosition(world, position))
                        staleConduits.Add(position);
                }

                foreach (Vector3i position in ConnectorPositions)
                {
                    if (!IsConnectorAtWorldPosition(world, position))
                        staleConnectors.Add(position);
                }

                for (int i = 0; i < staleConduits.Count; i++)
                    ConduitPositions.Remove(staleConduits[i]);

                for (int i = 0; i < staleConnectors.Count; i++)
                    ConnectorPositions.Remove(staleConnectors[i]);
            }
        }

        private static bool IsConduitAtWorldPosition(World world, Vector3i position)
        {
            Block block = world.GetBlock(position).Block;
            return block is LogisticsConduitBlock;
        }

        private static bool IsConnectorAtWorldPosition(World world, Vector3i position)
        {
            Block block = world.GetBlock(position).Block;
            return block is LogisticsConnectorBlock;
        }

        public static void Clear()
        {
            lock (SyncRoot)
            {
                ConduitPositions.Clear();
                ConnectorPositions.Clear();
            }
        }
    }
}
