using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadScript : MonoBehaviour
{

    private Transform parent;
    private Vector2 mouseMoveDelta;
    public float mouseSensibilityHorizontal = 1f;
    public float mouseSensibilityVertical = 0.5f;

    void Start()
    {
        parent = transform.parent;
    }
    
    void Update()
    {
        //カメラ制御
        transform.Rotate(
            new Vector2( Input.GetAxis("Mouse Y") * -mouseSensibilityVertical, 0)
            );
        parent.Rotate(
            new Vector2(0, Input.GetAxis("Mouse X") * mouseSensibilityHorizontal)
            );
    }
}
