public class AutoForgeBlock : BlockWorkstation
{
    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
        AutoForgeRegistry.Register(_blockPos);
    }

    public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        AutoForgeRegistry.Unregister(_blockPos);
        base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
    }
}
