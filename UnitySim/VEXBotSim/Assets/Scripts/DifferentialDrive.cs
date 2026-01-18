using UnityEngine;

[System.Serializable]
public class Wheel
{
    public Transform wheelTransform;
    public float motorFreeRPM = 600f;
    public float motorStallTorque = 2f; // Nm
    public float wheelRadius = 0.05f; // meters
    public float gearRatio = 1f;

    [HideInInspector] public float motorAngularVelocity; // rad/s
    [HideInInspector] public float wheelLinearSpeed; // m/s
}

public class DifferentialDrive : MonoBehaviour
{
    public bool isAIControlled;
    public Rigidbody rb;
    public Wheel frontLeft;
    public Wheel frontRight;
    public Wheel rearLeft;
    public Wheel rearRight;

    [Range(-1f, 1f)] public float leftInput = 0f;  // -1 back, 1 forward
    [Range(-1f, 1f)] public float rightInput = 0f;

    void FixedUpdate()
    {
        Wheel[] leftWheels = { frontLeft, rearLeft };
        Wheel[] rightWheels = { frontRight, rearRight };

        // Apply motor physics per wheel
        float leftForce = 0f;
        float rightForce = 0f;

        foreach (Wheel w in leftWheels)
        {
            ApplyMotorPhysics(w, leftInput, out float force);
            leftForce += force;
        }

        foreach (Wheel w in rightWheels)
        {
            ApplyMotorPhysics(w, rightInput, out float force);
            rightForce += force;
        }

        // Compute total force and torque for robot
        float totalForce = leftForce + rightForce;
        float torque = (rightForce - leftForce) * 0.5f; // simplistic turning

        // Apply forward/backward velocity
        Vector3 forwardVelocity = transform.forward * (totalForce / rb.mass) * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardVelocity);

        // Apply simple turning
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, torque, 0f));
    }

    void Update()
    {
        if (isAIControlled)
            return;
        InputDebug inputDebug = FindFirstObjectByType<InputDebug>();

        float forward = inputDebug.left.y;
        forward *= -0.1f;

        float turn = inputDebug.right.x;
        turn *= -0.1f;

        leftInput  = forward + turn;
        rightInput = forward - turn;




    }

    void ApplyMotorPhysics(Wheel w, float input, out float wheelForce)
    {
        // Convert input [-1,1] to desired torque
        float appliedTorque = w.motorStallTorque * input;

        // Motor angular velocity
        float motorFreeRad = w.motorFreeRPM * 2f * Mathf.PI / 60f;
        w.motorAngularVelocity = motorFreeRad * (1f - (appliedTorque / w.motorStallTorque));
        w.wheelLinearSpeed = (w.motorAngularVelocity / w.gearRatio) * w.wheelRadius;

        // Wheel force to apply to robot
        wheelForce = appliedTorque / w.wheelRadius;
    }
}
