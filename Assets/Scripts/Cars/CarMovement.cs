using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarMoviment : MonoBehaviour
{
	[Tooltip("Unidades por segundo")]
	public float speed = 10f;

	[Tooltip("Tempo (s) até destruir automaticamente. <= 0 desativa destruição automática")]
	public float despawnTime = 100f;

	public float remainingTime;
	public bool isStaticCar = false;

    // Rastreia Colliders que entraram no trigger — permite checar .enabled e .gameObject.activeInHierarchy
    private HashSet<Collider> trackedColliders = new HashSet<Collider>();

    // Guarda velocidade original para restaurar
    private float defaultSpeed;

    void Start()
	{
        defaultSpeed = speed;
        if (!isStaticCar)
            remainingTime = despawnTime;
    }

	void FixedUpdate()
	{
		// Movimento frame-rate independent
		if (!isStaticCar)
            transform.Translate(Vector3.forward * speed * Time.fixedDeltaTime, Space.Self);
	}

    void OnDisable()
    {
        trackedColliders.Clear();
    }

    void Update()
	{
		// Contador simples de despawn
		if (remainingTime > 0f && !isStaticCar)
		{
			remainingTime -= Time.deltaTime;
			if (remainingTime <= 0f)
				Destroy(gameObject);
		}

        // Verifica colliders rastreados: se foram desabilitados ou destruídos,
        // trata como "exit" (OnTriggerExit pode não ser chamado quando collider é desabilitado)
        if (trackedColliders.Count > 0)
        {
            // Criar lista para evitar modificar HashSet durante iteração
            var copy = trackedColliders.ToList();
            foreach (var col in copy)
            {
                if (col == null || !col.enabled || !col.gameObject.activeInHierarchy)
                {
                    HandleColliderExit(col);
                }
            }
        }
    }

	public void DespawnNow()
	{
		Destroy(gameObject);
	}

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("CarBack") || collision.CompareTag("Semaphore"))
        {
            HandleColliderEnter(collision);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("CarBack") || collision.CompareTag("Semaphore"))
        {
            HandleColliderExit(collision);
        }
    }

    private void HandleColliderEnter(Collider col)
    {
        if (col == null) return;
        if (trackedColliders.Add(col) && trackedColliders.Count == 1)
        {
            // primeiro collider dentro -> parar carro
            this.speed = 0f;
        }
    }

    private void HandleColliderExit(Collider col)
    {
        // Remove (Remove trata nulls internamente; defensivamente verifica null)
        if (col != null)
            trackedColliders.Remove(col);
        else
            trackedColliders.RemoveWhere(c => c == null);

        if (trackedColliders.Count == 0)
        {
            // nenhum collider restante -> retomar velocidade original
            this.speed = defaultSpeed;
        }
    }
}
