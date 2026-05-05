using System.Collections.Generic;
using System.Linq;

namespace LogisticsNetwork.Network
{
    public static class NetworkRegistry
    {
        private static readonly object SyncRoot = new object();
        private static readonly HashSet<Vector3i> ConduitPositions = new HashSet<Vector3i>();

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

        public static List<Vector3i> GetConduitPositions()
        {
            lock (SyncRoot)
            {
                return ConduitPositions.ToList();
            }
        }

        public static bool IsConduitRegistered(Vector3i position)
        {
            lock (SyncRoot)
            {
                return ConduitPositions.Contains(position);
            }
        }

        public static void Clear()
        {
            lock (SyncRoot)
            {
                ConduitPositions.Clear();
            }
        }
    }
}
