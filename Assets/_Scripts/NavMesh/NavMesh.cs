using UnityEngine;
using UnityEngine.AI;

public class NavMesh : MonoBehaviour
{
    [SerializeField] private float wanderRadius = 20f;
    [SerializeField] private float wanderInterval = 5f;
    private NavMeshAgent navMeshAgent;
    private float wanderTimer;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        wanderTimer += Time.deltaTime;

        // picks a new random point once the interval elapses or the agent arrives at its destination
        if (wanderTimer >= wanderInterval || (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance))
        {
            wanderTimer = 0f;
            SetNewWanderDestination();
        }
    }

    private void SetNewWanderDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out UnityEngine.AI.NavMeshHit hit, wanderRadius, UnityEngine.AI.NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
        }
    }
}
