using System;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomSelector : MonoBehaviour
{
    public float speed = 10f;
    public float buttonHeight = 100f;
    public float maxWidthPositionY = 1f;
    public GameObject listButtonPrefab;
    public List<SessionInfo> cashedSessionList;

    [NonSerialized]public int SelectedButtonNum;

    private float _nearPosition;
    private float _elapsedTime;
    private float _beforePosition;

    private void Start()
    {
        NetworkManager.Instance.roomSelectorInstance = this;
    }

    private void Update()
    {
        _elapsedTime += Time.deltaTime; // 移動の経過時間を加算していく
        
        // ホイールで上下に動かすターゲットを設定
        if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0) // ホイールがどっちかでも動かされてたなら
        {
            float round = Mathf.Round(transform.localPosition.y / buttonHeight); // 現在のy座標をボタンの高さで割って四捨五入、これでリストの上から何番目の場所にいるか出す
            float wheel = Input.GetAxis("Mouse ScrollWheel") * -10; // GetAxisが0.1か-0.1しか出してくれないので10倍、なんか思ってたのと上下逆だから-倍
            
            _nearPosition = (round + wheel) * buttonHeight; // 最も近い、切りが良い場所のy座標
            _elapsedTime = 0; // ホイールが動かされたときを移動の開始時間とする
            _beforePosition = transform.localPosition.y; // ホイールが動かされた時の位置を移動開始位置とする
        }
        
        // 時間に合わせて移動
        transform.localPosition = new Vector2(
            transform.localPosition.x,
            Mathf.Lerp(_beforePosition, _nearPosition, speed * _elapsedTime + 0.3f)
        );
        
        // オンのボタンが何番目かを取得
        if (gameObject.transform.childCount > 0){
            foreach (Transform child in gameObject.transform){
                if (child.GetChild(0).GetComponent<Toggle>().isOn){ // 時間なくなってきたのでGetComponentでごり押し
                    SelectedButtonNum = child.GetSiblingIndex();
                }
            }
        }else{
            SelectedButtonNum = -1;
        }
    }

    
    public void UpdateRoomList(List<SessionInfo> sessionList)
    {
        cashedSessionList = sessionList;
        
        // 元のボタンを全削除
        // もし大規模に運用するなら差分のみ変化するようにしないと更新のたびにボタンがリセットされて面倒になるが、今回はその予定がないので豪快に一度全削除
        foreach (Transform child in gameObject.transform){
            Destroy(child.gameObject);
        }
        
        if (sessionList.Count < 1) return;
        
        // ボタン生成
        foreach (var session in sessionList)
        {
            //ボタンを生成してcanvasの子にする
            GameObject button = Instantiate(listButtonPrefab, transform.position, Quaternion.identity);
            button.transform.SetParent(transform, false);
            button.transform.GetChild(0).GetComponent<Toggle>().group = GetComponent<ToggleGroup>();

            //ボタンの表示内容を設定
            var buttonTexts = button.transform.GetChild(0); 
            buttonTexts.GetChild(1).GetComponent<TextMeshProUGUI>().text = session.Properties["DisplayRoomName"]; // ルームの表示名
            buttonTexts.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"{session.PlayerCount} / {session.MaxPlayers}"; // 部屋の人数 / 最大人数
            
        }
    }
    
}
