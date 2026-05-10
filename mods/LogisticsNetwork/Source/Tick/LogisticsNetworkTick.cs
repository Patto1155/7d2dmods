using System.Collections.Generic;
using System.Text;
using LogisticsNetwork.Blocks;
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
            bool topologyChanged = snapshot != lastSnapshot;
            if (topologyChanged)
            {
                lastSnapshot = snapshot;

                for (int i = 0; i < graphs.Count; i++)
                {
                    NetworkGraph graph = graphs[i];
                    int graphIndex = i + 1;
                    Log.Out(graph.ToSummaryString(graphIndex) + " topologyHash=" + TopologyFingerprint(graph));
                    LogConnectorRoleSummary(world, graph, graphIndex);
                    LogStorageEndpoints(world, graph, graphIndex);
                    LogWorkstationEndpoints(world, graph, graphIndex);
                    LogConnectorSnapshots(world, graph, graphIndex);
                    LogRoutingIntents(world, graph, graphIndex);
                }
            }

            if (LogisticsNetworkFeatures.EnableLiveStorageTransfer ||
                LogisticsNetworkFeatures.EnableLiveWorkstationOutputExtraction)
                TryLiveTransfers(world, graphs);
        }

        /// <summary>
        /// Runs every scan tick so item movement can progress without topology changes.
        /// At most one successful move per tick across all graphs and routes.
        /// Each route plan is dispatched to the appropriate transfer service based on attachment kinds:
        /// storage→storage uses <see cref="StorageTransfer"/>; workstation→storage uses
        /// <see cref="WorkstationOutputTransfer"/>. Each path is gated by its own feature flag.
        /// </summary>
        private static void TryLiveTransfers(World world, List<NetworkGraph> graphs)
        {
            if (LogisticsNetworkFeatures.RespectWorldIsRemote && world != null && world.IsRemote())
                return;

            for (int i = 0; i < graphs.Count; i++)
            {
                NetworkGraph graph = graphs[i];
                int graphIndex = i + 1;
                ItemRouteRequest request = new ItemRouteRequest(world, graph, graphIndex);
                ItemRouteReport report = ItemRoutingService.BuildPassiveReport(request);

                if (report.KeepStockTarget > 0)
                    continue;

                foreach (ItemRoutePlan plan in report.Plans)
                {
                    if (plan.SourceAttachmentKind == "storage" && plan.DestinationAttachmentKind == "storage")
                    {
                        if (!LogisticsNetworkFeatures.EnableLiveStorageTransfer)
                            continue;

                        if (StorageTransfer.TryMoveOneStackUnit(world, plan, graphIndex, out string detail))
                        {
                            Log.Out("graph #" + graphIndex + " transfer OK " + detail);
                            return;
                        }

                        continue;
                    }

                    if (plan.SourceAttachmentKind == "workstation" && plan.DestinationAttachmentKind == "storage")
                    {
                        if (!LogisticsNetworkFeatures.EnableLiveWorkstationOutputExtraction)
                            continue;

                        if (WorkstationOutputTransfer.TryMoveOneOutputUnit(world, plan, graphIndex, out string detail))
                        {
                            Log.Out("graph #" + graphIndex + " workstation outputExtract OK " + detail);
                            return;
                        }

                        continue;
                    }
                }
            }
        }

        private static void LogConnectorRoleSummary(World world, NetworkGraph graph, int graphIndex)
        {
            int connectors = 0;
            int importers = 0;
            int exporters = 0;
            int filters = 0;

            foreach (Vector3i position in graph.Connectors)
            {
                Block block = world.GetBlock(position).Block;
                if (block is LogisticsImporterBlock)
                {
                    importers++;
                    continue;
                }

                if (block is LogisticsExporterBlock)
                {
                    exporters++;
                    continue;
                }

                if (block is LogisticsFilterBlock)
                {
                    filters++;
                    continue;
                }

                connectors++;
            }

            Log.Out("[LogisticsNetwork] graph #" + graphIndex +
                    " connectorRoles connector=" + connectors +
                    " importer=" + importers +
                    " exporter=" + exporters +
                    " filter=" + filters);
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

        private static void LogWorkstationEndpoints(World world, NetworkGraph graph, int graphIndex)
        {
            foreach (Vector3i position in graph.Workstations)
            {
                if (!WorkstationEndpoint.TryDescribe(world, position, out NetworkEndpoint endpoint))
                    continue;

                Log.Out(endpoint.ToLogString(graphIndex));
            }
        }

        private static void LogConnectorSnapshots(World world, NetworkGraph graph, int graphIndex)
        {
            foreach (Vector3i position in graph.Connectors)
            {
                if (!NetworkConnectorSnapshot.TryDescribe(world, position, out NetworkConnectorSnapshot snapshot))
                    continue;

                Log.Out(snapshot.ToLogString(graphIndex));
            }
        }

        private static void LogRoutingIntents(World world, NetworkGraph graph, int graphIndex)
        {
            ItemRouteRequest request = new ItemRouteRequest(world, graph, graphIndex);
            ItemRouteReport report = ItemRoutingService.BuildPassiveReport(request);

            Log.Out("[LogisticsNetwork] graph #" + report.GraphIndex +
                    " routes summary=" + report.Summary +
                    " importer=" + report.ImporterCount +
                    " exporter=" + report.ExporterCount +
                    " filter=" + report.FilterCount +
                    " connector=" + report.ConnectorCount +
                    " attachedStorage=" + report.AttachedStorage +
                    " attachedWorkstation=" + report.AttachedWorkstation +
                    " plannedPairs=" + report.Plans.Count +
                    " overflowSources=" + report.OverflowSources +
                    " overflowDestinations=" + report.OverflowDestinations +
                    " filterMode=" + report.FilterMode +
                    " transferFilterMode=" + LogisticsNetworkFeatures.ItemTransferFilterMode +
                    " pullAllMatching=" + (report.PullAllMatching ? "Y" : "N") +
                    " keepStockTarget=" + report.KeepStockTarget);

            foreach (ItemRouteDecision decision in report.Decisions)
            {
                Log.Out("[LogisticsNetwork] graph #" + report.GraphIndex +
                        " routeNode pos=" + decision.Position.x + "," + decision.Position.y + "," + decision.Position.z +
                        " role=" + decision.Role +
                        " decision=" + decision.Decision);
            }

            foreach (ItemRoutePlan plan in report.Plans)
            {
                Log.Out("[LogisticsNetwork] graph #" + report.GraphIndex +
                        " routePlan src=" + plan.Source.x + "," + plan.Source.y + "," + plan.Source.z +
                        "/" + plan.SourceAttachmentKind +
                        " pri=" + plan.SourcePriority +
                        " dst=" + plan.Destination.x + "," + plan.Destination.y + "," + plan.Destination.z +
                        "/" + plan.DestinationAttachmentKind +
                        " pri=" + plan.DestinationPriority +
                        " filterMode=" + plan.FilterMode +
                        " mode=passive");
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
