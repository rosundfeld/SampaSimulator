using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarMoviment : MonoBehaviour
{
	private const string TagCarBack = "CarBack";
	private const string TagSemaphore = "Semaphore";
	private const string TagDespawner = "Despawner";
	private const string TagPlayer = "Player";

	[Tooltip("Velocidade máxima (unidades por segundo)")]
	[SerializeField] private float speed = 10f;

	[Tooltip("Aceleração (unidades por segundo²) ao sair do repouso")]
	[SerializeField] private float acceleration = 5f;

	[Tooltip("Tags que contam como atropelamento")]
	[SerializeField] private string[] hitTags = { TagPlayer };

	[Tooltip("Tempo (s) até destruir automaticamente. <= 0 desativa destruição automática")]
	[SerializeField] private float despawnTime = 0f;

	[SerializeField] private bool isStaticCar = false;

    // Rastreia Colliders que entraram no trigger � permite checar .enabled e .gameObject.activeInHierarchy
    private HashSet<Collider> trackedColliders = new HashSet<Collider>();

    // Velocidade atual; sobe gradualmente até 'speed' (máxima)
    private float currentSpeed;

    void Start()
	{
        currentSpeed = speed;
    }

	void FixedUpdate()
	{
		if (isStaticCar) return;

		// Parado enquanto houver colliders bloqueando; senão acelera até a máxima
		if (trackedColliders.Count > 0)
			currentSpeed = 0f;
		else
			currentSpeed = Mathf.MoveTowards(currentSpeed, speed, acceleration * Time.fixedDeltaTime);

		transform.Translate(Vector3.forward * currentSpeed * Time.fixedDeltaTime, Space.Self);
	}

    void OnDisable()
    {
        trackedColliders.Clear();
    }

    void Update()
	{

        // Verifica colliders rastreados: se foram desabilitados ou destru�dos,
        // trata como "exit" (OnTriggerExit pode n�o ser chamado quando collider � desabilitado)
        if (trackedColliders.Count > 0)
        {
            // Criar lista para evitar modificar HashSet durante itera��o
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
        if (collision.CompareTag(TagCarBack) || collision.CompareTag(TagSemaphore))
        {
            HandleColliderEnter(collision);
        }

		if (collision.CompareTag(TagDespawner))
		{
			HandleDespawnColliderEnter(collision);
		}

		if (!isStaticCar && currentSpeed >= speed && IsHitTarget(collision))
		{
			Debug.Log("Atropelou");
		}
	}

	private bool IsHitTarget(Collider col)
	{
		foreach (var tag in hitTags)
		{
			if (col.CompareTag(tag)) return true;
		}
		return false;
	}

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag(TagCarBack) || collision.CompareTag(TagSemaphore))
        {
            HandleColliderExit(collision);
        }
    }

	private void HandleDespawnColliderEnter(Collider col)
	{
		if (col == null) return;
		if (trackedColliders.Add(col) && trackedColliders.Count == 1)
		{
			DespawnNow();
		}
	}

	private void HandleColliderEnter(Collider col)
    {
        if (col == null) return;
        if (trackedColliders.Add(col) && trackedColliders.Count == 1)
        {
            // primeiro collider dentro -> parar carro
            currentSpeed = 0f;
        }
    }

    private void HandleColliderExit(Collider col)
    {
        // Remove (Remove trata nulls internamente; defensivamente verifica null)
        if (col != null)
            trackedColliders.Remove(col);
        else
            trackedColliders.RemoveWhere(c => c == null);

    }
}
