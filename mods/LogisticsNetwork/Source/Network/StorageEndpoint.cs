namespace LogisticsNetwork.Network
{
    /// <summary>
    /// Passive resolver for adjacent storage tile entities (<see cref="TileEntityLootContainer"/>).
    /// Exposes metadata only — no item movement.
    /// </summary>
    public static class StorageEndpoint
    {
        /// <summary>
        /// Builds a <see cref="NetworkEndpoint"/> snapshot for a storage graph node position.
        /// Returns false only when <paramref name="world"/> is null.
        /// </summary>
        public static bool TryDescribe(World world, Vector3i position, out NetworkEndpoint endpoint)
        {
            endpoint = null;
            if (world == null)
                return false;

            if (!world.IsChunkAreaLoaded(position.x, position.y, position.z))
            {
                endpoint = NetworkEndpoint.StorageUnresolved(world, position, "chunk_unloaded");
                return true;
            }

            TileEntity tileEntity = world.GetTileEntity(0, position);
            if (tileEntity == null)
            {
                endpoint = new NetworkEndpoint(
                    NetworkEndpointKind.Storage,
                    position,
                    chunkLoaded: true,
                    isValid: false,
                    typeName: "null_tile_entity",
                    slotCount: null);
                return true;
            }

            if (tileEntity is TileEntityLootContainer loot)
            {
                endpoint = NetworkEndpoint.StorageResolved(loot, position, chunkLoaded: true);
                return true;
            }

            endpoint = new NetworkEndpoint(
                NetworkEndpointKind.Storage,
                position,
                chunkLoaded: true,
                isValid: false,
                typeName: tileEntity.GetType().Name,
                slotCount: null);
            return true;
        }
    }
}
