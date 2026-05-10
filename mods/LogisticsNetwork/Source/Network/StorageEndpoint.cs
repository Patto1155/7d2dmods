using System;
using System.Reflection;

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
                    slotCount: null,
                    detail: null);

                return true;
            }

            if (tileEntity is TileEntityLootContainer loot)
            {
                endpoint = BuildStorageSnapshot(loot, position);
                return true;
            }

            endpoint = new NetworkEndpoint(
                NetworkEndpointKind.Storage,
                position,
                chunkLoaded: true,
                isValid: false,
                typeName: tileEntity.GetType().Name,
                slotCount: null,
                detail: null);

            return true;
        }

        private static NetworkEndpoint BuildStorageSnapshot(TileEntityLootContainer loot, Vector3i position)
        {
            Array slots = loot != null ? loot.items as Array : null;
            if (slots == null)
            {
                return new NetworkEndpoint(
                    NetworkEndpointKind.Storage,
                    position,
                    chunkLoaded: true,
                    isValid: true,
                    typeName: loot != null ? loot.GetType().Name : "TileEntityLootContainer",
                    slotCount: null,
                    detail: "slots_unavailable");
            }

            int totalSlots = slots.Length;
            int occupiedSlots = 0;

            for (int i = 0; i < totalSlots; i++)
            {
                object stack = slots.GetValue(i);
                if (HasItems(stack))
                    occupiedSlots++;
            }

            bool canInsert = occupiedSlots < totalSlots;
            bool canExtract = occupiedSlots > 0;
            string detail = "slotsUsed=" + occupiedSlots + "/" + totalSlots +
                            " canInsert=" + (canInsert ? "Y" : "N") +
                            " canExtract=" + (canExtract ? "Y" : "N");

            return new NetworkEndpoint(
                NetworkEndpointKind.Storage,
                position,
                chunkLoaded: true,
                isValid: true,
                typeName: loot.GetType().Name,
                slotCount: totalSlots,
                detail: detail);
        }

        private static bool HasItems(object stack)
        {
            if (stack == null)
                return false;

            Type stackType = stack.GetType();

            int count;
            if (TryReadInt(stackType.GetProperty("count", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), stack, out count) ||
                TryReadInt(stackType.GetProperty("Count", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), stack, out count) ||
                TryReadInt(stackType.GetField("count", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), stack, out count) ||
                TryReadInt(stackType.GetField("Count", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), stack, out count))
            {
                return count > 0;
            }

            bool isEmpty;
            if (TryReadBool(stackType.GetProperty("IsEmpty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), stack, out isEmpty) ||
                TryReadBool(stackType.GetField("IsEmpty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), stack, out isEmpty))
            {
                return !isEmpty;
            }

            // If stack shape is unknown at runtime, treat as occupied to avoid false negatives.
            return true;
        }

        private static bool TryReadInt(MemberInfo member, object instance, out int value)
        {
            value = 0;
            object raw;
            if (!TryReadMember(member, instance, out raw))
                return false;

            if (raw is int intValue)
            {
                value = intValue;
                return true;
            }

            return false;
        }

        private static bool TryReadBool(MemberInfo member, object instance, out bool value)
        {
            value = false;
            object raw;
            if (!TryReadMember(member, instance, out raw))
                return false;

            if (raw is bool boolValue)
            {
                value = boolValue;
                return true;
            }

            return false;
        }

        private static bool TryReadMember(MemberInfo member, object instance, out object value)
        {
            value = null;
            if (member == null || instance == null)
                return false;

            try
            {
                if (member is PropertyInfo property)
                {
                    value = property.GetValue(instance, null);
                    return true;
                }

                if (member is FieldInfo field)
                {
                    value = field.GetValue(instance);
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }
}
