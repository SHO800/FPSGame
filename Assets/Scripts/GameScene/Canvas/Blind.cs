using System;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Blind : MonoBehaviour
{
    public float speed = 0.5f;
    private float _elapsedTime = -1;
    private bool _isOut = true;
    private Image _image;
    private Vector3 _inPosition;
    private Vector3 _outPosition;
    private TextMeshProUGUI _tmp;

    private void Start()
    {
        _image = GetComponent<Image>();
        _inPosition = transform.position;
        _outPosition = new Vector3(_inPosition.x, _inPosition.y - Screen.height, _inPosition.z);
        _tmp = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SlideOut()
    {
        //下へ出ていく
        _image.enabled = true;
        _elapsedTime = 0;
        _isOut = true;
    }
    
    public void SlideIn(string winner)
    {
        //下から出てくる
        _image.enabled = true;
        _elapsedTime = 0;
        _isOut = false;
        
        // ここでStartSceneに戻ってしまおう
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _tmp.text = $"{winner} Wins!";

    }

    private void Update()
    {
        if (_elapsedTime < 0) return;
        _elapsedTime += Time.deltaTime;
        float duration = _elapsedTime / speed;

        if (duration <= 1)transform.position =new Vector3(_inPosition.x,  _isOut ? Mathf.SmoothStep(_inPosition.y, _outPosition.y, duration) : Mathf.SmoothStep(_outPosition.y, _inPosition.y, duration) , _inPosition.z);
        if (_isOut && duration >= 1) _image.enabled = false;
        if (!_isOut && duration >= 3)
        {
            NetworkManager.Instance.Runner.Shutdown();
            SceneManager.LoadScene("StartScene");
        }
    }
}
