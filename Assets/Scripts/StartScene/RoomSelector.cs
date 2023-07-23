using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 期限近いのでルームデータ関連は去年電算で作ったコードを流用
// 昔のコードコメントついてたはいいけど汚いし思い出せない;;

public class RoomSelector : MonoBehaviourPunCallbacks
{
    public float speed = 100f;
    public float buttonHeight = 100f;
    public float maxWidthPositionY = 1f;
    public GameObject listButtonPrefab;
    
    public DataTable cashRoomList = new DataTable();

    [NonSerialized]public int SelectedButtonNum;

    private float _nearPosition;
    private float _elapsedTime;
    private float _beforePosition;
    
    
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
    
    
    
    //ルームの情報が更新されたときのコールバック
    public override void OnRoomListUpdate(List<RoomInfo> list){
        foreach (RoomInfo room in list){ // 来た情報はリストなので一つづつ処理
            
            DataRow[] inListRoom = cashRoomList.Select($"RoomName = '{room.Name}'"); // キャッシュに同じものがないか取得しておく
            
            if(inListRoom.Length > 0) // すでにキャッシュに同じ情報があった場合は以前の情報を削除
                cashRoomList.Rows.Remove(inListRoom[0]);

            if (room.RemovedFromList) return; // 来た情報が削除された部屋の情報だったら消すだけで終わり
            
            if (room.IsOpen) // 参加可能な部屋のデータだったら追加 (もし元々キャッシュにあって参加可能でないなら満員になったということなので消すだけでおｋ)
                cashRoomList.Rows.Add(
                    room.Name,
                    room.CustomProperties.TryGetValue("RoomName", out var property) ? property : "(ルーム名不明)",
                    $"{room.PlayerCount.ToString()} of {room.MaxPlayers.ToString()}"
                );
        }

        UpdateRoomList();
    }
    
    private void UpdateRoomList(){
        
        // 元のボタンを全削除
        foreach (Transform child in gameObject.transform){
            Destroy(child.gameObject);
        }
        
        // ボタン生成
        for (int i = 0; i < cashRoomList.Rows.Count; i++){
            //ボタンを生成してcanvasの子にする
            GameObject button = Instantiate(listButtonPrefab, transform.position, Quaternion.identity);
            button.transform.SetParent(transform, false);
            button.GetComponent<Toggle>().group = GetComponent<ToggleGroup>();

            //ボタンの表示内容を設定
            //このfor文の1回目はルームの表示名、2回目は参加人数
            //なおデータテーブルの列は1列目から ルーム名 ルームの表示名 ルームの人数/最大人数 の3つ 
            for (int j = 0; j < 2; j++){
                var buttonsTMP = button.transform.GetChild(j).GetComponent<TextMeshProUGUI>();
                buttonsTMP.text = cashRoomList.Rows[i][j+1].ToString();
            }
        }
    }
    
}
