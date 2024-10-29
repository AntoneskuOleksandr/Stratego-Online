using UnityEngine;

public class CaptureScreenshot : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ScreenCapture.CaptureScreenshot(@"E:\Work\Unity Projects\Stratego-Online\Stratego Online\Assets\Project\UI\Icons\Captain.png");
            Debug.Log("Screenshot done!");
        }
    }

}
