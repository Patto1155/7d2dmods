namespace LogisticsNetwork.Network
{
    /// <summary>
    /// Immutable snapshot of a resolved logistics endpoint for logs and future routing.
    /// Does not retain tile entity references — resolve again each tick when needed.
    /// </summary>
    public sealed class NetworkEndpoint
    {
        internal NetworkEndpoint(NetworkEndpointKind kind, Vector3i position, bool chunkLoaded, bool isValid, string typeName, int? slotCount, string detail)
        {
            Kind = kind;
            Position = position;
            ChunkLoaded = chunkLoaded;
            IsValid = isValid;
            TypeName = typeName ?? string.Empty;
            SlotCount = slotCount;
            Detail = string.IsNullOrEmpty(detail) ? null : detail;
        }

        public NetworkEndpointKind Kind { get; }

        public Vector3i Position { get; }

        /// <summary>
        /// False when the chunk area is not loaded or <paramref name="chunkLoaded"/> was false at resolution time.
        /// </summary>
        public bool ChunkLoaded { get; }

        /// <summary>
        /// True when the tile entity at this position matches the expected kind (e.g. loot container or workstation).
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Runtime type name of the resolved tile entity, or a short diagnostic label.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// For storage/workstation metadata: length of a known inventory array when present; otherwise null.
        /// </summary>
        public int? SlotCount { get; }

        /// <summary>
        /// Optional diagnostic note for passive inspection output.
        /// </summary>
        public string Detail { get; }

        public static NetworkEndpoint StorageUnresolved(World world, Vector3i position, string reason)
        {
            bool chunkLoaded = world != null && world.IsChunkAreaLoaded(position.x, position.y, position.z);
            return new NetworkEndpoint(NetworkEndpointKind.Storage, position, chunkLoaded, false, reason, null, null);
        }

        public static NetworkEndpoint StorageResolved(TileEntityLootContainer loot, Vector3i position, bool chunkLoaded)
        {
            int? slots = null;
            if (loot != null && loot.items != null)
                slots = loot.items.Length;

            return new NetworkEndpoint(NetworkEndpointKind.Storage, position, chunkLoaded, true, loot.GetType().Name, slots, null);
        }

        public static NetworkEndpoint WorkstationUnresolved(World world, Vector3i position, string reason)
        {
            bool chunkLoaded = world != null && world.IsChunkAreaLoaded(position.x, position.y, position.z);
            return new NetworkEndpoint(NetworkEndpointKind.Workstation, position, chunkLoaded, false, reason, null, null);
        }

        public static NetworkEndpoint WorkstationResolved(TileEntity tileEntity, Vector3i position, bool chunkLoaded, string detail)
        {
            string typeName = tileEntity != null ? tileEntity.GetType().Name : "null_tile_entity";
            return new NetworkEndpoint(NetworkEndpointKind.Workstation, position, chunkLoaded, true, typeName, null, detail);
        }

        public string ToLogString(int graphIndex)
        {
            string kindLabel = Kind == NetworkEndpointKind.Storage ? "storage" : "workstation";
            return "[LogisticsNetwork] graph #" + graphIndex +
                   " " + kindLabel + " pos=" + Position.x + "," + Position.y + "," + Position.z +
                   " chunkLoaded=" + (ChunkLoaded ? "Y" : "N") +
                   " valid=" + (IsValid ? "Y" : "N") +
                   " type=" + TypeName +
                   " slots=" + (SlotCount.HasValue ? SlotCount.Value.ToString() : "?") +
                   (string.IsNullOrEmpty(Detail) ? string.Empty : " detail=" + Detail);
        }
    }
}
