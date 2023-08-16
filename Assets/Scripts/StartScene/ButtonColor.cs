using UnityEngine;
using UnityEngine.UI;

public class ButtonColor : MonoBehaviour
{
    private Camera _camera;
    private Image _image;

    private void Start()
    {
        _camera = Camera.main;
        _image = GetComponent<Image>();
    }


    private void Update()
    {
        _image.color = _camera.backgroundColor;
    }
}
