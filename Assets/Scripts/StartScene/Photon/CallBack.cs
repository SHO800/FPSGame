using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CallBack : MonoBehaviourPunCallbacks
{
    public override void OnConnectedToMaster(){
        Debug.Log("マスターサーバーに接続しました。");
        PhotonNetwork.JoinLobby();
    }
    public override void OnDisconnected(DisconnectCause cause){
        Debug.Log($"接続に失敗したか切断されました。 {cause.ToString()}");
    }

    public override void OnJoinedLobby(){
        Debug.Log("ロビーに参加しました。");
    }
    public override void OnLeftLobby(){
        Debug.Log("ロビーから退出しました。");
    }
}