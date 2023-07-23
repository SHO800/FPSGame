using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.Serialization;

public class JoinRoomButton : MonoBehaviourPunCallbacks
{
    public RoomSelector roomSelector;
    public float speed = 10f;

    [SerializeField]private GameObject windows;
    private TextMeshProUGUI _text;
    private Button _button;
    private Vector2 _normalPosition;
    private Vector2 _canvasSize;
    private float _elapsedTime;
    
    private bool _isWindowsMoving;

    private void Start()
    {
        _text = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        _button = GetComponent<Button>();
        
        _canvasSize = transform.parent.parent.GetComponent<RectTransform>().sizeDelta;
        _normalPosition.x = transform.localPosition.x; 
    }

    private void Update(){
        
        if (roomSelector.SelectedButtonNum > -1){
            _button.interactable = true;
            _text.color = new Color(0f, 0f, 0f, 1f);
        }else{
            _button.interactable = false;
            _text.color = new Color(0f, 0f, 0f, 0.75f);
        }

        if (!_isWindowsMoving) return;
        _elapsedTime += Time.deltaTime;
        float duration = speed * _elapsedTime;
        windows.transform.localPosition = new Vector2(
            windows.transform.localPosition.x,
            Mathf.SmoothStep(_normalPosition.y, -_canvasSize.y, duration)
            );

        if (duration < 1f) return;
        _isWindowsMoving = false;
        _elapsedTime = 0;
        RoomJoin();

    }

    public void OnClick()
    {
        _isWindowsMoving = true;
    }
    
    private void RoomJoin(){
        string roomName = roomSelector.cashRoomList.Rows[roomSelector.SelectedButtonNum][0].ToString();
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom(){
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }
}