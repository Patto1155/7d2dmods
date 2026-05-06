using System.Collections.Generic;
using System.Text;
using LogisticsNetwork.Network;
using LogisticsNetwork.Util;

namespace LogisticsNetwork.Tick
{
    public static class LogisticsNetworkTick
    {
        private static string lastSnapshot;

        public static void Reset()
        {
            lastSnapshot = null;
            NetworkScanner.ResetBootstrapState();
        }

        public static void RunAll(World world)
        {
            List<NetworkGraph> graphs = NetworkScanner.ScanAll(world);
            if (graphs.Count == 0)
            {
                lastSnapshot = null;
                return;
            }

            string snapshot = BuildSnapshot(graphs);
            if (snapshot == lastSnapshot)
                return;

            lastSnapshot = snapshot;

            for (int i = 0; i < graphs.Count; i++)
            {
                NetworkGraph graph = graphs[i];
                Log.Out(graph.ToSummaryString(i + 1) + " topologyHash=" + TopologyFingerprint(graph));
                LogStorageEndpoints(world, graph, i + 1);
            }
        }

        private static void LogStorageEndpoints(World world, NetworkGraph graph, int graphIndex)
        {
            foreach (Vector3i position in graph.Storage)
            {
                if (!StorageEndpoint.TryDescribe(world, position, out NetworkEndpoint endpoint))
                    continue;

                Log.Out(endpoint.ToLogString(graphIndex));
            }
        }

        private static string BuildSnapshot(List<NetworkGraph> graphs)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < graphs.Count; i++)
            {
                if (i > 0)
                    sb.Append('|');

                NetworkGraph graph = graphs[i];
                sb.Append(graph.Origin.x).Append(',').Append(graph.Origin.y).Append(',').Append(graph.Origin.z);
                sb.Append(':');
                sb.Append(graph.ConduitCount).Append(',').Append(graph.ConnectorCount).Append(',').Append(graph.StorageCount).Append(',').Append(graph.WorkstationCount);
                sb.Append(',');
                sb.Append(graph.TruncatedByDepthLimit ? '1' : '0');
                sb.Append(',');
                sb.Append(TopologyFingerprint(graph));
            }

            return sb.ToString();
        }

        private static int TopologyFingerprint(NetworkGraph graph)
        {
            List<Vector3i> keys = new List<Vector3i>();
            AddSortedKeys(keys, graph.Conduits);
            AddSortedKeys(keys, graph.Connectors);
            AddSortedKeys(keys, graph.Storage);
            AddSortedKeys(keys, graph.Workstations);

            unchecked
            {
                int h = (int)2166136261;
                for (int i = 0; i < keys.Count; i++)
                {
                    Vector3i p = keys[i];
                    h ^= p.x;
                    h *= 16777619;
                    h ^= p.y;
                    h *= 16777619;
                    h ^= p.z;
                    h *= 16777619;
                }

                return h;
            }
        }

        private static void AddSortedKeys(List<Vector3i> keys, IEnumerable<Vector3i> positions)
        {
            foreach (Vector3i position in positions)
                keys.Add(position);

            keys.Sort(CompareVector3i);
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
    }
}
