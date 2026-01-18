using UnityEngine;
using TMPro;

public class TotalScore : MonoBehaviour
{
    public int totalRedScore;
    public int totalBlueScore;
    public ScoreZoneScript[] assets;
    public ParkingZone redPark;
    public ParkingZone bluePark;
    public TextMeshProUGUI scoreText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        totalRedScore = 0;
        totalBlueScore = 0;
        foreach (ScoreZoneScript score in assets)
        {
            totalRedScore += score.finalRedScore;
            totalBlueScore += score.finalBlueScore;
        }
        totalRedScore += redPark.points;
        totalBlueScore += bluePark.points;
        scoreText.text = "SCORE\nRed Alliance: " + totalRedScore + "\nBlue Alliance: " + totalBlueScore;

    }
}
