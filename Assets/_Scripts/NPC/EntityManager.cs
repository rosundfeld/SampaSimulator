using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public static EntityManager Instance { get; private set; }

    [SerializeField] private int maxNpcCount = 100;
    [SerializeField] private string npcTag = "NPC";

    public int CurrentNpcCount => GameObject.FindGameObjectsWithTag(npcTag).Length;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Usado pelos spawners para saber se ainda podem instanciar NPCs
    public bool CanSpawnNpc()
    {
        return CurrentNpcCount < maxNpcCount;
    }
}
