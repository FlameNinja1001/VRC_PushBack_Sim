using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class AITest : MonoBehaviour
{
    [Header("Targeting & Movement")]
    public Transform target;
    public float speed = 1f;
    public NavMeshAgent agent;

    [Header("Distance Thresholds")]
    public float distance;
    public float blockDistanceThreshold;
    public float goalDistanceThreshold;

    [Header("Outtake Settings")]
    public BlockControl blockControl;
    public bool isOuttaking;
    public float outtakeDuration;
    public float outtakeLerpDuration;

    [Header("Tag Settings")]
    public string tag = "RedBlock";

    [Header("Rotation Settings")]
    public bool isRotating;
    private Coroutine LookCoroutine;
    private Coroutine OuttakeCoroutine;

    [Header("NavMesh Corner Tracking")]
    private int currentCornerIndex = 1; // start at first corner after agent position
    public float cornerReachThreshold = 0.1f;

    void Start()
    {
        agent.updateRotation = false; // never use built-in rotation
        target = FindNewBlockTarget();
        if (target != null) StartRotating();
    }

    void Update()
    {
        if (target == null)
        {
            if (blockControl.blockStorage == 0)
                target = FindNewBlockTarget();
            else if (blockControl.blockStorage > 0)
                target = FindGoalOrBlockTarget();
            else if (blockControl.blockStorage == 5)
                target = FindNewGoalTarget();

            currentCornerIndex = 1;
            if (target != null) StartRotating();
        }

        if (target == null) return;

        distance = Vector3.Distance(transform.position, target.position);

        // Enable/disable NavMesh movement
        if (isRotating || isOuttaking)
            agent.enabled = false;
        else
        {
            agent.enabled = true;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(target.position, out hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }

        }

        // Handle intake trigger for blocks
        if (target.CompareTag(tag))
        {
            if (distance < blockDistanceThreshold)
                blockControl.intakeTrigger.SetActive(true);
        }
        else
        {
            blockControl.intakeTrigger.SetActive(false);
        }

        // Handle outtake toward goal
        if (target.CompareTag("GoalObj"))
        {
            if (distance < goalDistanceThreshold && !isOuttaking)
                Outtake();
        }
        // If full storage, switch target to goal
        if (target.name.Contains("Block") && blockControl.blockStorage == 5)
        {
            target = FindNewGoalTarget();
            currentCornerIndex = 1;
            StartRotating();
        }

        // Avoid targeting child objects
        if (target.IsChildOf(this.transform))
        {
            target = null;
        }        

        // --- ROTATE AT NAVMESH CORNERS ---
        if (agent.enabled && agent.hasPath && !isRotating)
        {
            Vector3[] corners = agent.path.corners;

            if (currentCornerIndex < corners.Length)
            {
                Vector3 corner = corners[currentCornerIndex];
                float distToCorner = Vector3.Distance(transform.position, corner);

                if (distToCorner < cornerReachThreshold)
                {
                    currentCornerIndex++;
                    if (currentCornerIndex < corners.Length)
                    {
                        target.position = corners[currentCornerIndex]; // temp target for LookAt
                        StartRotating();
                    }
                }
            }
        }
    }

    // --------------------------
    // ROTATION
    // --------------------------
    public void StartRotating()
    {
        if (LookCoroutine != null)
            StopCoroutine(LookCoroutine);

        LookCoroutine = StartCoroutine(LookAt());
    }

    private IEnumerator LookAt()
    {
        if (target == null) yield break;

        isRotating = true;

        Quaternion lookRotation = Quaternion.LookRotation(-(target.position - transform.position));
        Vector3 euler = lookRotation.eulerAngles;
        lookRotation = Quaternion.Euler(0f, euler.y, 0f);

        float time = 0f;

        while (time < 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time);
            time += Time.deltaTime * speed;
            yield return null;
        }
        
        transform.rotation = lookRotation; // snap exactly
        isRotating = false;
    }

    // --------------------------
    // OUTTAKE
    // --------------------------
    public void Outtake()
    {
        if (OuttakeCoroutine != null)
            StopCoroutine(OuttakeCoroutine);

        OuttakeCoroutine = StartCoroutine(OuttakeToGoal());
    }

    private IEnumerator OuttakeToGoal()
    {
        isOuttaking = true;
        blockControl.intakeTrigger.SetActive(true);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float elapsed = 0f;
        float duration = outtakeLerpDuration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.position = Vector3.Lerp(startPos, target.position, t);
            transform.rotation = Quaternion.Lerp(startRot, target.rotation, t);

            yield return null;
        }

        transform.position = target.position;
        transform.rotation = target.rotation;

        if (target.name.Contains("center"))
            blockControl.outtakeToCenterBool = true;
        else if (target.name.Contains("top"))
            blockControl.outtakeToTopBool = true;
        else if (target.name.Contains("bottom"))
            blockControl.outtakeToBottomBool = true;

        yield return new WaitForSeconds(outtakeDuration);

        isOuttaking = false;
        blockControl.outtakeToBottomBool = false;
        blockControl.outtakeToTopBool = false;
        blockControl.outtakeToCenterBool = false;

        target = null;
        yield return null;
    }

    // --------------------------
    // TARGETING
    // --------------------------
    public Transform FindNewBlockTarget()
    {
        GameObject[] allBlocks = GameObject.FindGameObjectsWithTag(tag);
        float bestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject obj in allBlocks)
        {
            if (obj.transform.IsChildOf(this.transform)) continue;
            if (IsInScoreZone(obj)) continue;

            float dist = Vector3.Distance(obj.transform.position, transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = obj.transform;
            }
        }

        return closest;
    }

    public Transform FindNewGoalTarget()
    {
        GameObject[] goals = GameObject.FindGameObjectsWithTag("GoalObj");
        float bestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject obj in goals)
        {
            float dist = Vector3.Distance(obj.transform.position, transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = obj.transform;
            }
        }

        return closest;
    }

    public Transform FindGoalOrBlockTarget()
    {
        Transform goal = FindNewGoalTarget();
        Transform block = FindNewBlockTarget();

        if (block == null) return goal;

        float goalDist = Vector3.Distance(goal.position, transform.position);
        float blockDist = Vector3.Distance(block.position, transform.position);

        return (goalDist > blockDist) ? goal : block;
    }

    public bool IsInScoreZone(GameObject obj)
    {
        foreach (var zone in ScoreZoneScript.AllZones)
        {
            if (zone == obj)
                return true;
        }
        return false;
    }
}
