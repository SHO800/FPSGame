using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MisileScript : MonoBehaviour
{
    private GameObject player;
    private Rigidbody rb;
    public float timeOut = 0.01f;
    private float timeElapsed;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
        transform.LookAt(player.transform);
    }

    
    void Update()
    {
        rb.AddForce(transform.forward * 100f, ForceMode.Acceleration);
        timeElapsed += Time.deltaTime;

        if (timeElapsed > timeOut)
        {
            Destroy(gameObject);
        }
    }
}
