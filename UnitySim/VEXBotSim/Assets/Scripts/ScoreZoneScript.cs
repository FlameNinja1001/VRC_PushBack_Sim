using UnityEngine;

public class ScoreZoneScript : MonoBehaviour
{
    public bool isLongGoal = false;
    public ScoreZoneScript miniGameObject;
    public int initialRedScore = 0;
    public int initialBlueScore = 0;
    public bool controlBonusOn;

    public int finalRedScore = 0;
    public int finalBlueScore = 0;
    void Update()
    {
        if (!isLongGoal)
        {
            if (controlBonusOn)
            {
                if (initialRedScore > initialBlueScore)
                {
                    finalRedScore = initialRedScore + 10;
                    finalBlueScore = initialBlueScore;
                }
                else if (initialBlueScore > initialRedScore)
                {
                    finalBlueScore = initialBlueScore + 10;
                    finalRedScore = initialRedScore;
                }
                else
                {
                    finalRedScore = initialRedScore;
                    finalBlueScore = initialBlueScore;
                }
            }
        }     
        else if (controlBonusOn)
        {
            if (miniGameObject.initialRedScore > miniGameObject.initialBlueScore)
            {
                finalRedScore = initialRedScore + 10;
                finalBlueScore = initialBlueScore;
            }
            else if (miniGameObject.initialBlueScore > miniGameObject.initialRedScore)
            {
                finalBlueScore = initialBlueScore + 10;
                finalRedScore = initialRedScore;
            }
            else
            {
                finalRedScore = initialRedScore;
                finalBlueScore = initialBlueScore;
            }
        }   
    }

    void OnTriggerEnter(Collider other)
    {
        // ---------- SCORING ----------
        if (other.CompareTag("RedBlock"))
        {
            initialRedScore += 3;
        }
        if (other.CompareTag("BlueBlock"))
        {
            initialBlueScore += 3;
        }

        
    }

    void OnTriggerExit(Collider other)
    {
        // ---------- SCORING ----------
        if (other.CompareTag("RedBlock"))
        {
            initialRedScore -= 3;
        }
        if (other.CompareTag("BlueBlock"))
        {
            initialBlueScore -= 3;
        }        
    }
}


