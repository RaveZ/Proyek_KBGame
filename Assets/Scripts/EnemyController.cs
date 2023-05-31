using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public float Agression = 1;
    public float patrolRadius = 10f;
    public float chaseRadius = 20f;
    public float fieldOfViewAngle = 90f;
    public float loseSightDuration = 3f; // Time to patrol after losing sight of the player
    public float agentSpeed = 10;
    public float patrolInterval = 1f;
    public GameObject player;
    public int coinCounter;

    private NavMeshAgent agent;
    private Vector3 targetPosition;
    private bool isChasing = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = gameObject.transform.parent.GetComponentInChildren<MazeAgent>().gameObject;
        agent.speed = agentSpeed;
        SetPatrol();
    }

    public void SetSpeed(float agression)
    {
        agentSpeed = 2 * agression + 3; 
    }
    private void Update()
    {
        //coinCounter = player.GetComponent<MazeAgent>().coinCounter;
        if (!isChasing)
        {
            
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                Invoke("SetPatrol", patrolInterval);
            }
            
            
            if (CanSeePlayer())
            {
                isChasing = true;
                SetDestination(player.transform.position);
            }
        }
        else
        {
            if (!CanSeePlayer())
            {
                isChasing = false;
                Invoke("SetPatrol", loseSightDuration);
                
            }
            else
            {
                SetDestination(player.transform.position);
            }
        }
    }
    void SetPatrol()
    {
        targetPosition = GetRandomPointOnNavMesh();
        SetDestination(targetPosition);
    }
    private Vector3 GetRandomPointOnNavMesh()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas);
        return hit.position;
    }

    private void SetDestination(Vector3 destination)
    {
        agent.SetDestination(destination);
    }

    private bool CanSeePlayer()
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle <= fieldOfViewAngle * 0.5f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, chaseRadius))
            {
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        Quaternion leftRayRotation = Quaternion.AngleAxis(-fieldOfViewAngle * 0.5f, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(fieldOfViewAngle * 0.5f, Vector3.up);
        Vector3 leftRayDirection = leftRayRotation * transform.forward;
        Vector3 rightRayDirection = rightRayRotation * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, leftRayDirection * chaseRadius);
        Gizmos.DrawRay(transform.position, rightRayDirection * chaseRadius);
    }
}
