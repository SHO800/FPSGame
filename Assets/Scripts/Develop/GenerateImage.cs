using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateImage : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ScreenCapture.CaptureScreenshot("Assets/Scripts/Develop/ScreenShot/ScreenShot.png");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
