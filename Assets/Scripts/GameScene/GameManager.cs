using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Timeline;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviourPunCallbacks
{
    [HideInInspector]public static string RoomName;
    [HideInInspector]public static RoomOptions RoomOptions;
    [HideInInspector]public static bool IsGameStarted;
    public CinemachineVirtualCamera playerFollowCamera;
    public Blind blind;
    public float spawnTime;

    public Transform marker1;
    public Transform marker2;
    public string[] spawnItems;
    
    
    public static Transform SMarker1;
    public static Transform SMarker2;
    public static string[] SSpawnItems;

    private static float _spawnTimer;

    public static List<string> Survivor;
    
    private void Start()
    {
        SMarker1 = marker1;
        SMarker2 = marker2;
        SSpawnItems = spawnItems;
        
        if (RoomName == null) return; // もしStartSceneを経ていないならスキップ
        if (RoomOptions == null)
        {
            PhotonNetwork.JoinRoom(RoomName);
        }
        else
        {
            PhotonNetwork.CreateRoom(null, RoomOptions);
        }

        _spawnTimer = 0;
    }

    private void Update()
    {
        if(!IsGameStarted) return;
        if(Time.time - _spawnTimer >= spawnTime) SpawnItem();
    }

    public override void OnJoinedRoom(){
        SpawnSelfPlayer();
        blind.FadeOut();
    }

    private void SpawnSelfPlayer()
    {
        var position = new Vector3(0, -3f, 0);
        GameObject player = PhotonNetwork.Instantiate("Player", position, Quaternion.identity);
        GameObject cameraRoot = GameObject.FindWithTag("PlayerCameraRoot");
        playerFollowCamera.Follow = cameraRoot.transform;
    }

    public static void GameStart()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        IsGameStarted = true;
        
        for (int i = 0; i < PhotonNetwork.CurrentRoom.Players.Count; i++)
        {
            SpawnItem();
            Survivor.Add(PhotonNetwork.CurrentRoom.Players[i].NickName);
        }

    }

    public void EndGame()
    {

    }

    private static void SpawnItem()
    {
        float x = Random.Range(SMarker1.position.x, SMarker2.position.x);
        float y = Random.Range(SMarker1.position.y, SMarker2.position.y);
        float z = Random.Range(SMarker1.position.z, SMarker2.position.z);
        
        PhotonNetwork.Instantiate(SSpawnItems[Random.Range(0, SSpawnItems.Length)], new Vector3(x,y,z), Quaternion.identity);

        _spawnTimer = Time.time;
    }
}
