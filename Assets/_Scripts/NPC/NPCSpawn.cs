using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCSpawn : MonoBehaviour
{
   public List<GameObject> NpcList;
	public float spawnInterval = 0f;
	public float spawnRange = 10f;

	private Coroutine spawnCoroutine;
	public bool isSpawning = false;

    // Armazena os GameObjects dos carros que est�o atualmente dentro do collider
    private HashSet<GameObject> npcsInCollider = new HashSet<GameObject>();


    void Start()
	{
		// Inicia o spawn automaticamente; remova se quiser iniciar manualmente
		StartSpawning();
	}

	void OnDisable()
	{
		// Garante que a coroutine pare quando o object for desativado
		StopSpawning();
        npcsInCollider.Clear();
    }

	public void StartSpawning()
	{
        if (isSpawning) return;
		isSpawning = true;
		spawnCoroutine = StartCoroutine(SpawnCarRoutine());
	}

	public void StopSpawning()
	{
		if (!isSpawning) return;
		isSpawning = false;

		// Opcionalmente, cancelar imediatamente usando StopCoroutine
		if (spawnCoroutine != null)
		{
			StopCoroutine(spawnCoroutine);
			spawnCoroutine = null;
		}
	}

    // Quando um collider entra no trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            // Adiciona o carro ao conjunto; se for o primeiro, para o spawn
            if (npcsInCollider.Add(other.gameObject) && npcsInCollider.Count == 1)
            {
                StopSpawning();
            }
        }
    }

    // Quando um collider sai do trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            // Remove o carro do conjunto; se ficar vazio, reinicia o spawn
            if (npcsInCollider.Remove(other.gameObject) && npcsInCollider.Count == 0)
            {
                StartSpawning();
            }
        }
    }

    IEnumerator SpawnCarRoutine()
	{
		spawnInterval = Random.Range(1f, 10f);
		while (isSpawning)
		{
			yield return new WaitForSeconds(spawnInterval);

			// Instancia um NPC, respeitando o limite definido pelo EntityManager
			bool canSpawn = EntityManager.Instance == null || EntityManager.Instance.CanSpawnNpc();
			if (canSpawn && NpcList != null && NpcList.Count > 0)
			{
				Instantiate(NpcList[Random.Range(0, NpcList.Count)], transform.position, transform.rotation);
			}

            // Decide pr�ximo intervalo e espera
            spawnInterval = Random.Range(1f, spawnRange);
        }
	}
}
