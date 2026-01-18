using UnityEngine;

public class TestScript : MonoBehaviour
{
    public int maxRPM = 480;

    public float currentRPM;
    public float wheelDiameterInches = 3.25f;    
    public float wheelRadius; //meters    
    public float stallTorque = 2.0f;

    public float v_wheel;

    public GameObject testWheel;

    public Transform wheelPos;

    public Vector3 velocityAtWheel;

    public Rigidbody rb;

    public float wheeLinearSpeed;

    public float currentTorque;

    public float forceMagnitude;

    public Vector3 forceVector;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        wheelPos = testWheel.transform;
        rb = gameObject.GetComponent<Rigidbody>();
        wheelRadius = (wheelDiameterInches * 0.0254f)/2;
    }

    // Update is called once per frame
    void FixedUpdate()
    {        
        velocityAtWheel = rb.GetPointVelocity(wheelPos.position);
        v_wheel = Vector3.Dot(velocityAtWheel, wheelPos.forward);

        currentRPM = (v_wheel/(2*Mathf.PI*wheelRadius)) * 60;

        //currentRPM = 60f;

        currentTorque = stallTorque * (1-(currentRPM/maxRPM));

        if (currentTorque < 0f)
        {
            currentTorque = 0f;
        }

        forceMagnitude = (currentTorque/wheelRadius);

        forceVector = forceMagnitude * wheelPos.forward;

        rb.AddForceAtPosition(forceVector,wheelPos.position);
    }

}
