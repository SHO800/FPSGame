using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSelector : MonoBehaviour
{
    public float speed = 100f;
    private float _nearPosition;
    private float _elapsedTime;
    private float _beforePosition;

    void Update()
    {
        _elapsedTime += Time.deltaTime;
        
        if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0)
        {
            _nearPosition = (Mathf.Round(transform.localPosition.y / 60) + Input.GetAxis("Mouse ScrollWheel") * -10) * 60;
            _elapsedTime = 0;
            _beforePosition = transform.localPosition.y;
        }
        
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            Mathf.Lerp(_beforePosition, _nearPosition, speed * _elapsedTime),
            transform.localPosition.z	
            );
    }
}
