using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnCars : MonoBehaviour
{
	public List<GameObject> CarList;
	public float spawnInterval = 0f;

	private Coroutine spawnCoroutine;
	public bool isSpawning = false;

    // Armazena os GameObjects dos carros que estão atualmente dentro do collider
    private HashSet<GameObject> carsInCollider = new HashSet<GameObject>();


    void Start()
	{
		// Inicia o spawn automaticamente; remova se quiser iniciar manualmente
		StartSpawning();
	}

	void OnDisable()
	{
		// Garante que a coroutine pare quando o object for desativado
		StopSpawning();
        carsInCollider.Clear();
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
        if (other.CompareTag("Car"))
        {
            // Adiciona o carro ao conjunto; se for o primeiro, para o spawn
            if (carsInCollider.Add(other.gameObject) && carsInCollider.Count == 1)
            {
                StopSpawning();
            }
        }
    }

    // Quando um collider sai do trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            // Remove o carro do conjunto; se ficar vazio, reinicia o spawn
            if (carsInCollider.Remove(other.gameObject) && carsInCollider.Count == 0)
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

			// Instancia um carro
			if (CarList != null && CarList.Count > 0)
			{
				Instantiate(CarList[Random.Range(0, CarList.Count)], transform.position, transform.rotation);
			}

            // Decide próximo intervalo e espera
            spawnInterval = Random.Range(1f, 10f);
        }
	}
}