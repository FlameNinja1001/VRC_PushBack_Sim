using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class AITest : MonoBehaviour
{
    public Transform target;
    public float speed = 1f;
    private Coroutine LookCoroutine;
    private Coroutine OuttakeCoroutine;

    public bool isRotating;

    public NavMeshAgent agent;

    public float distance;
    public float blockDistanceThreshold;
    public float goalDistanceThreshold;

    public BlockControl blockControl;    

    public bool isOuttaking;
    public float outtakeDuration;
    public float outtakeLerpDuration;

    public void StartRotating()
    {
        if (LookCoroutine != null)
        {
            StopCoroutine(LookCoroutine);
        }

        LookCoroutine = StartCoroutine(LookAt());
    }

    public void Outtake()
    {
        if (OuttakeCoroutine != null)
        {
            StopCoroutine(OuttakeCoroutine);
        }

        OuttakeCoroutine = StartCoroutine(OuttakeToGoal());
    }

    private IEnumerator LookAt()
    {
        isRotating = true;
        Quaternion lookRotation = Quaternion.LookRotation(-(target.position - transform.position));
        Vector3 euler = lookRotation.eulerAngles;
        lookRotation = Quaternion.Euler(0f, euler.y, 0f);

        float time = 0;

        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time);

            time += Time.deltaTime * speed;

            yield return null;
        }
        isRotating = false;
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
            float t = elapsed / duration;
            t = Mathf.Clamp01(t);

            transform.position = Vector3.Lerp(startPos, target.position, t);
            transform.rotation = Quaternion.Lerp(startRot, target.rotation, t);

            yield return null;
        }

        // Snap EXACT at the end
        transform.position = target.position;
        transform.rotation = target.rotation;

        if (target.gameObject.name.Contains("center"))
        {
            blockControl.outtakeToCenterBool = true;
        }
        else if (target.gameObject.name.Contains("top"))
        {
            blockControl.outtakeToTopBool = true;
        }
        else if (target.gameObject.name.Contains("bottom"))
        {
            blockControl.outtakeToBottomBool = true;
        }
        yield return new WaitForSeconds(outtakeDuration);

        isOuttaking = false;
        blockControl.outtakeToBottomBool = false;
        blockControl.outtakeToTopBool = false;
        blockControl.outtakeToCenterBool = false;
        target = null;
        yield return null;
    }

    void Start()
    {        
        target = FindNewBlockTarget();        
        agent.updateRotation = false;
        StartRotating();
    }

    void Update()
    {        
        if (target == null)
        {
            if (blockControl.blockStorage > 0)
            {
                target = FindGoalOrBlockTarget();
            }
            else if (blockControl.blockStorage == 0)
            {
                target = FindNewBlockTarget();
            }
            else if (blockControl.blockStorage == 5)
            {
                target = FindNewGoalTarget();
            }
            StartRotating();
        }
        distance = Vector3.Distance(transform.position, target.position);
        if (isRotating || isOuttaking)
        {
            agent.enabled = false;
        }
        else
        {
            agent.enabled = true;
            agent.SetDestination(target.position);
        }

        if (target.gameObject.CompareTag("RedBlock"))
        {
            if (distance < blockDistanceThreshold)
            {
                blockControl.intakeTrigger.SetActive(true);
            }
        }
        else
        {
            blockControl.intakeTrigger.SetActive(false);
        }

        if (target.gameObject.CompareTag("GoalObj"))
        {
            if (distance < goalDistanceThreshold)
            {
                if (!isOuttaking)
                {
                    Outtake();
                }
            }
        }

        if (target.IsChildOf(this.transform))
        {
            target = null;          
        }
        if (target.gameObject.name.Contains("Block") && blockControl.blockStorage == 5)
        {
            target = FindNewGoalTarget();
        }
    }

    public Transform FindNewBlockTarget()
    {
        GameObject[] allBlocks = GameObject.FindGameObjectsWithTag("RedBlock");
        float bestDist = 10000;
        float currentDist;
        Transform transformToReturn = null;
        foreach (GameObject obj in allBlocks)
        {
            if (obj.transform.IsChildOf(this.transform))
            {
                continue;
            }
            if (IsInScoreZone(obj)) 
            {
                continue; 
            }
            currentDist = Vector3.Distance(obj.transform.position, transform.position);
            if (currentDist <= bestDist)
            {
                bestDist = currentDist;
                transformToReturn = obj.transform;
            }
        }
        return transformToReturn;
    }

    public Transform FindNewGoalTarget()
    {
        GameObject[] goals = GameObject.FindGameObjectsWithTag("GoalObj");
        float bestDist = 10000;
        float currentDist;
        Transform transformToReturn = null;
        foreach (GameObject obj in goals)
        {            
            currentDist = Vector3.Distance(obj.transform.position, transform.position);
            if (currentDist <= bestDist)
            {
                bestDist = currentDist;
                transformToReturn = obj.transform;
            }
        }
        return transformToReturn;
    }

    public Transform FindGoalOrBlockTarget()
    {
        Transform goal = FindNewGoalTarget();
        Transform block = FindNewBlockTarget();

        if (block == null)
        {
            return goal;
        }

        float goalDist = Vector3.Distance(goal.transform.position, transform.position);
        float blockDist = Vector3.Distance(block.transform.position, transform.position);

        if (goalDist > blockDist)
        {
            return goal;
        }
        else
        {
            return block;
        }
    }

    public bool IsInScoreZone(GameObject obj)
    {
        ScoreZoneScript[] scoreZones = FindObjectsOfType<ScoreZoneScript>();
        foreach (ScoreZoneScript zone in scoreZones)
        {
            Collider zoneCollider = zone.GetComponent<Collider>();

            if (zoneCollider != null)
            {                
                if (zoneCollider.bounds.Contains(obj.transform.position))
                {
                    return true;
                }
            }
        }
        return false;
    }
    
}
