using System.Collections.Generic;
using System.Threading;
using Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [HideInInspector]public static string RoomName;
    // [HideInInspector]public static RoomOptions RoomOptions;
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

    public static List<string> Survivor = new List<string>();
    
    private void Start()
    {
        SMarker1 = marker1;
        SMarker2 = marker2;
        SSpawnItems = spawnItems;
        _spawnTimer = 0;
        
        // pun時代の遺物だけど多分いらない
        
        // if (RoomName == null) return; // もしStartSceneを経ていないならスキップ
        // while (true)
        // {
        //     if (PhotonNetwork.IsConnectedAndReady) break;
        //     Debug.Log("Waiting");
        //     Thread.Sleep(100);
        // } 
        // if (RoomOptions == null)
        // {
        //     PhotonNetwork.JoinRoom(RoomName);
        // }
        // else
        // {
        //     PhotonNetwork.CreateRoom(null, RoomOptions);
        // }

    }

    private void Update()
    {
        if(!IsGameStarted) return;
        if(Time.time - _spawnTimer >= spawnTime) SpawnItem();
    }

    // public override void OnJoinedRoom(){
    //     SpawnSelfPlayer();
    //     blind.FadeOut();
    // }

    private void SpawnSelfPlayer()
    {
        var position = new Vector3(0, -3f, 0);
        // GameObject player = PhotonNetwork.Instantiate("Player", position, Quaternion.identity);
        GameObject cameraRoot = GameObject.FindWithTag("PlayerCameraRoot");
        playerFollowCamera.Follow = cameraRoot.transform;
    }

    public static void GameStart()
    {
        // PhotonNetwork.CurrentRoom.IsOpen = false;
        // IsGameStarted = true;
        //
        // for (int i = 0; i < PhotonNetwork.CurrentRoom.Players.Count; i++)
        {
            SpawnItem();
        }
        // Debug.Log(PhotonNetwork.CurrentRoom.Players.Values.ToList().ToString());
        // foreach (var player in PhotonNetwork.CurrentRoom.Players.Values.ToList())
        // {
        //     Survivor.Add(player.NickName);
        // }
        // Debug.Log(Survivor.ToString());
        
    }

    public void EndGame()
    {

    }

    private static void SpawnItem()
    {
        float x = Random.Range(SMarker1.position.x, SMarker2.position.x);
        float y = Random.Range(SMarker1.position.y, SMarker2.position.y);
        float z = Random.Range(SMarker1.position.z, SMarker2.position.z);
        
        // PhotonNetwork.Instantiate(SSpawnItems[Random.Range(0, SSpawnItems.Length)], new Vector3(x,y,z), Quaternion.identity);

        _spawnTimer = Time.time;
    }
    
    
}
