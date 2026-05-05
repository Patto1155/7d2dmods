using System.Collections.Generic;

public static class ConduitNetwork
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

    public static List<TileEntityLootContainer> Scan(World world, Vector3i origin, int maxSteps = 32)
    {
        List<TileEntityLootContainer> results = new List<TileEntityLootContainer>();

        // BFS visited check before block-type check to avoid re-scanning conduits in dense networks
        HashSet<Vector3i> visited = new HashSet<Vector3i>();
        Queue<Vector3i> queue = new Queue<Vector3i>();

        visited.Add(origin);
        queue.Enqueue(origin);
        int steps = 0;

        while (queue.Count > 0 && steps < maxSteps)
        {
            Vector3i current = queue.Dequeue();
            steps++;

            foreach (Vector3i dir in Directions)
            {
                Vector3i neighbor = current + dir;

                if (visited.Contains(neighbor))
                    continue;

                visited.Add(neighbor);

                BlockValue bv = world.GetBlock(neighbor);
                Block block = bv.Block;

                if (block is ConduitBlock || block is AutoForgeBlock)
                {
                    queue.Enqueue(neighbor);
                    continue;
                }

                // Not a conduit — check if there's a loot tile entity here
                TileEntity te = world.GetTileEntity(0, neighbor);
                if (te is TileEntityLootContainer loot)
                    results.Add(loot);
            }
        }

        return results;
    }
}
