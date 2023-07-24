using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static string RoomName;
    public static RoomOptions RoomOptions;
    public CinemachineVirtualCamera playerFollowCamera;
    void Start()
    {
        if (RoomName == null) return; // もしStartSceneを経ていないならスキップ
        if (RoomOptions == null)
        {
            PhotonNetwork.JoinRoom(RoomName);
        }
        else
        {
            PhotonNetwork.CreateRoom(null, RoomOptions);
        }
    }

    public override void OnJoinedRoom(){
        SpawnSelfPlayer();
    }

    private void SpawnSelfPlayer()
    {
        var position = new Vector3(0, -3f, 0);
        GameObject player = PhotonNetwork.Instantiate("Player", position, Quaternion.identity);
        GameObject cameraRoot = GameObject.FindWithTag("PlayerCameraRoot");
        playerFollowCamera.Follow = cameraRoot.transform;
    } 
}
