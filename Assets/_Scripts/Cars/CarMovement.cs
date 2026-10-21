using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
	private const string CAR_BACK_TAG = "CarBack";
	private const string SEMAPHORE_TAG = "Semaphore";
	private const string DESPAWNER_TAG = "Despawner";
	private const string PLAYER_TAG = "Player";

	[Tooltip("Velocidade máxima (unidades por segundo)")]
	[SerializeField] private float speed = 10f;

	[Tooltip("Velocidade mínima para que encostar no player conte como atropelamento")]
	[SerializeField] private float hitSpeed = 2f;

	[Tooltip("Aceleração (unidades por segundo²) ao sair do repouso")]
	[SerializeField] private float acceleration = 5f;

	[Tooltip("Tags que contam como atropelamento")]
	[SerializeField] private string[] hitTags = { PLAYER_TAG };

	[SerializeField] private bool isStaticCar = false;

	// Colliders (traseira de outro carro / semáforo) que estão bloqueando este carro.
	private readonly HashSet<Collider> blockingColliders = new HashSet<Collider>();

	// Velocidade atual; sobe gradualmente até 'speed' (máxima)
	private float currentSpeed;

	private void Start()
	{
		currentSpeed = speed;
	}

	private void Update()
	{
		// OnTriggerExit não é chamado quando o collider é desativado/destruído (ex.: semáforo abrindo),
		// então limpamos manualmente quem não está mais ativo.
		blockingColliders.RemoveWhere(col => col == null || !col.enabled || !col.gameObject.activeInHierarchy);
	}

	private void FixedUpdate()
	{
		if (isStaticCar)
			return;

		// Parado enquanto houver colliders bloqueando; senão acelera até a máxima
		if (blockingColliders.Count > 0)
			currentSpeed = 0f;
		else
			currentSpeed = Mathf.MoveTowards(currentSpeed, speed, acceleration * Time.fixedDeltaTime);

		transform.Translate(Vector3.forward * currentSpeed * Time.fixedDeltaTime, Space.Self);
	}

	private void OnDisable()
	{
		blockingColliders.Clear();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (IsBlockingCollider(other))
		{
			blockingColliders.Add(other);
			currentSpeed = 0f;
		}

		if (other.CompareTag(DESPAWNER_TAG))
		{
			Destroy(gameObject);
			return;
		}

		if (!isStaticCar && currentSpeed >= hitSpeed && IsHitTarget(other))
			PlayerDeathManager.Instance.Kill(PlayerDeathManager.DeathCause.Car);
	}

	private void OnTriggerExit(Collider other)
	{
		if (IsBlockingCollider(other))
			blockingColliders.Remove(other);
	}

	private bool IsBlockingCollider(Collider col)
	{
		return col.CompareTag(CAR_BACK_TAG) || col.CompareTag(SEMAPHORE_TAG);
	}

	private bool IsHitTarget(Collider col)
	{
		foreach (string hitTag in hitTags)
		{
			if (col.CompareTag(hitTag))
				return true;
		}
		return false;
	}
}
