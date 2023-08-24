using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
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
    // [Networked(OnChanged = nameof(OnGameStateChanged))]private GameStates GameState { get; set; }
    private GameStates GameState { get; set; }

    [SerializeField] private NetworkPrefabRef playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    private bool _isInitialized;
    
    [HideInInspector]public Transform marker1;
    [HideInInspector]public Transform marker2;
    public float spawnTime;
    private GameObject[] _spawnItems;

    private bool _mouseButton0;
    private bool _mouseButton1;
    private static float _spawnTimer;
    private bool _isGameSceneLoaded;

    private void Awake()
    {
        Instance = this;
    }

    private async void Start()
    {
        //マスタークライアントになったときにスポーンするプレハブをResources/Itemsから読み込んでおく
        _spawnItems = Array.ConvertAll(Resources.LoadAll("Items"), item => item as GameObject);

        if (_runner is not null) return;
        // ネットワーククライアント立ち上げ
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        
        // await _runner.JoinSessionLobby(SessionLobby.ClientServer); // ロビー参加 TODO: StartSceneを飛ばしている
        //Debugging
        await _runner.StartGame(new StartGameArgs()
        {
            SessionName = "test",
            GameMode = GameMode.Shared,
            Scene = SceneManager.GetActiveScene().buildIndex + 1,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    private void Update()
    {
        _mouseButton0 = _mouseButton0 || Input.GetMouseButton(0);
        _mouseButton1 = _mouseButton1 || Input.GetMouseButton(1);
        
        if(!_runner.IsSharedModeMasterClient) return;
        if (Input.GetKeyDown(KeyCode.Return))
        {
            StartGame();
            Debug.Log("StartGame");
        }
    }
    public override void FixedUpdateNetwork()
    {
        if(!_isInitialized) return;
        if(Runner.SimulationRenderTime - _spawnTimer >= spawnTime) SpawnItem(); // あとでlifetimeに変更
    }

    public async void JoinSession(string roomName)
    {
        await _runner.StartGame(new StartGameArgs()
        {
            SessionName = roomName,
            GameMode = GameMode.Shared,
            Scene = SceneManager.GetActiveScene().buildIndex + 1,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }
    
    public async void CreateSession(string displayRoomName, int maxPlayers)
    {
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
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
        NetworkObject item = _runner.Spawn(_spawnItems[Random.Range(0, _spawnItems.Length)], GenerateRandomSpawnPos(), Quaternion.identity);
        item.ReleaseStateAuthority();
        _spawnTimer = Runner.SimulationRenderTime;
    }
    
    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log(MethodBase.GetCurrentMethod()?.Name);
    }

    public static void OnGameStateChanged(Changed<NetworkManager> changed)
    {
        // if(changed.Behaviour.GameState == GameStates.InGame) Debug.Log("GameStarted");
    }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("joinned");
        if (player != Runner.LocalPlayer) return;
        StartCoroutine(SpawnPlayer(player));


        // プレイヤー切断時にアバターを消去できるよう辞書に入れとく
        // _spawnedCharacters.Add(player, networkPlayerObject);

        // Debug用に一人入ったらもうゲーム開始にする
    }
    
    private IEnumerator SpawnPlayer(PlayerRef player)
    {
        
        yield return new WaitUntil(() => _isGameSceneLoaded);
        
        // 各プレイヤー固有の座標を生成
        Vector3 spawnPosition = new Vector3((player.RawEncoded % Runner.Config.Simulation.DefaultPlayers) * 3, 1, 0);
        NetworkObject networkPlayerObject = Runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        
        //networkPlayerObjectの名前をPlayer(Clone)からPlayer1,2,3...に変更
        networkPlayerObject.gameObject.name = $"Player{player.RawEncoded}";
        
        // StartGame();
    }

    private void StartGame()
    {
        GameState = GameStates.InGame; 
        if (_isInitialized) return;
        marker1 = GameObject.Find("Marker1").transform;
        marker2 = GameObject.Find("Marker2").transform;
        // spawnItems = spawnItems;
        _spawnTimer = 0;
        _isInitialized = true;
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
        // if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        // {
        //     runner.Despawn(networkObject);
        //     _spawnedCharacters.Remove(player);
        // }
    }
    
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        //Shareモード移行で不要に?
        
        // NetworkInputData data = new NetworkInputData
        // {
        //     CameraDirection = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")), // クライアントの情報をそのまま適用するのはチート対策等の観点では良くないけどレスポンスがあまりに悪すぎるのでこの方法に
        //     MoveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")),
        //     IsSprint = Input.GetKey(KeyCode.LeftShift),
        //     IsJump = Input.GetKey(KeyCode.Space),
        //     IsAction = Input.GetKey(KeyCode.E),
        //     IsReload = Input.GetKey(KeyCode.R),
        // };
        //
        // if (_mouseButton0) data.MouseButtons |= NetworkInputData.MOUSE_BUTTON1;
        // _mouseButton0 = false;
        // if (_mouseButton1) data.MouseButtons |= NetworkInputData.MOUSE_BUTTON2;
        // _mouseButton1 = false;
        //
        // input.Set(data);
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
        _isGameSceneLoaded = true;
    }

    public void OnSceneLoadStart(NetworkRunner runner) 
    {
        Debug.Log(MethodBase.GetCurrentMethod()?.Name);
    }
}
