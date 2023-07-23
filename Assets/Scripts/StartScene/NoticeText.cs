using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class NoticeText : MonoBehaviourPunCallbacks
{
    private TextMeshProUGUI _displayText;
    private float _elapsedTime;
    private bool _isNoRoom;
    private void Start(){
        _displayText = GetComponent<TextMeshProUGUI>();
        if (!PhotonNetwork.InLobby){
            _displayText.text = "Connecting online...";
        }
    }

    private void Update()
    {
        if (!_isNoRoom) return;
        
        _elapsedTime += Time.deltaTime;
        _displayText.text = _elapsedTime > 0.5f ? " No rooms available_" : "No rooms available";
        if (_elapsedTime > 1f) _elapsedTime = 0;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        _isNoRoom = (roomList.Count == 0);
        _displayText.text = roomList.Count == 0 ? "No rooms available" : "";
    }

}