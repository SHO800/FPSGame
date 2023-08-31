using Fusion;
using TMPro;
using UnityEngine;

public class NoticeText : NetworkBehaviour
{
    private TextMeshProUGUI _displayText;
    private float _elapsedTime;

    public RoomSelector roomSelector;
    
    private void Start(){
        _displayText = GetComponent<TextMeshProUGUI>();
        _displayText.text = "Connecting online...";
    }

    private void Update()
    {
        if (roomSelector.cashedSessionList is not null && roomSelector.cashedSessionList.Count != 0)
        {
            _displayText.text = "";
            return;
        }
        
        _elapsedTime += Time.deltaTime;
        _displayText.text = _elapsedTime > 0.5f ? " No rooms available_" : "No rooms available";
        if (_elapsedTime > 1f) _elapsedTime = 0;
    }
}