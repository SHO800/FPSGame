using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    public GameObject[] windows;

    private int _currentWindowNum = 0;

    public void ChangeWindow(bool isPrev = false)
    {
        
        _currentWindowNum = isPrev ? _currentWindowNum - 1 : _currentWindowNum + 1;
        
        if (isPrev)
        {
            _currentWindowNum++;
            windows[_currentWindowNum - 1].GetComponent<WindowProperty>().Hide(false);
            windows[_currentWindowNum].GetComponent<WindowProperty>().Show(false);
        }
        else
        {
            _currentWindowNum--;
            windows[_currentWindowNum - 1].GetComponent<WindowProperty>().Hide(true);
            windows[_currentWindowNum].GetComponent<WindowProperty>().Show(true);
        }

    }
}
