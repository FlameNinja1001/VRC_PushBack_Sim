using UnityEngine;

public class ParkingZone : MonoBehaviour
{
    public int botsInSpace = 0;
    public int points = 0;
    
    void Update()
    {
        if (botsInSpace < 0)
        {
            botsInSpace = 0;
        }
        if (botsInSpace > 2)
        {
            botsInSpace = 2;
        }
        if (botsInSpace == 0)
        {
            points = 0;
        }
        else if (botsInSpace == 1)
        {
            points = 8;
        }
        else if (botsInSpace == 2)
        {
            points = 30;
        }
         
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Robot"))
        {
            botsInSpace += 1;
        }
        
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Robot"))
        {
            botsInSpace -= 1;
        }       
    }
}
