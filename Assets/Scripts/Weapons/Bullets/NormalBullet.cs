using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NormalBullet : MonoBehaviour
{
    public float damage;
    
    // private Rigidbody _rb;
    
    private void Start()
    {
        // _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (transform.position.y < -100)
        {
            Destroy(gameObject);
        }
    }

    // Triggerだと貫通することがあるのでCollisionにしてる
    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Enter");
        Destroy(gameObject);
    }
    
    private void OnCollisionStay(Collision other)
    {
        Debug.Log("Stay");
        Destroy(gameObject);
    }

    
}
