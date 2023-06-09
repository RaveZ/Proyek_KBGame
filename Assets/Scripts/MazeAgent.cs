using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class MazeAgent : Agent
{
    public float trapReward;
    public float wallReward;
    [Serializable]
    class Level
    {
        public Transform[] spawnLoc;
        public Transform goal;
        public GameObject[] coins;

    }
    public GameObject[] enemies;
    private Vector3[] enemiesStart;
    [SerializeField] private Level level;
    [SerializeField] private float speed= 1f;
    [SerializeField] private float jumpPower=10f;
    [SerializeField] private int sightDistance;
    private bool isJumping=false;
    public int coinCounter = 0;
    public Vector3[] offsetRay = new Vector3[4];
    public int maxHealth = 3;
    private int health = 3;
    private Material material;
    
    // Start is called before the first frame update
    void Start()
    {

        enemies = gameObject.transform.parent.GetComponentsInChildren<EnemyController>().Select(enemy => enemy.gameObject).ToArray();
        enemiesStart = enemies.Select(enemy => enemy.transform.localPosition).ToArray();
        level.goal = gameObject.transform.parent.GetComponentsInChildren<Transform>().Where(coin => coin.CompareTag("Goal")).ToArray()[0];
        level.spawnLoc = gameObject.transform.parent.GetComponentsInChildren<Transform>().Where(coin => coin.CompareTag("Spawn")).ToArray();
        level.coins = gameObject.transform.parent.GetComponentsInChildren<Transform>().Where(coin => coin.CompareTag("Coin")).Select(coin => coin.gameObject).ToArray();
        material = GetComponentsInChildren<Renderer>()[0].material;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -5)
        {
            EndEpisode();
        }
/*        if (StepCount - 1 == MaxStep)
        {

            die();
        }*/
    }

    public override void OnEpisodeBegin()
    {
        health = maxHealth;
        coinCounter = 0;
        material.color = Color.green;
        int random = Random.Range(0,level.spawnLoc.Length);
        transform.localPosition = level.spawnLoc[random].localPosition;
        transform.rotation = Quaternion.identity;
        coinCounter = 0;
        for(int i = 0; i < enemies.Length; i++)
        {
            enemies[i].transform.localPosition = enemiesStart[i];
        }
        foreach(GameObject coin in level.coins)
        {
            coin.SetActive(true);
            coin.transform.parent.gameObject.SetActive(true);
        }
        print("Begin");
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.localRotation.y);
        sensor.AddObservation(level.goal.transform.localPosition);
        foreach(GameObject enemy in enemies)
        {
            sensor.AddObservation(enemy.transform.localPosition);
        }
        sensor.AddObservation(health);
        sensor.AddObservation(isJumping);
        sensor.AddObservation(coinCounter);
/*        foreach(Transform loc in level.spawnLoc)
        {
            sensor.AddObservation(loc.localPosition);
        }*/
        foreach(GameObject coin in level.coins)
        {
            if (coin.activeSelf == true)
            {
                sensor.AddObservation(coin.transform.localPosition);
            }
        }

        SensorRaycast(sensor, transform.forward, offsetRay[0]); // depan
        SensorRaycast(sensor, -transform.forward, offsetRay[1]); // belakang
        SensorRaycast(sensor, transform.right, offsetRay[2]); // kanan
        SensorRaycast(sensor, -transform.right, offsetRay[3]); // kiri
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 0.8f, transform.forward, 3.5f);
        foreach (RaycastHit q in hits)
        {
            if (q.collider.transform.CompareTag("Coin"))
            {
                sensor.AddObservation(1f);
            }

        }
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
            if (hit.collider.transform.CompareTag("Enemy"))
            {
                sensor.AddObservation(-1f);
            }
        }
        
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float rotate = actions.ContinuousActions[0];
        //float moveZ = actions.ContinuousActions[1];
        float force = Math.Abs(actions.ContinuousActions[1]);
        
        var rb = GetComponent<Rigidbody>();
        transform.localPosition += transform.right * Time.deltaTime * speed;
        
        if(rotate >= 0.5f)
        {
            transform.Rotate(new Vector3(0, 90f, 0));
        }else if(rotate <= -0.5f)
        {
            transform.Rotate(new Vector3(0, -90f, 0));
        }

        if(!isJumping && force >= 0.5f)
        {
            isJumping = true;
            rb.AddForce(0, jumpPower, 0);
        }
    }
    

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Vertical"); //depan belakang
        //continuousActions[1] = Input.GetAxisRaw("Horizontal"); //Kiri kanan
        continuousActions[1] = Input.GetAxisRaw("Jump");
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.gameObject.CompareTag("Wall"))
        {
            die();
            /*AddReward(-0.05f);*/
        }
        if (other.collider.gameObject.CompareTag("Goal"))
        {
            goal();
        }
        if (other.collider.gameObject.CompareTag("Trap"))
        {
            gotHit();
        }

        if (other.collider.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
        }
        if (other.collider.gameObject.CompareTag("Enemy"))
        {
            die();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Collider>().gameObject.CompareTag("Coin"))
        {
            other.gameObject.SetActive(false);
            coinCounter++;
            AddReward(1f);
        }
    }
    private void gotHit()
    {
        AddReward(trapReward);
        health -= 1;
        if(health == 2)
        {
            material.color = Color.yellow;
        }else if (health == 1) 
        {
            material.color = Color.red;
        }else if( health <=0)
        {
            die();
        }
    }
    private void die()
    {
        AddReward(-1f);
        EndEpisode();
    }

    private void goal()
    {
        if (health == 3)
        {
            AddReward(3f);
        }
        else if (health == 2)
        {
            AddReward(2f);
        }
        else if (health == 1)
        {
            AddReward(1f);
        }
        coinCounter = 0;
        EndEpisode();
        /*if (coinCounter >= (int)(level.coins.Length*0.5))
        {
            
        }
        else
        {
            die();
        }*/
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + offsetRay[0], transform.forward * sightDistance);
        Gizmos.DrawRay(transform.position + offsetRay[1], -transform.forward * sightDistance);
        Gizmos.DrawRay(transform.position + offsetRay[2], transform.right * sightDistance);
        Gizmos.DrawRay(transform.position + offsetRay[3], -transform.right * sightDistance);
        Gizmos.DrawWireSphere(transform.position, 3.5f);

    }

}
