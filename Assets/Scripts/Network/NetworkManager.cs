using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum GameStates: byte
{
    Waiting = 0,
    InGame = 1,
    Result = 2
}

public class NetworkManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    
    public static NetworkManager Instance;
    [HideInInspector] public string nickName;
    [HideInInspector] public RoomSelector roomSelectorInstance; // RoomSelectorは1つしか存在し得ないのでそっちからsetしてもらう
    private NetworkRunner _runner;
    private GameStates GameState { get; set; }

    [SerializeField] private NetworkPrefabRef playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    private bool _isInitialized;
    
    public float spawnTime;
    [HideInInspector]public Transform marker1;
    [HideInInspector]public Transform marker2;
    [FormerlySerializedAs("SpawnItems")] [SerializeField]public GameObject[] spawnItems;

    private bool _mouseButton0;
    private bool _mouseButton1;
    private static float _spawnTimer;

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
        
        await _runner.JoinSessionLobby(SessionLobby.ClientServer); // ロビー参加
    }

    private void Update()
    {
        _mouseButton0 = _mouseButton0 || Input.GetMouseButton(0);
        _mouseButton1 = _mouseButton1 || Input.GetMouseButton(1);
        
        // if (!_runner.IsPlayer) return;
        if(!_runner.IsServer || !GameState.Equals(GameStates.InGame)) return;
        if(Time.time - _spawnTimer >= spawnTime) SpawnItem();
    }

    public async void JoinSession(string roomName)
    {
        await _runner.StartGame(new StartGameArgs()
        {
            SessionName = roomName,
            GameMode = GameMode.AutoHostOrClient,
            // GameMode = GameMode.Shared,
            Scene = SceneManager.GetActiveScene().buildIndex + 1,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }
    
    public async void CreateSession(string displayRoomName, int maxPlayers)
    {
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            // GameMode = GameMode.Shared,
            Scene = SceneManager.GetActiveScene().buildIndex + 1,
            SceneManager =  gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = maxPlayers,
            SessionProperties = new Dictionary<string, SessionProperty>
            {
                {"DisplayRoomName", displayRoomName}
            }
        });
    }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        roomSelectorInstance.UpdateRoomList(sessionList);
    }

    // -------------
    // 以下GameScene
    // -------------

    private Vector3 GenerateRandomSpawnPos()
    {
        var pos1 = marker1.position;
        var pos2 = marker2.position;
        
        var x = Random.Range(pos1.x, pos2.x);
        var y = Random.Range(pos1.y, pos2.y);
        var z = Random.Range(pos1.z, pos2.z);
        
        return new Vector3(x, y, z);
    }
    
    private void SpawnItem()
    {
        NetworkObject item = _runner.Spawn(spawnItems[Random.Range(0, spawnItems.Length)], GenerateRandomSpawnPos(), Quaternion.identity);
        _spawnTimer = Time.time;
    }
    
    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log(MethodBase.GetCurrentMethod()?.Name);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        if (!runner.IsServer) return;
        
        // 各プレイヤー固有の座標を生成
        Vector3 spawnPosition =
            new Vector3((player.RawEncoded % runner.Config.Simulation.DefaultPlayers) * 3, 1, 0);
        NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        
        //networkPlayerObjectの名前をPlayer(Clone)からPlayer1,2,3...に変更
        networkPlayerObject.gameObject.name = $"Player{player.RawEncoded}";
        
        // プレイヤー切断時にアバターを消去できるよう辞書に入れとく
        _spawnedCharacters.Add(player, networkPlayerObject);
        
        GameState = GameStates.InGame; // Debug
        if (_isInitialized) return;
        marker1 = GameObject.Find("Marker1").transform;
        marker2 = GameObject.Find("Marker2").transform;
        // spawnItems = spawnItems;
        _spawnTimer = 0;
        _isInitialized = true;
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }
    
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

        NetworkInputData data = new NetworkInputData
        {
            CameraDirection = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")), // クライアントの情報をそのまま適用するのはチート対策等の観点では良くないけどレスポンスがあまりに悪すぎるのでこの方法に
            MoveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")),
            IsSprint = Input.GetKey(KeyCode.LeftShift),
            IsJump = Input.GetKey(KeyCode.Space),
            IsAction = Input.GetKey(KeyCode.E),
            IsReload = Input.GetKey(KeyCode.R),
        };

        if (_mouseButton0) data.MouseButtons |= NetworkInputData.MOUSE_BUTTON1;
        _mouseButton0 = false;
        if (_mouseButton1) data.MouseButtons |= NetworkInputData.MOUSE_BUTTON2;
        _mouseButton1 = false;

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) 
    {
        Debug.Log(MethodBase.GetCurrentMethod()?.Name);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) 
    {
        Debug.Log(MethodBase.GetCurrentMethod()?.Name);
    }



    public void OnDisconnectedFromServer(NetworkRunner runner) 
    {
        Debug.Log(MethodBase.GetCurrentMethod()?.Name);
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) 
    {
        Debug.Log(MethodBase.GetCurrentMethod()?.Name);
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) 
    {
        Debug.Log(MethodBase.GetCurrentMethod()?.Name);
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) 
    {
        Debug.Log(MethodBase.GetCurrentMethod()?.Name);
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) 
    {
        Debug.Log(MethodBase.GetCurrentMethod()?.Name);
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) 
    {
        Debug.Log(MethodBase.GetCurrentMethod()?.Name);
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) 
    {
        Debug.Log(MethodBase.GetCurrentMethod()?.Name);
    }

    public void OnSceneLoadDone(NetworkRunner runner) 
    {
        Debug.Log(MethodBase.GetCurrentMethod()?.Name);
    }

    public void OnSceneLoadStart(NetworkRunner runner) 
    {
        Debug.Log(MethodBase.GetCurrentMethod()?.Name);
    }

    public override void FixedUpdateNetwork()
    {
        // Debug.Log(Runner.SessionInfo);
        // Debug.Log(Runner.LobbyInfo);
        // Debug.Log("----------------------------");
    }
}
