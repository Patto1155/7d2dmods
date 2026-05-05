// PHASE 1 STUB: craftingData.Recipe — TileEntityWorkstation.craftingData field name unverified; recipe processing disabled
// PHASE 1 STUB: craftingData.CraftingProgress — field name unverified; disabled
// PHASE 1 STUB: IngredientsAvailable / DeductIngredients / PushOutput — require craftingData.Recipe; disabled
// PHASE 1 STUB: inputSlots / outputSlots — field names on TileEntityWorkstation unverified; disabled

using System.Collections.Generic;

public static class AutoForgeTick
{
    public static void RunAll()
    {
        foreach (Vector3i pos in AutoForgeRegistry.All())
        {
            World world = GameManager.Instance.World;
            if (world == null)
                continue;

            TileEntityWorkstation te = world.GetTileEntity(0, pos) as TileEntityWorkstation;
            if (te == null)
            {
                AutoForgeRegistry.Unregister(pos);
                continue;
            }

            RunOne(world, pos, te);
        }
    }

    private static void RunOne(World world, Vector3i pos, TileEntityWorkstation te)
    {
        List<TileEntityLootContainer> network = ConduitNetwork.Scan(world, pos);

        Log.Out("[AutoForge] Tick at " + pos + " — " + network.Count + " containers in network");

        // PHASE 1 STUB: recipe processing disabled — craftingData API unverified
        Log.Out("[AutoForge] Tick at " + pos + " — recipe processing stub");
    }
}
