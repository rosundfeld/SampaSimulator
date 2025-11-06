using System.Collections;
using UnityEngine;

public class CarMoviment : MonoBehaviour
{
	[Tooltip("Unidades por segundo")]
	public float speed = 10f;

	[Tooltip("Tempo (s) até destruir automaticamente. <= 0 desativa destruição automática")]
	public float despawnTime = 5f;

	public float remainingTime;

	void Start()
	{
		remainingTime = despawnTime;
	}

	void FixedUpdate()
	{
		// Movimento frame-rate independent
		transform.Translate(Vector3.forward * speed * Time.fixedDeltaTime, Space.Self);
	}


	void Update()
	{
		// Contador simples de despawn
		if (remainingTime > 0f)
		{
			remainingTime -= Time.deltaTime;
			if (remainingTime <= 0f)
				Destroy(gameObject);
		}
	}

	public void DespawnNow()
	{
		Destroy(gameObject);
	}
}
