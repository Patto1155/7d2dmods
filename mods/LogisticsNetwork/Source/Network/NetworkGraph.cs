using System.Collections.Generic;

namespace LogisticsNetwork.Network
{
    public enum NetworkEndpointKind
    {
        Storage,
        Workstation
    }

    public sealed class NetworkGraph
    {
        private readonly HashSet<Vector3i> conduits = new HashSet<Vector3i>();
        private readonly HashSet<Vector3i> connectors = new HashSet<Vector3i>();
        private readonly HashSet<Vector3i> storage = new HashSet<Vector3i>();
        private readonly HashSet<Vector3i> workstations = new HashSet<Vector3i>();

        public NetworkGraph(Vector3i origin)
        {
            Origin = origin;
        }

        public Vector3i Origin { get; }

        public IReadOnlyCollection<Vector3i> Conduits => conduits;
        public IReadOnlyCollection<Vector3i> Connectors => connectors;
        public IReadOnlyCollection<Vector3i> Storage => storage;
        public IReadOnlyCollection<Vector3i> Workstations => workstations;

        public int ConduitCount => conduits.Count;
        public int ConnectorCount => connectors.Count;
        public int StorageCount => storage.Count;
        public int WorkstationCount => workstations.Count;

        public bool TruncatedByDepthLimit { get; set; }

        public bool IsEmpty => ConduitCount == 0 && ConnectorCount == 0 && StorageCount == 0 && WorkstationCount == 0;

        public void AddConduit(Vector3i position)
        {
            conduits.Add(position);
        }

        public void AddConnector(Vector3i position)
        {
            connectors.Add(position);
        }

        public void AddEndpoint(NetworkEndpointKind kind, Vector3i position)
        {
            switch (kind)
            {
                case NetworkEndpointKind.Storage:
                    storage.Add(position);
                    break;
                case NetworkEndpointKind.Workstation:
                    workstations.Add(position);
                    break;
            }
        }

        public string ToSummaryString(int index)
        {
            string truncatedNote = TruncatedByDepthLimit ? " truncatedDepth=Y" : string.Empty;
            return "Network #" + index + " origin=" + Origin +
                   " conduits=" + ConduitCount +
                   " connectors=" + ConnectorCount +
                   " storage=" + StorageCount +
                   " workstations=" + WorkstationCount +
                   truncatedNote;
        }
    }
}
