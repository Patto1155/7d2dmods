namespace LogisticsNetwork.Network
{
    /// <summary>
    /// Immutable snapshot of a resolved logistics endpoint for logs and future routing.
    /// Does not retain tile entity references — resolve again each tick when needed.
    /// </summary>
    public sealed class NetworkEndpoint
    {
        internal NetworkEndpoint(NetworkEndpointKind kind, Vector3i position, bool chunkLoaded, bool isValid, string typeName, int? slotCount)
        {
            Kind = kind;
            Position = position;
            ChunkLoaded = chunkLoaded;
            IsValid = isValid;
            TypeName = typeName ?? string.Empty;
            SlotCount = slotCount;
        }

        public NetworkEndpointKind Kind { get; }

        public Vector3i Position { get; }

        /// <summary>
        /// False when the chunk area is not loaded or <paramref name="chunkLoaded"/> was false at resolution time.
        /// </summary>
        public bool ChunkLoaded { get; }

        /// <summary>
        /// True when the tile entity at this position matches the expected kind (e.g. loot container for storage).
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Runtime type name of the resolved tile entity, or a short diagnostic label.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// For storage: length of <see cref="TileEntityLootContainer.items"/> when present; otherwise null.
        /// </summary>
        public int? SlotCount { get; }

        public static NetworkEndpoint StorageUnresolved(World world, Vector3i position, string reason)
        {
            bool chunkLoaded = world != null && world.IsChunkAreaLoaded(position.x, position.y, position.z);
            return new NetworkEndpoint(NetworkEndpointKind.Storage, position, chunkLoaded, false, reason, null);
        }

        public static NetworkEndpoint StorageResolved(TileEntityLootContainer loot, Vector3i position, bool chunkLoaded)
        {
            int? slots = null;
            if (loot != null && loot.items != null)
                slots = loot.items.Length;

            return new NetworkEndpoint(NetworkEndpointKind.Storage, position, chunkLoaded, true, loot.GetType().Name, slots);
        }

        public string ToLogString(int graphIndex)
        {
            return "[LogisticsNetwork] graph #" + graphIndex +
                   " storage pos=" + Position.x + "," + Position.y + "," + Position.z +
                   " chunkLoaded=" + (ChunkLoaded ? "Y" : "N") +
                   " valid=" + (IsValid ? "Y" : "N") +
                   " type=" + TypeName +
                   " slots=" + (SlotCount.HasValue ? SlotCount.Value.ToString() : "?");
        }
    }
}
