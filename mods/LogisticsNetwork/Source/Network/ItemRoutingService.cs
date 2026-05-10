using System.Collections.Generic;

namespace LogisticsNetwork.Network
{
    /// <summary>
    /// Passive routing evaluator for early routing bring-up.
    /// Live item moves are handled separately by <see cref="StorageTransfer"/> behind <see cref="LogisticsNetworkFeatures.EnableLiveStorageTransfer"/>.
    /// </summary>
    public static class ItemRoutingService
    {
        public static ItemRouteReport BuildPassiveReport(ItemRouteRequest request)
        {
            int importerCount = 0;
            int exporterCount = 0;
            int filterCount = 0;
            int connectorCount = 0;
            int attachedStorage = 0;
            int attachedWorkstation = 0;

            List<ItemRouteDecision> decisions = new List<ItemRouteDecision>();
            List<ItemRouteNode> sourceNodes = new List<ItemRouteNode>();
            List<ItemRouteNode> destinationNodes = new List<ItemRouteNode>();
            ItemFilterRule filterRule = ItemFilterRule.FromTransferFeatures();
            RoutingOptions options = request.Options ?? RoutingOptions.Default();
            foreach (Vector3i position in request.Graph.Connectors)
            {
                if (!NetworkConnectorSnapshot.TryDescribe(request.World, position, out NetworkConnectorSnapshot snapshot))
                    continue;

                switch (snapshot.Role)
                {
                    case "importer":
                        importerCount++;
                        break;
                    case "exporter":
                        exporterCount++;
                        break;
                    case "filter":
                        filterCount++;
                        break;
                    default:
                        connectorCount++;
                        break;
                }

                if (snapshot.AttachmentKind == "storage")
                    attachedStorage++;
                else if (snapshot.AttachmentKind == "workstation")
                    attachedWorkstation++;

                string decision = BuildDecision(snapshot);
                decisions.Add(new ItemRouteDecision(position, snapshot.Role, decision));

                if (decision == "source_candidate")
                    sourceNodes.Add(new ItemRouteNode(position, snapshot.Role, snapshot.AttachmentKind, GetDefaultPriority(snapshot.Role), filterRule.Mode));
                else if (decision == "destination_candidate")
                    destinationNodes.Add(new ItemRouteNode(position, snapshot.Role, snapshot.AttachmentKind, GetDefaultPriority(snapshot.Role), filterRule.Mode));
            }

            bool hasSourceCandidates = importerCount > 0 && attachedStorage > 0;
            bool hasDestinationCandidates = exporterCount > 0 && attachedStorage > 0;

            string summary;
            if (!hasSourceCandidates && !hasDestinationCandidates)
            {
                summary = "waiting_for_importer_exporter_storage";
            }
            else if (!hasSourceCandidates)
            {
                summary = "waiting_for_import_sources";
            }
            else if (!hasDestinationCandidates)
            {
                summary = "waiting_for_export_destinations";
            }
            else
            {
                summary = "route_candidates_ready";
            }

            List<ItemRoutePlan> plans = BuildPlans(sourceNodes, destinationNodes);
            int overflowSources = sourceNodes.Count - plans.Count;
            int overflowDestinations = destinationNodes.Count - plans.Count;

            if (plans.Count == 0 && (sourceNodes.Count > 0 || destinationNodes.Count > 0))
                summary = summary + "_no_pair";

            return new ItemRouteReport(
                request.GraphIndex,
                importerCount,
                exporterCount,
                filterCount,
                connectorCount,
                attachedStorage,
                attachedWorkstation,
                summary,
                decisions,
                plans,
                overflowSources,
                overflowDestinations,
                filterRule.Mode,
                options.PullAllMatching,
                options.KeepStockTarget);
        }

        private static string BuildDecision(NetworkConnectorSnapshot snapshot)
        {
            if (!snapshot.HasAttachment)
                return "idle:no_attachment";

            if (snapshot.Role == "importer")
            {
                return snapshot.AttachmentKind == "storage" || snapshot.AttachmentKind == "workstation"
                    ? "source_candidate"
                    : "idle:unsupported_source";
            }

            if (snapshot.Role == "exporter")
            {
                return snapshot.AttachmentKind == "storage" || snapshot.AttachmentKind == "workstation"
                    ? "destination_candidate"
                    : "idle:unsupported_destination";
            }

            if (snapshot.Role == "filter")
                return "policy_placeholder";

            return "observer_only";
        }

        private static List<ItemRoutePlan> BuildPlans(List<ItemRouteNode> sources, List<ItemRouteNode> destinations)
        {
            List<ItemRoutePlan> plans = new List<ItemRoutePlan>();
            sources.Sort(CompareNodes);
            destinations.Sort(CompareNodes);
            int pairCount = sources.Count < destinations.Count ? sources.Count : destinations.Count;

            for (int i = 0; i < pairCount; i++)
            {
                ItemRouteNode source = sources[i];
                ItemRouteNode destination = destinations[i];
                ItemFilterRuleMode mode = source.FilterMode == destination.FilterMode
                    ? source.FilterMode
                    : ItemFilterRuleMode.AllowAll;

                plans.Add(new ItemRoutePlan(
                    source.Position,
                    destination.Position,
                    source.AttachmentKind,
                    destination.AttachmentKind,
                    source.Priority,
                    destination.Priority,
                    mode));
            }

            return plans;
        }

        private static int CompareNodes(ItemRouteNode left, ItemRouteNode right)
        {
            int priorityCompare = right.Priority.CompareTo(left.Priority);
            if (priorityCompare != 0)
                return priorityCompare;

            int xCompare = left.Position.x.CompareTo(right.Position.x);
            if (xCompare != 0)
                return xCompare;

            int yCompare = left.Position.y.CompareTo(right.Position.y);
            if (yCompare != 0)
                return yCompare;

            return left.Position.z.CompareTo(right.Position.z);
        }

        private static int GetDefaultPriority(string role)
        {
            switch (role)
            {
                case "importer":
                case "exporter":
                    return 100;
                case "filter":
                    return 50;
                default:
                    return 0;
            }
        }
    }

    public sealed class ItemRouteRequest
    {
        public ItemRouteRequest(World world, NetworkGraph graph, int graphIndex, RoutingOptions options = null)
        {
            World = world;
            Graph = graph;
            GraphIndex = graphIndex;
            Options = options ?? RoutingOptions.Default();
        }

        public World World { get; }
        public NetworkGraph Graph { get; }
        public int GraphIndex { get; }
        public RoutingOptions Options { get; }
    }

    public sealed class ItemRouteReport
    {
        public ItemRouteReport(
            int graphIndex,
            int importerCount,
            int exporterCount,
            int filterCount,
            int connectorCount,
            int attachedStorage,
            int attachedWorkstation,
            string summary,
            IReadOnlyList<ItemRouteDecision> decisions,
            IReadOnlyList<ItemRoutePlan> plans,
            int overflowSources,
            int overflowDestinations,
            ItemFilterRuleMode filterMode,
            bool pullAllMatching,
            int keepStockTarget)
        {
            GraphIndex = graphIndex;
            ImporterCount = importerCount;
            ExporterCount = exporterCount;
            FilterCount = filterCount;
            ConnectorCount = connectorCount;
            AttachedStorage = attachedStorage;
            AttachedWorkstation = attachedWorkstation;
            Summary = summary;
            Decisions = decisions;
            Plans = plans;
            OverflowSources = overflowSources;
            OverflowDestinations = overflowDestinations;
            FilterMode = filterMode;
            PullAllMatching = pullAllMatching;
            KeepStockTarget = keepStockTarget;
        }

        public int GraphIndex { get; }
        public int ImporterCount { get; }
        public int ExporterCount { get; }
        public int FilterCount { get; }
        public int ConnectorCount { get; }
        public int AttachedStorage { get; }
        public int AttachedWorkstation { get; }
        public string Summary { get; }
        public IReadOnlyList<ItemRouteDecision> Decisions { get; }
        public IReadOnlyList<ItemRoutePlan> Plans { get; }
        public int OverflowSources { get; }
        public int OverflowDestinations { get; }
        public ItemFilterRuleMode FilterMode { get; }
        public bool PullAllMatching { get; }
        public int KeepStockTarget { get; }
    }

    public sealed class ItemRouteDecision
    {
        public ItemRouteDecision(Vector3i position, string role, string decision)
        {
            Position = position;
            Role = role;
            Decision = decision;
        }

        public Vector3i Position { get; }
        public string Role { get; }
        public string Decision { get; }
    }

    public sealed class ItemRouteNode
    {
        public ItemRouteNode(Vector3i position, string role, string attachmentKind, int priority, ItemFilterRuleMode filterMode)
        {
            Position = position;
            Role = role;
            AttachmentKind = attachmentKind;
            Priority = priority;
            FilterMode = filterMode;
        }

        public Vector3i Position { get; }
        public string Role { get; }
        public string AttachmentKind { get; }
        public int Priority { get; }
        public ItemFilterRuleMode FilterMode { get; }
    }

    public sealed class ItemRoutePlan
    {
        public ItemRoutePlan(
            Vector3i source,
            Vector3i destination,
            string sourceAttachmentKind,
            string destinationAttachmentKind,
            int sourcePriority,
            int destinationPriority,
            ItemFilterRuleMode filterMode)
        {
            Source = source;
            Destination = destination;
            SourceAttachmentKind = sourceAttachmentKind;
            DestinationAttachmentKind = destinationAttachmentKind;
            SourcePriority = sourcePriority;
            DestinationPriority = destinationPriority;
            FilterMode = filterMode;
        }

        public Vector3i Source { get; }
        public Vector3i Destination { get; }
        public string SourceAttachmentKind { get; }
        public string DestinationAttachmentKind { get; }
        public int SourcePriority { get; }
        public int DestinationPriority { get; }
        public ItemFilterRuleMode FilterMode { get; }
    }

    public enum ItemFilterRuleMode
    {
        AllowAll,
        Whitelist,
        Blacklist
    }

    public sealed class ItemFilterRule
    {
        private ItemFilterRule(ItemFilterRuleMode mode, IReadOnlyCollection<string> itemIds)
        {
            Mode = mode;
            ItemIds = itemIds;
        }

        public ItemFilterRuleMode Mode { get; }
        public IReadOnlyCollection<string> ItemIds { get; }

        public static ItemFilterRule AllowAll()
        {
            return new ItemFilterRule(ItemFilterRuleMode.AllowAll, new string[0]);
        }

        /// <summary>
        /// Mirrors <see cref="LogisticsNetworkFeatures.ItemTransferFilterMode"/> and
        /// <see cref="LogisticsNetworkFeatures.ItemTransferFilterIds"/> so passive route plans and tick logs
        /// report the same filter semantics as <see cref="StorageTransfer"/> (live moves still read features directly).
        /// </summary>
        public static ItemFilterRule FromTransferFeatures()
        {
            ItemFilterRuleMode mode = LogisticsNetworkFeatures.ItemTransferFilterMode;
            string[] raw = LogisticsNetworkFeatures.ItemTransferFilterIds;
            if (raw == null || raw.Length == 0)
                return new ItemFilterRule(mode, new string[0]);

            string[] copy = new string[raw.Length];
            System.Array.Copy(raw, copy, raw.Length);
            return new ItemFilterRule(mode, copy);
        }
    }

    public sealed class RoutingOptions
    {
        public RoutingOptions(bool pullAllMatching, int keepStockTarget)
        {
            PullAllMatching = pullAllMatching;
            KeepStockTarget = keepStockTarget < 0 ? 0 : keepStockTarget;
        }

        public bool PullAllMatching { get; }
        public int KeepStockTarget { get; }

        public static RoutingOptions Default()
        {
            return new RoutingOptions(pullAllMatching: true, keepStockTarget: 0);
        }
    }
}
