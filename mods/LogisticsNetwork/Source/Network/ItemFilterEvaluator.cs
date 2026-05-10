using System;

namespace LogisticsNetwork.Network
{
    /// <summary>
    /// Evaluates whether an item id (typically <see cref="ItemClass.Name"/>) passes whitelist/blacklist rules.
    /// </summary>
    public static class ItemFilterEvaluator
    {
        /// <summary>
        /// Returns whether <paramref name="itemClassName"/> may be transferred under the given rule.
        /// Empty whitelist allows nothing; empty blacklist allows everything.
        /// Matching is ordinal-ignore-case against trimmed entries in <paramref name="filterIds"/>.
        /// </summary>
        public static bool AllowsItemId(string itemClassName, ItemFilterRuleMode mode, string[] filterIds)
        {
            if (mode == ItemFilterRuleMode.AllowAll)
                return true;

            string name = itemClassName ?? string.Empty;
            bool emptyIds = filterIds == null || filterIds.Length == 0;

            if (mode == ItemFilterRuleMode.Whitelist)
            {
                if (emptyIds)
                    return false;
                return ContainsId(name, filterIds);
            }

            if (mode == ItemFilterRuleMode.Blacklist)
            {
                if (emptyIds)
                    return true;
                return !ContainsId(name, filterIds);
            }

            return true;
        }

        private static bool ContainsId(string itemClassName, string[] filterIds)
        {
            for (int i = 0; i < filterIds.Length; i++)
            {
                string id = filterIds[i];
                if (string.IsNullOrWhiteSpace(id))
                    continue;
                if (string.Equals(itemClassName, id.Trim(), StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
