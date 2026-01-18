using UnityEngine;

public class MatchLoad : MonoBehaviour
{
    public GameObject loadUp;
    public GameObject loadDown;

    public GameObject[] cubes;

    public bool isLoadUp = true;
    public bool loadUpVisual = false;

    public GameObject triggerObj;
    public Vector3 originalScale;

    public void Start()
    {
        originalScale = triggerObj.transform.localScale;
    }
    public void Update()
    {
        loadUpVisual = loadUp;
        InputDebug inputDebug = FindFirstObjectByType<InputDebug>();
        if (inputDebug.a_button)
        {
            SwitchState();
        }
    }
    public void SwitchState()
    {
        
        loadUp.SetActive(!loadUp.activeSelf);
        loadDown.SetActive(!loadDown.activeSelf);
        isLoadUp = !isLoadUp;    
        foreach (GameObject cube in cubes)
        {
            cube.SetActive(!cube.activeSelf);
        }
        if (isLoadUp)
        {
            triggerObj.transform.localScale = originalScale;
        }
        else
        {
            triggerObj.transform.localScale = new Vector3(originalScale.x, originalScale.y, 0.7801674f);
        }
    }
}
