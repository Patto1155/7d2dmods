namespace LogisticsNetwork.Network
{
    /// <summary>
    /// Pulls one unit of completed output from an adjacent vanilla <see cref="TileEntityWorkstation"/>
    /// (workbench / campfire / cement mixer / chemistry station) into a destination chest.
    /// Inputs, fuel, and tools are never read or written — only entries in <c>Output</c>.
    /// <see cref="TileEntityForge"/> uses a different single-stack output layout and is intentionally
    /// rejected here until its extraction semantics are verified.
    /// </summary>
    public static class WorkstationOutputTransfer
    {
        public static bool TryMoveOneOutputUnit(World world, ItemRoutePlan plan, int graphIndex, out string detail)
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

            if (plan.SourceAttachmentKind != "workstation" || plan.DestinationAttachmentKind != "storage")
            {
                detail = "skip:plan_not_workstation_storage";
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

            if (!srcSnap.HasAttachment || srcSnap.AttachmentKind != "workstation" ||
                !dstSnap.HasAttachment || dstSnap.AttachmentKind != "storage")
            {
                detail = "skip:attachments_not_workstation_storage";
                return false;
            }

            Vector3i fromPos = srcSnap.AttachmentPosition;
            Vector3i toPos = dstSnap.AttachmentPosition;

            if (!world.IsChunkAreaLoaded(fromPos.x, fromPos.y, fromPos.z) ||
                !world.IsChunkAreaLoaded(toPos.x, toPos.y, toPos.z))
            {
                detail = "skip:chunk_unloaded";
                return false;
            }

            TileEntity fromTile = world.GetTileEntity(0, fromPos);
            if (fromTile == null)
            {
                detail = "skip:source_tile_null";
                return false;
            }

            TileEntityWorkstation workstation = fromTile as TileEntityWorkstation;
            if (workstation == null)
            {
                detail = "skip:source_not_workstation type=" + fromTile.GetType().Name;
                return false;
            }

            if (workstation.IsUserAccessing())
            {
                detail = "skip:workstation_user_accessing";
                return false;
            }

            TileEntityLootContainer toLoot = world.GetTileEntity(0, toPos) as TileEntityLootContainer;
            if (toLoot == null)
            {
                detail = "skip:destination_not_loot_container";
                return false;
            }

            ItemStack[] output = workstation.Output;
            ItemStack[] toItems = toLoot.items;

            if (output == null || toItems == null)
            {
                detail = "skip:null_items_array output=" + (output == null ? "Y" : "N") +
                         " dest=" + (toItems == null ? "Y" : "N");
                return false;
            }

            int srcIdx = -1;
            for (int i = 0; i < output.Length; i++)
            {
                ItemStack slot = output[i];
                if (slot == null || slot.IsEmpty() || slot.count <= 0)
                    continue;

                string candidateName = StorageTransfer.DescribeItem(slot);
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
                    ? "skip:workstation_output_empty"
                    : "skip:no_matching_filtered_output";
                return false;
            }

            ItemStack sourceSlot = output[srcIdx];
            int countBefore = sourceSlot.count;
            string itemName = StorageTransfer.DescribeItem(sourceSlot);

            ItemStack moving = sourceSlot.Clone();
            moving.count = 1;

            if (!StorageTransfer.TryPlaceOneInDestination(toLoot, toItems, moving, out int destSlot, out string placementMode, out string failDetail))
            {
                detail = failDetail;
                return false;
            }

            if (sourceSlot.count <= 1)
                sourceSlot.Clear();
            else
                sourceSlot.count -= 1;

            workstation.setModified();
            toLoot.SetModified();

            detail = "graph=" + graphIndex +
                     " stationType=" + workstation.GetType().Name +
                     " item=" + itemName +
                     " placement=" + placementMode +
                     " from=" + fromPos.x + "," + fromPos.y + "," + fromPos.z + " outSlot=" + srcIdx +
                     " countBefore=" + countBefore +
                     " to=" + toPos.x + "," + toPos.y + "," + toPos.z +
                     (destSlot >= 0 ? " destSlot=" + destSlot : string.Empty) +
                     " countAfter=" + sourceSlot.count;

            return true;
        }
    }
}
