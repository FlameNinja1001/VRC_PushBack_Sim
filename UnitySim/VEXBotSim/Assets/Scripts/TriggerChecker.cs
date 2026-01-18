using UnityEngine;

public class TriggerChecker : MonoBehaviour
{
    public bool isInTrigger;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
        
   
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("RedBlock")||other.gameObject.CompareTag("BlueBlock")||other.gameObject.CompareTag("RedBlockLoad")||other.gameObject.CompareTag("BlueBlockLoad"))
        {
            isInTrigger = true;
        } 
    }

    void OnTriggerStay(Collider other)
    {
        GameObject parentGameObject = transform.parent.gameObject;
        MatchLoad matchLoad = parentGameObject.GetComponent<MatchLoad>();
        if (other.gameObject.CompareTag("RedBlock") || other.gameObject.CompareTag("BlueBlock"))
        {
            if (isInTrigger && matchLoad.isLoadUp)
            {
                BlockControl blockControl = parentGameObject.GetComponent<BlockControl>();
                if (blockControl.intakeTrigger && blockControl.blockStorage < 5)
                {
                    blockControl.MoveBlocksIntoSpace(other.gameObject);
                }
            }
        }  
        else if (other.gameObject.CompareTag("RedBlockLoad") || other.gameObject.CompareTag("BlueBlockLoad"))
        {
            if (isInTrigger && !matchLoad.isLoadUp)
            {
                                
                BlockControl blockControl = parentGameObject.GetComponent<BlockControl>();
                if (blockControl.intakeTrigger && blockControl.blockStorage < 5)
                {
                    blockControl.MoveBlocksIntoSpace(other.gameObject);
                }
            }
        }       
    }

    void OnTriggerExit(Collider other)
    {
        
    }


    
}

    
    

        
