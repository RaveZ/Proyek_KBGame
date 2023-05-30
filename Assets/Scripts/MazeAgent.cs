using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;
using System;

public class MazeAgent : Agent
{
    [Serializable]
    class Level
    {
        public Transform spawn;
        public Transform goal;
        public GameObject[] coins; 

    }

    [SerializeField] private Level[] level;
    [SerializeField] private float speed= 1f;
    [SerializeField] private float jumpPower=10f;
    [SerializeField] private int sightDistance;
    private bool isJumping=false;
    [SerializeField] private int curLevel = 0;
    private int coinCounter = 0;

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = level[curLevel].spawn.localPosition;
        coinCounter = 0;
        foreach(GameObject coin in level[curLevel].coins)
        {
            coin.SetActive(true);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.velocity.normalized);
        for (int i = 0; i < level.Length; i++)
        {
            sensor.AddObservation(level[i].goal.localPosition);
            foreach (GameObject coin in level[i].coins)
            {
                sensor.AddObservation(coin.transform.localPosition);
            }
        }

        SensorRaycast(sensor, transform.forward);
        SensorRaycast(sensor, -transform.forward);
        SensorRaycast(sensor, transform.right);
        SensorRaycast(sensor, -transform.right);
    }

    private void SensorRaycast(VectorSensor sensor, Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, sightDistance))
        {
            if (hit.transform.CompareTag("Wall"))
            {
                sensor.AddObservation(-0.5f);
            }
            if (hit.transform.CompareTag("Trap"))
            {
                sensor.AddObservation(-1f);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float force = actions.ContinuousActions[2];
        
        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * speed;
        if(force > 0)
        {
            isJumping = true;
        }
        if(!isJumping)
        {
            rb.AddForce(0, force * jumpPower, 0);
            Debug.Log(force * jumpPower);
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
        if (other.gameObject.CompareTag("Goal"))
        {
            goal();
        }
        if (other.gameObject.CompareTag("Trap") || other.gameObject.CompareTag("Wall"))
        {
            die();
        }
        if (other.gameObject.CompareTag("Coin"))
        {
            other.gameObject.SetActive(false);
            coinCounter++;
            AddReward(1f*coinCounter);
        }
        if (other.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
        }
    }

    private void die()
    {
        AddReward(-20f);
        EndEpisode();
    }

    private void goal()
    {
        AddReward(2f * coinCounter);
        coinCounter = 0;
        curLevel++;
        transform.localPosition = level[curLevel].goal.transform.localPosition;
    }

}
