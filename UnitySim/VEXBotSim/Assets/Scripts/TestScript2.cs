using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class TestScript2 : MonoBehaviour
{
    public int maxRPM = 480;
    public float wheelDiameterInches = 3.25f;    
    public float wheelRadius; //meters    
    public float stallTorque = 2f;
    public GameObject[] wheels;

    public List<Transform> wheelPositions = new List<Transform>();


    public Rigidbody rb;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (GameObject wheel in wheels)
        {
            wheelPositions.Add(wheel.transform);
        }
        rb = gameObject.GetComponent<Rigidbody>();
        wheelRadius = (wheelDiameterInches * 0.0254f)/2;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (Transform pos in wheelPositions)
        {
            Debug.DrawRay(pos.position, pos.forward * 10f, Color.blue);
            Vector3 velocityAtWheel = rb.GetPointVelocity(pos.position);
            float v_wheel = Vector3.Dot(velocityAtWheel, pos.forward);

            float currentRPM = (v_wheel/(2*Mathf.PI*wheelRadius)) * 60;

            float currentTorque = stallTorque * (1-(currentRPM/maxRPM));

            if (currentTorque < 0f)
            {
                currentTorque = 0f;
            }

            float forceMagnitude = (currentTorque/wheelRadius);

            Vector3 forceVector = forceMagnitude * pos.forward;

            rb.AddForceAtPosition(forceVector,pos.position);
        }           
    }

    void OnDrawGizmos()
    {
        if (wheelPositions == null) return;

        for (int i = 0; i < wheelPositions.Count; i++)
        {
            Transform pos = wheelPositions[i];
            if (pos == null) continue;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(pos.position, pos.position + pos.forward * 0.5f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos.position, pos.position + pos.right * 0.5f);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(pos.position, pos.position + pos.up * 0.5f);

            #if UNITY_EDITOR
            UnityEditor.Handles.Label(pos.position, "Wheel " + i);
            #endif
        }
    }


}
