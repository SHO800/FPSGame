using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowProperty : MonoBehaviour
{
    public float height;
    public float width;
    public float speed = 1400f;

    private RectTransform _rectTransform;
    private float _margin;
    private Vector3 _canvasSize;
    private Vector3 _selfSize;
    private Vector3 _targetPosition;
    private Vector3 _normalPosition;
    private Vector3 _rightHidePosition;
    private Vector3 _leftHidePosition;
    private bool _isActive;
    
    public void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        
        _targetPosition = transform.localPosition;
        
        _canvasSize = transform.parent.GetComponent<RectTransform>().sizeDelta;
        _selfSize = transform.GetComponent<RectTransform>().sizeDelta;
        _margin = _canvasSize.x / 10f;

        _normalPosition.x = transform.localPosition.x;
        _rightHidePosition.x = _normalPosition.x + (_canvasSize.x / 2 + _selfSize.x / 2);
        _leftHidePosition.x = _normalPosition.x - (_canvasSize.x / 2 + _selfSize.x / 2);
    }

    public void Show(bool isBack = false)
    {
        _isActive = true;
        gameObject.SetActive(true);

        if (isBack)
        {
            transform.localPosition = _leftHidePosition;
        }
        else
        {
            transform.localPosition = _rightHidePosition;
        }

        _targetPosition = _normalPosition;
    }

    public void Hide(bool isBack = false)
    {
        _isActive = false;
        
        transform.localPosition = _normalPosition;

        if (isBack)
        {
            _targetPosition = _rightHidePosition;
        }
        else
        {
            _targetPosition = _leftHidePosition;
        }

    }
    

    private void Update()
    {
        var velocity = Vector3.zero;
        var position = Vector3.zero;
        position.x = transform.localPosition.x;

        if (Vector3.Distance(position, _targetPosition) >= 0.1f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, _targetPosition, speed * Time.deltaTime);
        }
        else
        {
            var posi = transform.localPosition;
            posi.x = _targetPosition.x;
            transform.localPosition = posi;
            if (!_isActive) gameObject.SetActive(false);
        }

    }
}
