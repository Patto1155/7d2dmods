using System.Collections.Generic;
using System.Linq;

public static class AutoForgeRegistry
{
    private static readonly HashSet<Vector3i> positions = new HashSet<Vector3i>();

    public static void Register(Vector3i pos)
    {
        positions.Add(pos);
    }

    public static void Unregister(Vector3i pos)
    {
        positions.Remove(pos);
    }

    public static IEnumerable<Vector3i> All()
    {
        return positions.ToList();
    }
}
