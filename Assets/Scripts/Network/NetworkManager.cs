using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;

public class NetworkManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance;
    public string nickName;
    public RoomSelector roomSelectorInstance; // RoomSelectorは1つしか存在し得ないのでそっちからsetしてもらう
    
    private NetworkRunner _runner;

    private void Awake()
    {
        Instance = this;
    }

    private async void Start()
    {
        if (_runner is not null) return;
        // ネットワーククライアント立ち上げ
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        
        await _runner.JoinSessionLobby(SessionLobby.ClientServer);
    }
    
    private async void OnGUI() // 仮
    {
        if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
        {
            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Host,
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneManager =  gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
        }
    }

    public async void JoinSession(string roomName)
    {
        await _runner.StartGame(new StartGameArgs()
        {
            SessionName = roomName,
            GameMode = GameMode.AutoHostOrClient,
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }
    
    public async void CreateSession(string displayRoomName, int maxPlayers)
    {
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager =  gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = maxPlayers,
            SessionProperties = new Dictionary<string, SessionProperty>
            {
                {"DisplayRoomName", displayRoomName}
            }
        });
    }
    
    
    
    // -------------
    // 以下コールバック
    // -------------
    
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        roomSelectorInstance.UpdateRoomList(sessionList);
        // Debug.Log($"SessionListUpdated: {sessionList.Count}");
    }


    // -------------
    // 以下仮置場
    // -------------
    public void OnConnectedToServer(NetworkRunner runner) 
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) 
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) 
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) 
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }


    public void OnDisconnectedFromServer(NetworkRunner runner) 
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) 
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) 
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) 
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) 
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) 
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) 
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }

    public void OnSceneLoadDone(NetworkRunner runner) 
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }

    public void OnSceneLoadStart(NetworkRunner runner) 
    {
        Debug.Log(MethodBase.GetCurrentMethod().Name);
    }

    public override void FixedUpdateNetwork()
    {
        // Debug.Log(Runner.SessionInfo);
        // Debug.Log(Runner.LobbyInfo);
        // Debug.Log("----------------------------");
    }
}
