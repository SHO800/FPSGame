using UnityEngine;
using UnityEngine.UI;

public class Blind : MonoBehaviour
{
    public float speed = 0.5f;
    private float _elapsedTime = -1;
    private Image _image;
    
    public void FadeOut()
    {
        _image = GetComponent<Image>();
        _elapsedTime = 0;
    }

    private void Update()
    {
        if (_elapsedTime < 0) return;
        _elapsedTime += Time.deltaTime;

        float duration = _elapsedTime / speed;
        _image.color = Color.HSVToRGB(0, 0, Mathf.SmoothStep(0.6f, 0.9f, duration)); // moveProgressと同じように背景の明るさを変更
        if (duration >= 1) gameObject.SetActive(false);
    }
}
