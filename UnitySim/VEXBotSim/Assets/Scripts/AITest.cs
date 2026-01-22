using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class AITest : MonoBehaviour
{
    public Transform target;
    public float speed = 1f;
    private Coroutine LookCoroutine;

    public bool isRotating;

    public NavMeshAgent agent;

    public float distance;
    public float distanceThreshold;

    public BlockControl blockControl;    

    public void StartRotating()
    {
        if (LookCoroutine != null)
        {
            StopCoroutine(LookCoroutine);
        }

        LookCoroutine = StartCoroutine(LookAt());
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

    void Start()
    {        
        target = FindNewBlockTarget();        
        agent.updateRotation = false;
        StartRotating();
    }

    void Update()
    {
        distance = Vector3.Distance(transform.position, target.position);
        if (isRotating)
        {
            agent.enabled = false;
        }
        else
        {
            agent.enabled = true;
            agent.SetDestination(target.position);
        }

        if (distance < distanceThreshold)
        {
            blockControl.intakeTrigger.SetActive(true);
        }
        if (target.IsChildOf(this.transform))
        {
            target = FindNewBlockTarget();  
            StartRotating();          
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
