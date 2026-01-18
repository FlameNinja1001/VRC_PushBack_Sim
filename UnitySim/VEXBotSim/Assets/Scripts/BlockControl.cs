using UnityEngine;

public class BlockControl : MonoBehaviour
{
    public GameObject intakeTrigger;
    public Transform[] targetAreas;
    public Transform outtakeToTopTransform;
    public Transform outtakeToCenterTransform;
    public Transform outtakeToBottomTransform;
    public GameObject[] currentBlocks;

    public float outtakeTopForceMagnitude = 5.0f;
    public float outtakeBottomForceMagnitude = 5.0f;
    public float outtakeCenterForceMagnitude = 5.0f;

    public float timer = 0;
    public float timeSetter = 0.5f;

    public int blockStorage;
    public bool outtakeToTopBool;
    public bool outtakeToCenterBool;
    public bool outtakeToBottomBool;
    public bool isIntakeActiveBool;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        isIntakeActiveBool = intakeTrigger.activeSelf;
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            timer = 0;
        }
        
        InputDebug inputDebug = FindFirstObjectByType<InputDebug>();
        if (inputDebug.l1_button)
        {
            intakeTrigger.SetActive(!intakeTrigger.activeSelf);
            outtakeToBottomBool = false;
        }
        if (inputDebug.r2_button)
        {            
            outtakeToTopBool = !outtakeToTopBool;
            outtakeToCenterBool = false;
            outtakeToBottomBool = false;
        }
        if (inputDebug.r1_button)
        {            
            outtakeToCenterBool = !outtakeToCenterBool;
            outtakeToTopBool = false;
            outtakeToBottomBool = false;
        }
        if (inputDebug.down_button)
        {       
            intakeTrigger.SetActive(false);     
            outtakeToBottomBool = !outtakeToBottomBool;
            outtakeToTopBool = false;
            outtakeToCenterBool = false;
        }

        if (outtakeToTopBool)
        {
            Outtake(outtakeToTopTransform, outtakeTopForceMagnitude);
            
        }
        if (outtakeToBottomBool)
        {
            Outtake(outtakeToBottomTransform, outtakeBottomForceMagnitude);
            
        }
        if (outtakeToCenterBool)
        {
            Outtake(outtakeToCenterTransform, outtakeCenterForceMagnitude);
            
        }


    }

    public void MoveBlocksIntoSpace(GameObject block)
    {
        //block.SetActive(false);
        Rigidbody rb = block.GetComponent<Rigidbody>();       
        rb.isKinematic = true;            
        SphereCollider collider = block.GetComponent<SphereCollider>();
        collider.enabled = false;
        block.transform.position = targetAreas[blockStorage].position;
        currentBlocks[blockStorage] = block;
        block.transform.SetParent(transform);
        blockStorage++;        


    }

    public void Outtake(Transform customTransform, float forceMagnitude)
    {
        for (int i = 0; i < currentBlocks.Length; i++)
        {
            if (timer > 0)
            {
                return;
            }
            GameObject block = currentBlocks[i];
            if (block == null) continue;

            SphereCollider collider = block.GetComponent<SphereCollider>();
            collider.enabled = true;

            block.transform.parent = null;

            Rigidbody rb = block.GetComponent<Rigidbody>();
            rb.isKinematic = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            block.transform.position = customTransform.position;
            rb.AddForce(transform.forward * forceMagnitude, ForceMode.Impulse);
            blockStorage--;
            if (block.CompareTag("BlueBlockLoad"))
            {
                block.tag = "BlueBlock";
            }
            if (block.CompareTag("RedBlockLoad"))
            {
                block.tag = "RedBlock";
            }
            currentBlocks[i] = null;
            timer = timeSetter;
            Debug.Log(rb.linearVelocity.magnitude);
        }
    }
    
}
