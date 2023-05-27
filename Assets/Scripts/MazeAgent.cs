using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;

public class MazeAgent : Agent
{
    [SerializeField] private GameObject[] level;
    //[SerializeField] private Transform[] checkPoint;
    //[SerializeField] private Transform[] exitDoor;
    [SerializeField] private float speed= 3f;
    [SerializeField] private float jumpPower=20f;
    [SerializeField] private int curLevel = 0;
    private GameObject[] coins;
    private int coinCounter = 0;

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(level[curLevel].ToString());
        var obj = level[curLevel].transform.GetComponentsInChildren<GameObject>();
        coins = obj.Where(child => child.tag == "Coin").ToArray();
        Debug.Log("Coin Count: "+ coins.Length);
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnEpisodeBegin()
    {
        var thisCheckPoint = level[curLevel].GetComponentsInChildren<GameObject>().Where(child => child.tag == "Checkpoint").ToArray()[0];
        transform.localPosition = thisCheckPoint.transform.localPosition;
        coinCounter = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        //sensor.AddObservation(exitDoor.localPosition);
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
        if (other.gameObject.CompareTag("Trap"))
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
        var thisCheckPoint = level[curLevel].GetComponentsInChildren<GameObject>().Where(child => child.tag == "Goal").ToArray()[0];
        transform.localPosition = thisCheckPoint.transform.localPosition;

    }
    private void newLevel()
    {
        coins = GameObject.FindGameObjectsWithTag("Coin");
        Debug.Log("Coin Count: " + coins.Length);
        coinCounter= 0;
    }
}
