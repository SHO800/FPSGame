using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GradientButtonColor : MonoBehaviour
{
    public bool whichColorMain;
    private Camera _camera;
    private Transform _parent;
    private Transform _parentparent;
    private UIGradient _gradient;
    private Image _centerImage;
    private TextMeshProUGUI _tmp;

    private void Start()
    {
        _camera = Camera.main;
        _parent = transform.parent;
        _parentparent = _parent.parent;
        _gradient = _parentparent.GetComponent<UIGradient>();
        _centerImage = _parent.GetComponent<Image>();
        _tmp = GetComponent<TextMeshProUGUI>();
        
        ChangeColor();
    }

    private void Update()
    {
        if (_gradient.m_color1 != _camera.backgroundColor && _gradient.m_color2 != _camera.backgroundColor) ChangeColor();
    }

    private void ChangeColor()
    {
        if (whichColorMain)
        {
            _gradient.m_color2 = _camera.backgroundColor;
            _centerImage.color = _gradient.m_color2;
            _tmp.color = _gradient.m_color1;
        }
        else
        {
            _gradient.m_color1 = _camera.backgroundColor;
            _centerImage.color = _gradient.m_color1;
            _tmp.color = _gradient.m_color2;
        }
        
        // 何故か更新されないのでつけなおす
        _gradient.enabled = false;
        _gradient.enabled = true;
    }
}
