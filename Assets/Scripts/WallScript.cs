using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallScript : MonoBehaviour
{
    // public float timeOut = 0.01f;
    // private float timeElapsed;
    public GameObject prefab; 
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // timeElapsed += Time.deltaTime;

        // if (timeElapsed > timeOut)
        {
            GameObject createdObject =  Instantiate(prefab, new Vector3(0, 0, 10), Quaternion.identity);
            Debug.Log($"created! {createdObject.transform.position.ToString()}");
            // timeElapsed = 0.0f;
        }
    }
}
