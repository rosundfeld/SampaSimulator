using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Spawns a random prefab at intervals, pausing while something with the spawned tag still occupies the spawn trigger.
public abstract class BaseSpawner : MonoBehaviour
{
    [FormerlySerializedAs("CarList")]
    [FormerlySerializedAs("NpcList")]
    [SerializeField] private List<GameObject> prefabList;
    [SerializeField] private float minSpawnInterval = 1f;
    [FormerlySerializedAs("spawnRange")]
    [SerializeField] private float maxSpawnInterval = 10f;

    private readonly HashSet<GameObject> objectsInSpawnArea = new HashSet<GameObject>();
    private Coroutine spawnCoroutine;

    protected abstract string GetSpawnedTag();

    protected virtual bool CanSpawn()
    {
        return true;
    }

    private void OnEnable()
    {
        StartSpawning();
    }

    private void OnDisable()
    {
        StopSpawning();
        objectsInSpawnArea.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(GetSpawnedTag()))
            return;

        if (objectsInSpawnArea.Add(other.gameObject) && objectsInSpawnArea.Count == 1)
            StopSpawning();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(GetSpawnedTag()))
            return;

        if (objectsInSpawnArea.Remove(other.gameObject) && objectsInSpawnArea.Count == 0)
            StartSpawning();
    }

    private void StartSpawning()
    {
        if (spawnCoroutine != null)
            return;

        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private void StopSpawning()
    {
        if (spawnCoroutine == null)
            return;

        StopCoroutine(spawnCoroutine);
        spawnCoroutine = null;
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));

            if (CanSpawn() && prefabList.Count > 0)
                Instantiate(prefabList[Random.Range(0, prefabList.Count)], transform.position, transform.rotation);
        }
    }
}
