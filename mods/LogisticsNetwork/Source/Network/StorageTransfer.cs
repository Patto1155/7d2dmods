namespace LogisticsNetwork.Network
{
    /// <summary>
    /// Guarded chest-to-chest moves between endpoints resolved from importer/exporter route plans.
    /// </summary>
    public static class StorageTransfer
    {
        /// <summary>
        /// Moves one item from the source loot container to the destination loot container.
        /// Placement tries, in order: first empty slot, <see cref="TileEntityLootContainer.TryStackItem"/>,
        /// then <see cref="TileEntityLootContainer.AddItem"/>.
        /// </summary>
        /// <returns>True if one unit was moved.</returns>
        public static bool TryMoveOneStackUnit(World world, ItemRoutePlan plan, int graphIndex, out string detail)
        {
            detail = null;

            if (world == null)
            {
                detail = "skip:null_world";
                return false;
            }

            if (LogisticsNetworkFeatures.RespectWorldIsRemote && world.IsRemote())
            {
                detail = "skip:remote_world";
                return false;
            }

            if (plan.SourceAttachmentKind != "storage" || plan.DestinationAttachmentKind != "storage")
            {
                detail = "skip:plan_not_storage_storage";
                return false;
            }

            if (!NetworkConnectorSnapshot.TryDescribe(world, plan.Source, out NetworkConnectorSnapshot srcSnap))
            {
                detail = "skip:snapshot_failed side=source pos=" + plan.Source.x + "," + plan.Source.y + "," + plan.Source.z;
                return false;
            }

            if (!NetworkConnectorSnapshot.TryDescribe(world, plan.Destination, out NetworkConnectorSnapshot dstSnap))
            {
                detail = "skip:snapshot_failed side=destination pos=" + plan.Destination.x + "," + plan.Destination.y + "," + plan.Destination.z;
                return false;
            }

            if (srcSnap.Role != "importer" || dstSnap.Role != "exporter")
            {
                detail = "skip:roles_not_importer_exporter";
                return false;
            }

            if (!srcSnap.HasAttachment || srcSnap.AttachmentKind != "storage" ||
                !dstSnap.HasAttachment || dstSnap.AttachmentKind != "storage")
            {
                detail = "skip:attachments_not_storage";
                return false;
            }

            Vector3i fromPos = srcSnap.AttachmentPosition;
            Vector3i toPos = dstSnap.AttachmentPosition;

            if (fromPos.x == toPos.x && fromPos.y == toPos.y && fromPos.z == toPos.z)
            {
                detail = "skip:same_storage_block";
                return false;
            }

            if (!world.IsChunkAreaLoaded(fromPos.x, fromPos.y, fromPos.z) ||
                !world.IsChunkAreaLoaded(toPos.x, toPos.y, toPos.z))
            {
                detail = "skip:chunk_unloaded";
                return false;
            }

            TileEntityLootContainer fromLoot = world.GetTileEntity(0, fromPos) as TileEntityLootContainer;
            TileEntityLootContainer toLoot = world.GetTileEntity(0, toPos) as TileEntityLootContainer;

            if (fromLoot == null || toLoot == null)
            {
                detail = "skip:tile_not_loot_container";
                return false;
            }

            ItemStack[] fromItems = fromLoot.items;
            ItemStack[] toItems = toLoot.items;

            if (fromItems == null || toItems == null)
            {
                detail = "skip:null_items_array";
                return false;
            }

            int srcIdx = -1;
            for (int i = 0; i < fromItems.Length; i++)
            {
                ItemStack slot = fromItems[i];
                if (slot == null || slot.IsEmpty() || slot.count <= 0)
                    continue;

                string candidateName = DescribeItem(slot);
                if (!ItemFilterEvaluator.AllowsItemId(
                        candidateName,
                        LogisticsNetworkFeatures.ItemTransferFilterMode,
                        LogisticsNetworkFeatures.ItemTransferFilterIds))
                    continue;

                srcIdx = i;
                break;
            }

            if (srcIdx < 0)
            {
                detail = LogisticsNetworkFeatures.ItemTransferFilterMode == ItemFilterRuleMode.AllowAll
                    ? "skip:source_has_no_items"
                    : "skip:no_matching_filtered_source_item";
                return false;
            }

            ItemStack sourceSlot = fromItems[srcIdx];
            int countBefore = sourceSlot.count;

            ItemStack moving = sourceSlot.Clone();
            moving.count = 1;

            string itemName = DescribeItem(moving);

            if (!TryPlaceOneInDestination(toLoot, toItems, moving, out int destSlot, out string placementMode, out string failDetail))
            {
                detail = failDetail;
                return false;
            }

            if (sourceSlot.count <= 1)
                sourceSlot.Clear();
            else
                sourceSlot.count -= 1;

            fromLoot.UpdateSlot(srcIdx, sourceSlot);

            fromLoot.SetModified();
            toLoot.SetModified();

            detail = "graph=" + graphIndex +
                     " item=" + itemName +
                     " placement=" + placementMode +
                     " from=" + fromPos.x + "," + fromPos.y + "," + fromPos.z + " slot=" + srcIdx +
                     " countBefore=" + countBefore +
                     " to=" + toPos.x + "," + toPos.y + "," + toPos.z +
                     (destSlot >= 0 ? " destSlot=" + destSlot : string.Empty) +
                     " countAfter=" + sourceSlot.count;

            return true;
        }

        /// <summary>
        /// Tries to place exactly one item (caller sets <paramref name="movingOne"/>.count).
        /// Internal so workstation-output extraction can reuse the same placement contract.
        /// </summary>
        internal static bool TryPlaceOneInDestination(
            TileEntityLootContainer toLoot,
            ItemStack[] toItems,
            ItemStack movingOne,
            out int destSlot,
            out string placementMode,
            out string failDetail)
        {
            destSlot = -1;
            placementMode = null;
            failDetail = null;

            int emptySlots = 0;
            for (int j = 0; j < toItems.Length; j++)
            {
                ItemStack slot = toItems[j];
                if (slot != null && slot.IsEmpty())
                {
                    emptySlots++;
                    toLoot.UpdateSlot(j, movingOne);
                    destSlot = j;
                    placementMode = "empty";
                    return true;
                }
            }

            int stackCompatibleSlots = 0;
            string lastStackDiag = null;
            for (int j = 0; j < toItems.Length; j++)
            {
                ItemStack slot = toItems[j];
                if (slot == null || slot.IsEmpty())
                    continue;
                if (!slot.CanStackWith(movingOne))
                    continue;

                stackCompatibleSlots++;
                ItemStack chunk = movingOne.Clone();
                chunk.count = 1;
                var stackResult = toLoot.TryStackItem(j, chunk);
                if (chunk.count == 0)
                {
                    destSlot = j;
                    placementMode = "stack";
                    return true;
                }

                lastStackDiag = "slot=" + j + " remain=" + chunk.count + " stackOk=" + stackResult.Item1 + " stackPartial=" + stackResult.Item2;
            }

            ItemStack addChunk = movingOne.Clone();
            addChunk.count = 1;
            if (toLoot.AddItem(addChunk))
            {
                destSlot = -1;
                placementMode = "additem";
                return true;
            }

            failDetail = "skip:dest_placement_failed emptySlots=" + emptySlots +
                         " stackCompatibleSlots=" + stackCompatibleSlots +
                         (lastStackDiag != null ? " " + lastStackDiag : " stackPartialHint=none") +
                         " additem_false";
            return false;
        }

        internal static string DescribeItem(ItemStack stack)
        {
            if (stack == null)
                return "null_stack";

            try
            {
                if (stack.itemValue != null && stack.itemValue.ItemClass != null)
                    return stack.itemValue.ItemClass.Name ?? "unnamed";
            }
            catch
            {
            }

            return "unknown";
        }
    }
}
