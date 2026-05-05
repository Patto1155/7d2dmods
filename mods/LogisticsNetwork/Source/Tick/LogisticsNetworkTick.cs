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
                Log.Out(graphs[i].ToSummaryString(i + 1));
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
                sb.Append(graph.Origin);
                sb.Append(':');
                sb.Append(graph.ConduitCount);
                sb.Append(',');
                sb.Append(graph.StorageCount);
                sb.Append(',');
                sb.Append(graph.WorkstationCount);
            }

            return sb.ToString();
        }
    }
}
