using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCars : MonoBehaviour
{
	public List<GameObject> CarList;
	public float spawnInterval = 0f;

	private Coroutine spawnCoroutine;
	public bool isSpawning = false;

	void Start()
	{
		// Inicia o spawn automaticamente; remova se quiser iniciar manualmente
		StartSpawning();
	}

	void OnDisable()
	{
		// Garante que a coroutine pare quando o object for desativado
		StopSpawning();
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

	IEnumerator SpawnCarRoutine()
	{
		while (isSpawning)
		{
			// Instancia um carro
			if (CarList != null && CarList.Count > 0)
			{
				Instantiate(CarList[Random.Range(0, CarList.Count)], transform.position, transform.rotation);
			}

			// Decide próximo intervalo e espera
			spawnInterval = Random.Range(1f, 10f);
			yield return new WaitForSeconds(spawnInterval);
		}
	}
}