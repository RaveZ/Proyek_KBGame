using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using Unity.VisualScripting;

public class MazeAgent : Agent
{
    [Serializable]
    class Level
    {
        public Transform[] spawnLoc;
        public Transform goal;
        public GameObject[] coins;
        public GameObject[] Enemies;

    }

    [SerializeField] private Level level;
    [SerializeField] private float speed= 1f;
    [SerializeField] private float jumpPower=10f;
    [SerializeField] private int sightDistance;
    private bool isJumping=false;
    public int coinCounter = 0;
    public Vector3[] offsetRay = new Vector3[4];
    //public GameObject[] coins;
    // Start is called before the first frame update
    void Start()
    {
        level.goal = gameObject.transform.parent.GetComponentsInChildren<Transform>().Where(coin => coin.CompareTag("Goal")).ToArray()[0];
        level.spawnLoc = gameObject.transform.parent.GetComponentsInChildren<Transform>().Where(coin => coin.CompareTag("Spawn")).ToArray();
        level.coins = gameObject.transform.parent.GetComponentsInChildren<Transform>().Where(coin => coin.CompareTag("Coin")).Select(coin => coin.gameObject).ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -5)
        {
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        int random = Random.Range(0,level.spawnLoc.Length);
        transform.localPosition = level.spawnLoc[random].localPosition;
        coinCounter = 0;
        foreach(GameObject coin in level.coins)
        {
            coin.SetActive(true);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(level.goal.transform.localPosition);
        foreach(Transform loc in level.spawnLoc)
        {
            sensor.AddObservation(loc.localPosition);
        }
        foreach(GameObject coin in level.coins)
        {
            sensor.AddObservation(coin.transform.localPosition);
        }

        SensorRaycast(sensor, transform.forward, offsetRay[0]); // depan
        SensorRaycast(sensor, -transform.forward, offsetRay[1]); // belakang
        SensorRaycast(sensor, transform.right, offsetRay[2]); // kanan
        SensorRaycast(sensor, -transform.right, offsetRay[3]); // kiri
    }

    private void SensorRaycast(VectorSensor sensor, Vector3 direction, Vector3 offset)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + offset, direction, out hit, sightDistance))
        {
            if (hit.collider.transform.CompareTag("Wall"))
            {
                sensor.AddObservation(-1f);
            }
            if (hit.collider.transform.CompareTag("Trap"))
            {
                sensor.AddObservation(-1f);
            }
            if(hit.collider.transform.CompareTag("Coin"))
            {
                sensor.AddObservation(1f);
            }
            if (hit.collider.transform.CompareTag("Goal"))
            {
                sensor.AddObservation(1f);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float force = actions.ContinuousActions[2];
        
        var rb = GetComponent<Rigidbody>();
        //rb.velocity = new Vector3(moveX, 0, moveZ)* speed;
        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * speed;
        if(force > 0)
        {
            isJumping = true;
        }
        if(!isJumping)
        {
            if(force > 0)
            {
                rb.AddForce(0, force * jumpPower, 0);
/*                Debug.Log(force + "force");*/
            }
            
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
        continuousActions[2] = Math.Abs(Input.GetAxisRaw("Jump"));
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.gameObject.CompareTag("Goal"))
        {
            goal();
        }
        if (other.collider.gameObject.CompareTag("Trap"))
        {
            die();
        }
        if (other.collider.gameObject.CompareTag("Coin"))
        {
            Debug.Log("coin??");
            other.gameObject.SetActive(false);
            coinCounter++;
            AddReward(1f* (coinCounter / level.coins.Length));
        }
        if (other.collider.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
        }
        if (other.collider.gameObject.CompareTag("Wall"))
        {
            print("wall");
            AddReward(-1f);
        }
    }

    private void die()
    {
        AddReward(-1f);
        EndEpisode();
    }

    private void goal()
    {
        Debug.Log("goals");
        if (level.coins.Length > 0)
        {
            AddReward(1f * (coinCounter / level.coins.Length));
        }
        else
        {
            AddReward(1f);
        }
        coinCounter = 0;

        EndEpisode();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + offsetRay[0], transform.forward * sightDistance);
        Gizmos.DrawRay(transform.position + offsetRay[1], -transform.forward * sightDistance);
        Gizmos.DrawRay(transform.position + offsetRay[2], transform.right * sightDistance);
        Gizmos.DrawRay(transform.position + offsetRay[3], -transform.right * sightDistance);
    }

}
