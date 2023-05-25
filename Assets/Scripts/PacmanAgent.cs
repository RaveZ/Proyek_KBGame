using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class PacmanAgent : Agent
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Trap"))
        {
            print("hit Trap");
            SetReward(-2f);
            EndEpisode();
        }
        

    }
}
