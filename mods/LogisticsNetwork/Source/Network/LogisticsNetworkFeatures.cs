namespace LogisticsNetwork.Network
{
    /// <summary>
    /// Runtime feature switches. Prefer a single place to disable risky paths during multiplayer testing.
    /// </summary>
    public static class LogisticsNetworkFeatures
    {
        /// <summary>
        /// When true, attempts at most one guarded storage→storage item move per tick across all networks.
        /// Off by default for safety; enable when testing locally.
        /// </summary>
        public static bool EnableLiveStorageTransfer = false;

        /// <summary>
        /// When true, importer connectors adjacent to a vanilla <c>TileEntityWorkstation</c>
        /// (workbench / campfire / cement mixer / chemistry station) may pull one completed
        /// output stack unit per tick into a destination chest. Inputs / fuel / tools are never read.
        /// Off by default; enable for SP testing only. <c>TileEntityForge</c> is intentionally skipped
        /// while its single-output layout is still unverified for safe extraction.
        /// </summary>
        public static bool EnableLiveWorkstationOutputExtraction = false;

        /// <summary>
        /// When true, skips all inventory mutation when <see cref="World.IsRemote"/> is true (typical multiplayer client).
        /// </summary>
        public static bool RespectWorldIsRemote = true;

        /// <summary>
        /// When live transfer is enabled, only items whose <see cref="ItemClass.Name"/> passes this rule are moved.
        /// Use <see cref="ItemTransferFilterIds"/> for whitelist/blacklist entries (internal ids, e.g. resourceWood).
        /// Per-block filter UI is not wired yet — this is a global dev/test gate.
        /// </summary>
        public static ItemFilterRuleMode ItemTransferFilterMode = ItemFilterRuleMode.AllowAll;

        /// <summary>
        /// Entries matched against <see cref="ItemClass.Name"/> when mode is Whitelist or Blacklist.
        /// Null or empty array: whitelist allows nothing; blacklist allows all.
        /// </summary>
        public static string[] ItemTransferFilterIds = null;
    }
}
