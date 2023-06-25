using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private float sedikit;
    private float sedang;
    private float banyak;
    private float dekat;
    private float jauh;
    public int maxCoin;
    private NavMeshAgent agent;
    private Vector3 targetPosition;
    private bool isChasing = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = gameObject.transform.parent.GetComponentInChildren<MazeAgent>().gameObject;
        maxCoin = gameObject.transform.parent.GetComponentsInChildren<Transform>().Where(coin => coin.CompareTag("Coin")).Select(coin => coin.gameObject).ToArray().Length;
        agent.speed = agentSpeed;
        SetPatrol();
    }

    public void SetAgentSpeed(float agression)
    {
        agentSpeed = 2 * agression + 3;
        agent.speed = agentSpeed;
    }
    private void Update()
    {
        coinCounter = player.GetComponent<MazeAgent>().coinCounter;

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
                print("agent Detected");
                SetDestination(player.transform.position);
            }
        }
        CalculateFuzzy(coinCounter);
        CalculateAggression();

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

    private void CalculateFuzzy(float _coinCollected)
    {
        //sedikit
        if(_coinCollected <= maxCoin / 2)
        {
            sedikit = (-2 * _coinCollected)/maxCoin + 1;
        }else if (_coinCollected > maxCoin / 2)
        {
            sedikit = 0;
        }

        //sedang
        if (_coinCollected <= maxCoin / 2)
        {
            sedang = (2 * _coinCollected) / maxCoin;
        }
        else if (_coinCollected > maxCoin / 2)
        {
            sedang = 2 - (2 * _coinCollected / maxCoin);
        }

        //banyak
        if (_coinCollected >= maxCoin / 2)
        {
            banyak = (2 * _coinCollected) / maxCoin - 1;
        }
        else if (_coinCollected < maxCoin / 2)
        {
            banyak = 0;
        }

        if(agent.remainingDistance <= 7)
        {
            dekat = 1;
            jauh = 0;
        }else if (agent.remainingDistance > 7 && agent.remainingDistance <= 8)
        {
            dekat = 8 - agent.remainingDistance;
            jauh = agent.remainingDistance - 7;
        }else if(agent.remainingDistance > 8)
        {
            dekat = 0;
            jauh = 1;
        }
    }

    private void CalculateAggression()
    {
        var w6 = Mathf.Min(banyak, dekat);
        var w5 = Mathf.Min(banyak, jauh);
        var w4 = Mathf.Min(sedang, dekat);
        var w3 = Mathf.Min(sedang, jauh);
        var w2 = Mathf.Min(sedikit, dekat);
        var w1 = Mathf.Min(sedikit, jauh);
        var z1 = 3 - 2 * w1;
        var z2 = (2.5 * w2 + 0.5 + ((7 - w2) / 2)) / 2;
        var z3 = (2.5 * w3 + 0.5 + ((7 - w3) / 2)) / 2;
        var z4 = (2.5 * w4 + 2 + 5 - (w4 / 2)) / 2;
        var z5 = (2.5 * w5 + 2 + 5 - (w5 / 2)) / 2;
        var z6 = 2 * w6 + 4;

        var a = (float) (w1*z1 + z2*w2 + z3*w3 + z4*w4 + z5*w5 + z6*w6) / (w1 + w2 + w3 + w4 + w5 + w6);
        patrolInterval =  (17 - 2 * a) / 5;
        SetAgentSpeed(a);
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
                if (hit.collider.transform.parent.gameObject.CompareTag("Player"))
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
