using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HpNum : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private Slider _slider;
    private void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _slider = transform.parent.GetComponent<Slider>();
    }
    
    
    public void ChangeNum()
    {
        _text.text = _slider.value.ToString();
    }
}
