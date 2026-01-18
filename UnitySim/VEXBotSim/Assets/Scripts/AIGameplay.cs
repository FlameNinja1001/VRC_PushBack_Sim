using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class AIGameplay : MonoBehaviour
{
    public Transform target; 
    public NavMeshAgent agent;
    public float rotationSpeed = 5f;

    private bool reachedTarget = false;

    public string currentState = "LookForBlock";

    public List<GameObject> viableBlockList = new List<GameObject>();

    void Update()
    {
        agent.SetDestination(target.position);
        // Find all blocks tagged "RedBlock"
        GameObject[] allBlocks = GameObject.FindGameObjectsWithTag("RedBlock");

        // Filter based on y position
        viableBlockList.Clear();
        foreach (GameObject block in allBlocks)
        {
            if (block.transform.position.y < -0.1f && block.transform.position.y > -0.3f)
            {
                viableBlockList.Add(block);
            }
        }

        // Example of setting agent active
        if (currentState == "LookForBlock")
        {
            if (!agent.enabled) agent.enabled = true;
            target = findClosestBlock();
        }        
    }

    // Returns the closest block from the viable list
    Transform findClosestBlock()
    {
        if (viableBlockList.Count == 0) return null;

        GameObject closest = viableBlockList[0];
        float minDistance = Vector3.Distance(transform.position, closest.transform.position);

        foreach (GameObject block in viableBlockList)
        {
            float dist = Vector3.Distance(transform.position, block.transform.position);
            if (dist < minDistance)
            {
                closest = block;
                minDistance = dist;
            }
        }

        return closest.transform;
    }
}
