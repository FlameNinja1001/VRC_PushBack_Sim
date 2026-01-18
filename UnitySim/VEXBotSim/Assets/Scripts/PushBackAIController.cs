using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBackAIController : MonoBehaviour
{
    public enum Alliance { Red, Blue }
    public Alliance alliance;

    public DifferentialDrive drive;
    public BlockControl blockControl;

    // Transforms for goal locations
    public Transform center1;
    public Transform center2;
    public Transform bottom1;
    public Transform bottom2;
    public Transform topLeft1;
    public Transform topLeft2;
    public Transform topRight1;
    public Transform topRight2;

    // Park areas
    public Transform redPark;
    public Transform bluePark;

    // Tuning
    public float decisionRate = 0.25f;
    public float maxForward = 0.05f;
    public float maxTurn = 0.04f;
    public float closeEnoughDistance = 0.2f;
    public float snapDuration = 0.5f;
    public float waitAfterOuttake = 3f;
    public float groundY = -0.2196818f;
    public float groundTolerance = 0.05f;
    public float raycastDistance = 0.5f;
    public LayerMask obstacleMask;

    // Debug
    public string currentStateName = "SearchingBlock";

    enum AIState { GoingToBlock, GoingToGoal, Outtaking, Parking }
    AIState state;

    GameObject currentTargetBlock;
    Transform currentGoalTransform;
    Transform currentDriveTarget;

    float decisionTimer;
    HashSet<GameObject> skippedBlocks = new HashSet<GameObject>();

    // Banned blocks (e.g., in scoring zones)
    public List<GameObject> bannedBlocks = new List<GameObject>();

    void Update()
    {
        decisionTimer -= Time.deltaTime;
        if (decisionTimer <= 0f)
        {
            decisionTimer = decisionRate;
            Decide();
        }

        // Automatically ban blocks inside any scoring zones
        BanBlocksInScoreZones();

        if (currentTargetBlock != null && state == AIState.GoingToBlock)
        {
            Vector3 blockPos = currentTargetBlock.transform.position;
            Vector3[] rayOrigins = new Vector3[]
            {
                transform.position + Vector3.up * 0.5f,
                transform.position + Vector3.up * 0.9f,
                transform.position + Vector3.up * 1.3f
            };

            bool blocked = false;
            foreach (Vector3 origin in rayOrigins)
            {
                Vector3 dir = (blockPos - origin).normalized;
                if (Physics.Raycast(origin, dir, raycastDistance, obstacleMask))
                {
                    blocked = true;
                    break;
                }
            }

            if (blocked)
            {
                skippedBlocks.Add(currentTargetBlock);
                currentTargetBlock = null;
                currentDriveTarget = null;
                state = AIState.GoingToBlock;
                currentStateName = "SearchingBlock";
            }
        }

        if (state != AIState.Outtaking)
            DriveToTarget();
    }

    void Decide()
    {
        GameObject nearestBlock = FindNearestGroundedBlock();
        Transform nearestGoal = null;
        if (blockControl.blockStorage > 0)
            nearestGoal = FindNearestGoalTransform();

        if (nearestGoal != null && (nearestBlock == null || Vector3.Distance(transform.position, nearestGoal.position) < Vector3.Distance(transform.position, nearestBlock.transform.position)))
        {
            currentTargetBlock = null;
            currentGoalTransform = nearestGoal;
            currentDriveTarget = nearestGoal;
            state = AIState.GoingToGoal;
            currentStateName = "GoingToGoal";

            blockControl.intakeTrigger.SetActive(false);
        }
        else if (nearestBlock != null)
        {
            currentTargetBlock = nearestBlock;
            currentGoalTransform = null;
            currentDriveTarget = nearestBlock.transform;
            state = AIState.GoingToBlock;
            currentStateName = "GoingToBlock";

            blockControl.intakeTrigger.SetActive(true);
        }
        else
        {
            // No grounded blocks left → go to park area
            currentTargetBlock = null;
            currentGoalTransform = null;
            currentDriveTarget = alliance == Alliance.Red ? redPark : bluePark;
            state = AIState.Parking;
            currentStateName = "Parking";

            blockControl.intakeTrigger.SetActive(false);
        }
    }

    void DriveToTarget()
    {
        if (currentDriveTarget == null)
        {
            drive.leftInput = 0f;
            drive.rightInput = 0f;
            return;
        }

        Vector3 localTarget = transform.InverseTransformPoint(currentDriveTarget.position);
        float forward = Mathf.Clamp(localTarget.z, -1f, 1f) * maxForward;
        float turn = Mathf.Clamp(localTarget.x, -1f, 1f) * maxTurn;

        drive.leftInput = Mathf.Lerp(drive.leftInput, forward + turn, 0.1f);
        drive.rightInput = Mathf.Lerp(drive.rightInput, forward - turn, 0.1f);

        if (state == AIState.GoingToGoal && Vector3.Distance(transform.position, currentDriveTarget.position) < closeEnoughDistance)
        {
            StartCoroutine(SmoothSnapAndOuttake(currentDriveTarget));
        }
    }

    IEnumerator SmoothSnapAndOuttake(Transform goalTransform)
    {
        state = AIState.Outtaking;
        currentStateName = "Outtaking";

        blockControl.intakeTrigger.SetActive(false);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < snapDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / snapDuration);
            transform.position = Vector3.Lerp(startPos, goalTransform.position, t);
            transform.rotation = Quaternion.Lerp(startRot, goalTransform.rotation, t);
            yield return null;
        }

        transform.position = goalTransform.position;
        transform.rotation = goalTransform.rotation;

        if (goalTransform == topLeft1 || goalTransform == topLeft2 ||
            goalTransform == topRight1 || goalTransform == topRight2)
        {
            blockControl.outtakeToTopBool = true;
            blockControl.outtakeToCenterBool = false;
            blockControl.outtakeToBottomBool = false;
        }
        else if (goalTransform == center1 || goalTransform == center2)
        {
            blockControl.outtakeToTopBool = false;
            blockControl.outtakeToCenterBool = true;
            blockControl.outtakeToBottomBool = false;
        }
        else
        {
            blockControl.outtakeToTopBool = false;
            blockControl.outtakeToCenterBool = false;
            blockControl.outtakeToBottomBool = true;
        }

        yield return new WaitForSeconds(waitAfterOuttake);

        blockControl.outtakeToTopBool = false;
        blockControl.outtakeToCenterBool = false;
        blockControl.outtakeToBottomBool = false;

        state = AIState.GoingToBlock;
        currentGoalTransform = null;
        currentDriveTarget = null;
        currentStateName = "SearchingBlock";
    }

    GameObject FindNearestGroundedBlock()
    {
        string tag = alliance == Alliance.Red ? "RedBlock" : "BlueBlock";
        GameObject[] blocks = GameObject.FindGameObjectsWithTag(tag);

        GameObject closest = null;
        float bestDist = float.MaxValue;

        foreach (GameObject b in blocks)
        {
            if (skippedBlocks.Contains(b)) continue;
            if (bannedBlocks.Contains(b)) continue;

            float y = b.transform.position.y;
            if (y < groundY - groundTolerance || y > groundY + groundTolerance) continue;

            float d = Vector3.Distance(transform.position, b.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                closest = b;
            }
        }

        return closest;
    }

    Transform FindNearestGoalTransform()
    {
        Transform[] allGoals = new Transform[]
        {
            center1, center2,
            bottom1, bottom2,
            topLeft1, topLeft2,
            topRight1, topRight2
        };

        Transform closest = null;
        float bestDist = float.MaxValue;

        foreach (Transform t in allGoals)
        {
            float d = Vector3.Distance(transform.position, t.position);
            if (d < bestDist)
            {
                bestDist = d;
                closest = t;
            }
        }

        return closest;
    }

    void BanBlocksInScoreZones()
    {
        // Find all scoring zones
        ScoreZoneScript[] scoreZones = FindObjectsOfType<ScoreZoneScript>();

        foreach (ScoreZoneScript zone in scoreZones)
        {
            Collider zoneCollider = zone.GetComponent<Collider>();
            if (zoneCollider == null) continue;

            // Get all colliders overlapping with the scoring zone
            Collider[] hits = Physics.OverlapBox(
                zoneCollider.bounds.center,
                zoneCollider.bounds.extents,
                zoneCollider.transform.rotation
            );

            foreach (Collider hit in hits)
            {
                string blockTag = alliance == Alliance.Red ? "RedBlock" : "BlueBlock";
                if (hit.CompareTag(blockTag) && !bannedBlocks.Contains(hit.gameObject))
                {
                    bannedBlocks.Add(hit.gameObject);
                    Debug.Log($"Banned {hit.gameObject.name} because it's inside {zone.name}");
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (currentTargetBlock == null) return;

        Vector3 blockPos = currentTargetBlock.transform.position;
        Vector3[] rayOrigins = new Vector3[]
        {
            transform.position + Vector3.up * 0.5f,
            transform.position + Vector3.up * 0.9f,
            transform.position + Vector3.up * 1.3f
        };

        foreach (Vector3 origin in rayOrigins)
        {
            Vector3 dir = (blockPos - origin).normalized;
            Ray ray = new Ray(origin, dir);

            bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, raycastDistance, obstacleMask);

            Gizmos.color = hit ? Color.red : Color.yellow;
            Gizmos.DrawLine(origin, origin + dir * raycastDistance);

            if (hit)
            {
                Gizmos.DrawSphere(hitInfo.point, 0.02f);
            }
        }
    }
}
