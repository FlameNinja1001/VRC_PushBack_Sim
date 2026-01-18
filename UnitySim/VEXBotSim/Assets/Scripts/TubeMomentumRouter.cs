using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TubeMomentumTransfer : MonoBehaviour
{
    [Header("Momentum Settings")]
    [Range(0f, 1f)]
    public float transferFactor = 0.95f;   // how much momentum passes forward
    public float minSpeed = 0.02f;

    // Tube axis = RED ARROW of this cube
    Vector3 Axis => transform.right.normalized;

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody incoming = collision.rigidbody;
        if (!incoming) return;

        Vector3 axis = Axis;

        float speedAlongAxis = Vector3.Dot(incoming.linearVelocity, axis);
        if (Mathf.Abs(speedAlongAxis) < minSpeed)
            return;

        Vector3 transferDir = Mathf.Sign(speedAlongAxis) * axis;
        float speed = Mathf.Abs(speedAlongAxis);

        foreach (ContactPoint cp in collision.contacts)
        {
            Rigidbody target = cp.otherCollider.attachedRigidbody;
            if (!target || target == incoming)
                continue;

            // Ensure this is a forward contact, not floor/wall
            Vector3 toTarget = (target.worldCenterOfMass - incoming.worldCenterOfMass).normalized;
            if (Mathf.Abs(Vector3.Dot(toTarget, axis)) < 0.7f)
                continue;

            // --- MOMENTUM TRANSFER ---
            target.linearVelocity += transferDir * speed * transferFactor;

            // --- FIX A: prevent recoil (no reverse velocity) ---
            Vector3 v = incoming.linearVelocity;
            float along = Vector3.Dot(v, axis);
            if (Mathf.Sign(along) != Mathf.Sign(speedAlongAxis))
            {
                v -= Vector3.Project(v, axis);
                incoming.linearVelocity = v;
            }

            // --- FIX B: kill vertical pop-up for this frame ---
            Vector3 tv = target.linearVelocity;
            tv.y = 0f;
            target.linearVelocity = tv;

            break;
        }
    }
}
