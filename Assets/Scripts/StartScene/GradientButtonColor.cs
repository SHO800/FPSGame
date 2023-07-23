using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class GradientButtonColor : MonoBehaviour
{
    public bool whichColorMain;
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        // エディタ上で変更を適用するためにGetComponentをUpdate内に書いてる
        Transform parent = transform.parent;
        Transform parentparent = parent.parent;
        UIGradient gradient = parentparent.GetComponent<UIGradient>();
        Image centerImage = parent.GetComponent<Image>();
        TextMeshProUGUI tmp = GetComponent<TextMeshProUGUI>();
        
        if (whichColorMain)
        {
            gradient.m_color2 = _camera.backgroundColor;
            centerImage.color = gradient.m_color2;
            tmp.color = gradient.m_color1;
        }
        else
        {
            gradient.m_color1 = _camera.backgroundColor;
            centerImage.color = gradient.m_color1;
            tmp.color = gradient.m_color2;
        }
    }
}
