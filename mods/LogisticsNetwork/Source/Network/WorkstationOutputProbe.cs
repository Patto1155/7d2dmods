using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LogisticsNetwork.Network
{
    /// <summary>
    /// Read-only reflection probe for vanilla workstation instances.
    /// Discovers likely output-slot members without mutating state.
    /// </summary>
    public static class WorkstationOutputProbe
    {
        private const int MaxCandidates = 6;

        private static readonly string[] CandidateTokens =
        {
            "output",
            "result",
            "slot",
            "inventory"
        };

        public static string Describe(TileEntity tileEntity)
        {
            if (tileEntity == null)
                return null;

            Type type = tileEntity.GetType();
            List<string> candidates = new List<string>();
            int? outputArraySlots = null;

            while (type != null && type != typeof(object))
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

                FieldInfo[] fields = type.GetFields(flags);
                for (int i = 0; i < fields.Length; i++)
                {
                    if (candidates.Count >= MaxCandidates)
                        break;

                    FieldInfo field = fields[i];
                    if (!IsCandidateName(field.Name))
                        continue;

                    object value = null;
                    try
                    {
                        value = field.GetValue(tileEntity);
                    }
                    catch (Exception ex)
                    {
                        candidates.Add("field:" + field.Name + "!" + ex.GetType().Name);
                        continue;
                    }

                    string descriptor = DescribeValue(field.FieldType, value);
                    candidates.Add("field:" + field.Name + ":" + descriptor);
                    TryCaptureSlotCount(field.Name, value, ref outputArraySlots);
                }

                if (candidates.Count < MaxCandidates)
                {
                    PropertyInfo[] properties = type.GetProperties(flags);
                    for (int i = 0; i < properties.Length; i++)
                    {
                        if (candidates.Count >= MaxCandidates)
                            break;

                        PropertyInfo property = properties[i];
                        if (!property.CanRead)
                            continue;
                        if (property.GetIndexParameters().Length != 0)
                            continue;
                        if (!IsCandidateName(property.Name))
                            continue;

                        object value = null;
                        try
                        {
                            value = property.GetValue(tileEntity, null);
                        }
                        catch (Exception ex)
                        {
                            candidates.Add("prop:" + property.Name + "!" + ex.GetType().Name);
                            continue;
                        }

                        string descriptor = DescribeValue(property.PropertyType, value);
                        candidates.Add("prop:" + property.Name + ":" + descriptor);
                        TryCaptureSlotCount(property.Name, value, ref outputArraySlots);
                    }
                }

                type = type.BaseType;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("probe=").Append(tileEntity.GetType().Name);
            if (outputArraySlots.HasValue)
                sb.Append(" outputSlots=").Append(outputArraySlots.Value);

            if (candidates.Count > 0)
            {
                sb.Append(" candidates=");
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (i > 0)
                        sb.Append("|");
                    sb.Append(candidates[i]);
                }
            }
            else
            {
                sb.Append(" candidates=none");
            }

            return sb.ToString();
        }

        private static bool IsCandidateName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            string lower = name.ToLowerInvariant();
            for (int i = 0; i < CandidateTokens.Length; i++)
            {
                if (lower.Contains(CandidateTokens[i]))
                    return true;
            }

            return false;
        }

        private static string DescribeValue(Type declaredType, object value)
        {
            if (declaredType == null)
                return value == null ? "?" : value.GetType().Name;

            if (value == null)
                return declaredType.Name + "=null";

            Array array = value as Array;
            if (array != null)
                return declaredType.Name + "[" + array.Length + "]";

            return declaredType.Name;
        }

        private static void TryCaptureSlotCount(string memberName, object value, ref int? outputArraySlots)
        {
            if (outputArraySlots.HasValue)
                return;

            if (string.IsNullOrEmpty(memberName))
                return;

            string lower = memberName.ToLowerInvariant();
            if (!(lower.Contains("output") || lower.Contains("result")))
                return;

            Array array = value as Array;
            if (array != null)
                outputArraySlots = array.Length;
        }
    }
}
