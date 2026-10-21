public class NPCSpawner : BaseSpawner
{
    private const string NPC_TAG = "NPC";

    protected override string GetSpawnedTag()
    {
        return NPC_TAG;
    }

    protected override bool CanSpawn()
    {
        return EntityManager.Instance.CanSpawnNpc();
    }
}
