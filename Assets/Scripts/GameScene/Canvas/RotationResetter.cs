using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationResetter : MonoBehaviour
{

    void Start()
    {
        gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    
}
