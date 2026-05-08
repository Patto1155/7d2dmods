using System.Collections.Generic;
using System.Text;
using LogisticsNetwork.Blocks;

namespace LogisticsNetwork.Network
{
    /// <summary>
    /// Passive, one-way connector snapshot used for logging and future routing metadata.
    /// Captures the connector block plus its immediate adjacent block / tile-entity context.
    /// </summary>
    public sealed class NetworkConnectorSnapshot
    {
        private static readonly Vector3i[] Directions = new Vector3i[]
        {
            new Vector3i( 1, 0, 0),
            new Vector3i(-1, 0, 0),
            new Vector3i( 0, 1, 0),
            new Vector3i( 0,-1, 0),
            new Vector3i( 0, 0, 1),
            new Vector3i( 0, 0,-1),
        };

        internal NetworkConnectorSnapshot(
            Vector3i position,
            bool chunkLoaded,
            bool isValid,
            string blockTypeName,
            bool hasAttachment,
            Vector3i attachmentPosition,
            string attachmentKind,
            string attachedBlockTypeName,
            string attachedTileEntityTypeName,
            string detail,
            string adjacentSummary)
        {
            Position = position;
            ChunkLoaded = chunkLoaded;
            IsValid = isValid;
            BlockTypeName = blockTypeName ?? string.Empty;
            HasAttachment = hasAttachment;
            AttachmentPosition = attachmentPosition;
            AttachmentKind = string.IsNullOrEmpty(attachmentKind) ? "none" : attachmentKind;
            AttachedBlockTypeName = string.IsNullOrEmpty(attachedBlockTypeName) ? "unknown" : attachedBlockTypeName;
            AttachedTileEntityTypeName = string.IsNullOrEmpty(attachedTileEntityTypeName) ? "none" : attachedTileEntityTypeName;
            Detail = string.IsNullOrEmpty(detail) ? null : detail;
            AdjacentSummary = string.IsNullOrEmpty(adjacentSummary) ? "none" : adjacentSummary;
        }

        public Vector3i Position { get; }

        public bool ChunkLoaded { get; }

        public bool IsValid { get; }

        public string BlockTypeName { get; }

        public bool HasAttachment { get; }

        public Vector3i AttachmentPosition { get; }

        public string AttachmentKind { get; }

        public string AttachedBlockTypeName { get; }

        public string AttachedTileEntityTypeName { get; }

        public string Detail { get; }

        public string AdjacentSummary { get; }

        public static bool TryDescribe(World world, Vector3i position, out NetworkConnectorSnapshot snapshot)
        {
            snapshot = null;
            if (world == null)
                return false;

            bool chunkLoaded = world.IsChunkAreaLoaded(position.x, position.y, position.z);
            Block block = world.GetBlock(position).Block;
            string blockTypeName = DescribeBlock(block);

            if (!chunkLoaded)
            {
                snapshot = new NetworkConnectorSnapshot(
                    position,
                    chunkLoaded: false,
                    isValid: block is LogisticsConnectorBlock,
                    blockTypeName: blockTypeName,
                    hasAttachment: false,
                    attachmentPosition: default(Vector3i),
                    attachmentKind: "chunk_unloaded",
                    attachedBlockTypeName: "unknown",
                    attachedTileEntityTypeName: "unknown",
                    detail: "chunk_unloaded",
                    adjacentSummary: "chunk_unloaded");
                return true;
            }

            List<string> adjacentSummary = new List<string>(Directions.Length);
            bool hasAttachment = false;
            Vector3i attachmentPosition = default(Vector3i);
            string attachmentKind = null;
            string attachedBlockTypeName = null;
            string attachedTileEntityTypeName = null;
            int attachmentScore = int.MinValue;

            for (int i = 0; i < Directions.Length; i++)
            {
                Vector3i neighborPosition = position + Directions[i];
                Block neighborBlock = world.GetBlock(neighborPosition).Block;
                string neighborBlockTypeName = DescribeBlock(neighborBlock);

                TileEntity neighborTileEntity = world.GetTileEntity(0, neighborPosition);
                string neighborTileEntityTypeName = DescribeTileEntity(neighborTileEntity);

                string neighborKind = DescribeNeighborKind(neighborBlock, neighborTileEntity);
                adjacentSummary.Add(FormatNeighborEntry(neighborPosition, neighborKind, neighborBlockTypeName, neighborTileEntityTypeName));

                int score = GetAttachmentScore(neighborBlock, neighborTileEntity);
                if (score > attachmentScore)
                {
                    attachmentScore = score;
                    hasAttachment = score > 0;
                    attachmentPosition = neighborPosition;
                    attachmentKind = neighborKind;
                    attachedBlockTypeName = neighborBlockTypeName;
                    attachedTileEntityTypeName = neighborTileEntityTypeName;
                }
            }

            snapshot = new NetworkConnectorSnapshot(
                position,
                chunkLoaded: true,
                isValid: block is LogisticsConnectorBlock,
                blockTypeName: blockTypeName,
                hasAttachment: hasAttachment,
                attachmentPosition: attachmentPosition,
                attachmentKind: attachmentKind,
                attachedBlockTypeName: attachedBlockTypeName,
                attachedTileEntityTypeName: attachedTileEntityTypeName,
                detail: hasAttachment ? null : "no_attachment_found",
                adjacentSummary: JoinEntries(adjacentSummary));
            return true;
        }

        public string ToLogString(int graphIndex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("graph #").Append(graphIndex)
              .Append(" connector pos=").Append(FormatVector(Position))
              .Append(" chunkLoaded=").Append(ChunkLoaded ? "Y" : "N")
              .Append(" valid=").Append(IsValid ? "Y" : "N")
              .Append(" type=").Append(BlockTypeName)
              .Append(" attached=");

            if (HasAttachment)
            {
                sb.Append(FormatVector(AttachmentPosition))
                  .Append("/").Append(AttachmentKind)
                  .Append(" block=").Append(AttachedBlockTypeName)
                  .Append(" te=").Append(AttachedTileEntityTypeName);
            }
            else
            {
                sb.Append("none");
            }

            sb.Append(" neighbors=").Append(AdjacentSummary);

            if (!string.IsNullOrEmpty(Detail))
                sb.Append(" detail=").Append(Detail);

            return "[LogisticsNetwork] " + sb.ToString();
        }

        private static int GetAttachmentScore(Block block, TileEntity tileEntity)
        {
            if (tileEntity is TileEntityLootContainer || tileEntity is TileEntityWorkstation)
                return 3;

            if (tileEntity != null)
                return 2;

            if (block is LogisticsConduitBlock || block is LogisticsConnectorBlock)
                return 1;

            return block != null ? 0 : -1;
        }

        private static string DescribeNeighborKind(Block block, TileEntity tileEntity)
        {
            if (tileEntity is TileEntityLootContainer)
                return "storage";

            if (tileEntity is TileEntityWorkstation)
                return "workstation";

            if (tileEntity != null)
                return tileEntity.GetType().Name;

            if (block is LogisticsConduitBlock)
                return "conduit";

            if (block is LogisticsConnectorBlock)
                return "connector";

            return block != null ? block.GetType().Name : "air";
        }

        private static string DescribeBlock(Block block)
        {
            return block != null ? block.GetType().Name : "null_block";
        }

        private static string DescribeTileEntity(TileEntity tileEntity)
        {
            return tileEntity != null ? tileEntity.GetType().Name : "none";
        }

        private static string FormatNeighborEntry(Vector3i position, string kind, string blockType, string tileEntityType)
        {
            return FormatVector(position) + ":" + kind + ":block=" + blockType + ":te=" + tileEntityType;
        }

        private static string JoinEntries(List<string> entries)
        {
            if (entries == null || entries.Count == 0)
                return "none";

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < entries.Count; i++)
            {
                if (i > 0)
                    sb.Append('|');
                sb.Append(entries[i]);
            }

            return sb.ToString();
        }

        private static string FormatVector(Vector3i position)
        {
            return position.x + "," + position.y + "," + position.z;
        }
    }
}