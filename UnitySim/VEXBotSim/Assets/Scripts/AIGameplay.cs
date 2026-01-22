using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class AIGameplay : MonoBehaviour
{
    public Transform target;
    public NavMeshAgent agent;

    public float rotationSpeed = 6f;
    public float curveStrength = 0.6f;

    public string currentState = "LookForBlock"; // current state
    private string previousState = "";           // for detecting state entry

    public List<GameObject> viableBlockList = new List<GameObject>();
    public GameObject[] bannedBlocks;
    public Transform[] goals;

    private BlockControl blockControl;

    void Start()
    {
        agent.updateRotation = false; // rotate manually
        agent.updateUpAxis = false;   // flat / 2.5D movement
        blockControl = GetComponent<BlockControl>();
    }

    void Update()
    {
        // Handle state entry
        if (currentState != previousState)
        {
            OnStateEnter(currentState);
            previousState = currentState;
        }

        // Outtake state: snap to target and set blockControl flags
        if (currentState == "Outtake")
        {
            agent.enabled = false;
            transform.position = target.position;
            transform.rotation = target.rotation;

            if (target == goals[0] || target == goals[1] || target == goals[2] || target == goals[3])
                blockControl.outtakeToTopBool = true;
            else if (target == goals[4] || target == goals[5])
            {
                blockControl.outtakeToBottomBool = true;
                blockControl.intakeTrigger.SetActive(false);
            }
            else if (target == goals[6] || target == goals[7])
                blockControl.outtakeToCenterBool = true;
            else
                Debug.LogWarning("Outtake target not recognized!");

            return; // stop all other movement/rotation this frame
        }

        // ---------- FIND BLOCKS ----------
        GameObject[] allBlocks = GameObject.FindGameObjectsWithTag("RedBlock");
        viableBlockList.Clear();

        foreach (GameObject block in allBlocks)
        {
            if (block.transform.position.y < -0.1f && block.transform.position.y > -0.3f)
            {
                if (System.Array.IndexOf(blockControl.currentBlocks, block) == -1 &&
                    System.Array.IndexOf(bannedBlocks, block) == -1)
                {
                    viableBlockList.Add(block);
                }
            }
        }

        // ---------- Decide next state ----------
        if (blockControl.blockStorage > 0)
        {
            // Only switch to goal if carrying a block and goal is closer than nearest block
            Transform closestBlock = FindClosestBlock();
            Transform closestGoal = FindClosestGoal();

            if (closestGoal != null &&
                (closestBlock == null || 
                 Vector3.Distance(transform.position, closestBlock.position) >
                 Vector3.Distance(transform.position, closestGoal.position)))
            {
                currentState = "LookForGoal";
                target = closestGoal;
            }
        }
        else
        {
            currentState = "LookForBlock";
            target = FindClosestBlock();
            blockControl.outtakeToTopBool = false;
            blockControl.outtakeToBottomBool = false;
            blockControl.outtakeToCenterBool = false;
        }

        if (target == null) return;

        // ---------- NAVMESH PATH ----------
        if (agent.enabled)
            agent.SetDestination(target.position);

        // ---------- CURVED MOVEMENT ----------
        Vector3 desired = agent.desiredVelocity;
        if (desired.sqrMagnitude > 0.001f)
        {
            desired.Normalize();
            Vector3 side = Vector3.Cross(Vector3.up, desired);
            Vector3 curvedDir = (desired + side * curveStrength).normalized;
            transform.position += curvedDir * agent.speed * Time.deltaTime;
        }

        // ---------- ROTATION ----------
        if (currentState == "LookForBlock")
        {
            Vector3 lookDir = target.position - transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(-lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
        else if (currentState == "LookForGoal")
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, rotationSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) < 0.5f)
            {
                currentState = "Outtake";
            }
        }

        // ---------- REACHED DESTINATION (optional trigger) ----------
        if (currentState == "LookForGoal" && ReachedDestination())
        {
            blockControl.intakeTrigger.SetActive(true);
        }
    }

    void OnStateEnter(string state)
    {
        if (state == "LookForBlock")
        {
            agent.enabled = true;
            target = FindClosestBlock();
        }
        else if (state == "LookForGoal")
        {
            target = FindClosestGoal();
        }
        else if (state == "Outtake")
        {
            agent.enabled = false;
        }
    }

    // ---------- CLOSEST BLOCK ----------
    Transform FindClosestBlock()
    {
        if (viableBlockList.Count == 0) return null;

        GameObject closest = viableBlockList[0];
        float minDist = Vector3.Distance(transform.position, closest.transform.position);

        foreach (GameObject block in viableBlockList)
        {
            float dist = Vector3.Distance(transform.position, block.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = block;
            }
        }

        return closest.transform;
    }

    // ---------- CLOSEST GOAL ----------
    Transform FindClosestGoal()
    {
        if (goals.Length == 0) return null;

        Transform closest = goals[0];
        float minDist = Vector3.Distance(transform.position, closest.position);

        foreach (Transform goal in goals)
        {
            float dist = Vector3.Distance(transform.position, goal.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = goal;
            }
        }

        return closest;
    }

    // ---------- CHECK IF REACHED DESTINATION ----------
    bool ReachedDestination()
    {
        if (agent.pathPending) return false;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                return true;
        }

        return false;
    }
}
