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
    [SerializeField] private float speed= 3f;
    [SerializeField] private float jumpPower=20f;
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
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(level[curLevel].goal.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float force = actions.ContinuousActions[2];
        
        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * speed;
        rb.AddForce(0,0,force * jumpPower);
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
        continuousActions[2] = Input.GetAxisRaw("Jump");
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
            coinCounter++;
            AddReward(0.5f*coinCounter);
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
        curLevel++;
        transform.localPosition = level[curLevel].goal.transform.localPosition;
    }

    private void newLevel()
    {
        coinCounter= 0;
    }

}
