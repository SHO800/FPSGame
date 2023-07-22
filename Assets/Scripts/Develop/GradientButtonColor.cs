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

    private void Update()
    {
        Transform parent = transform.parent;
        Transform parentparent = parent.parent;
        UIGradient gradient = parentparent.GetComponent<UIGradient>();
        Image centerImage = parent.GetComponent<Image>();
        TextMeshProUGUI tmp = GetComponent<TextMeshProUGUI>();
        
        if (whichColorMain)
        {
            centerImage.color = gradient.m_color2;
            tmp.color = gradient.m_color1;
        }
        else
        {
            centerImage.color = gradient.m_color1;
            tmp.color = gradient.m_color2;
        }
    }
}
