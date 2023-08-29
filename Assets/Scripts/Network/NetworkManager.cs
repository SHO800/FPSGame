using System;
using System.Collections;
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
    Finished = 2
}

public class NetworkManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    
    public static NetworkManager Instance { get; private set; }
    [HideInInspector] public string nickName;
    [HideInInspector] public RoomSelector roomSelectorInstance; // RoomSelectorは1つしか存在し得ないのでそっちからsetしてもらう
    private NetworkRunner _runner;
    [SerializeField] private GameObject networkDataManagerPrefab;

    [SerializeField] private NetworkPrefabRef playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    private bool _isInitialized; // ゲームを開始したマスターでのみ有効化される TODO:マスターが変わったときのために_isInitializedはすべてのクライアントで有効にしておき、IsMaster的なのでスポーンするクライアントを変更するべきな気がする
    
    public NetworkDataManager NetworkDataManager{private set; get;}
    [HideInInspector]public Transform marker1;
    [HideInInspector]public Transform marker2;
    public float spawnTime;
    [SerializeField] private GameObject[] spawnItems;
    [Header("※合計100になるように入れること")] // コードで合計100に整えるのは面倒
    [SerializeField] private int[] spawnChance;

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
        if (Input.GetKeyDown(KeyCode.Return) && !_isInitialized)
        {
            StartGame();
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
        // 確率でどのアイテムが出るかを決める 思ったより考えること多い...
        int totalChance = spawnChance.Sum();
        int drawnNum = Random.Range(0, totalChance);
        int retryCount = 0;
        float checkedNum = 0;
        GameObject spawnItem = null;
        
        if (totalChance <= 0)
        {
            return;
        }
        while (spawnItem is null && retryCount < 10)
        {
            for (int i = 0; i < spawnItems.Length; i++)
            {
                if (checkedNum <= drawnNum && drawnNum < checkedNum + spawnChance[i])
                {
                    spawnItem = spawnItems[i];
                    break;
                }
                checkedNum += spawnChance[i];
            }

            retryCount++;
        }
        if(spawnItem is null) return; // 10回抽選してもなんかだめだったらもう設定の不備
        
        NetworkObject item = _runner.Spawn(spawnItem, GenerateRandomSpawnPos(), Quaternion.identity);
        item.ReleaseStateAuthority();
        _spawnTimer = Runner.SimulationRenderTime;
    }
    
    public void OnConnectedToServer(NetworkRunner runner)
    {
    }
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        //入ったのが自分だったときの処理だけ
        
        if (Runner.IsSharedModeMasterClient) // もし自分がマスターなら(すなわち自分が最初に入ったプレイヤーなら)
        {
            StartCoroutine(SpawnNetworkDataManager(player));
        }

        if (player != Runner.LocalPlayer) return; // 自分以外は無視
        StartCoroutine(SpawnPlayer(player));

        // Debug用に一人入ったらもうゲーム開始にする
    }
    
    private IEnumerator SpawnNetworkDataManager(PlayerRef player)
    {
        yield return new WaitUntil(() => _isGameSceneLoaded);
        NetworkDataManager = Runner.Spawn(networkDataManagerPrefab, Vector3.zero, Quaternion.identity, player, (r, o) => { o.name = "NetworkDataManager";}).GetComponent<NetworkDataManager>(); // NetworkDataManagerを出しとく
    }
    
    private IEnumerator SpawnPlayer(PlayerRef player)
    {
        
        yield return new WaitUntil(() => _isGameSceneLoaded);
        
        // 各プレイヤー固有の座標を生成
        Vector3 spawnPosition = new Vector3((player.RawEncoded % Runner.Config.Simulation.DefaultPlayers) * 3, 1, 0);
        NetworkObject networkPlayerObject = Runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        
        //networkPlayerObjectの名前をPlayer(Clone)からPlayer1,2,3...に変更
        networkPlayerObject.gameObject.name = $"Player{player.RawEncoded}";
        
        //NetworkDataManagerを取得
        NetworkDataManager ??= GameObject.Find("NetworkDataManager(Clone)").GetComponent<NetworkDataManager>(); //NetworkDataManagerの名前から(Clone)を消したかったけどなんか普通の方法ではできなかった
        
        // StartGame();
    }

    private void StartGame()
    {
        NetworkDataManager.GameState = GameStates.InGame; 
        if (_isInitialized) return;
        marker1 = GameObject.Find("Marker1").transform;
        marker2 = GameObject.Find("Marker2").transform;
        _spawnTimer = 0;
        _isInitialized = true;
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
        // TODO: 退出時にNetworkDataManagerの権限委譲とか色々
        
        // if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        // {
        //     runner.Despawn(networkObject);
        //     _spawnedCharacters.Remove(player);
        // }
    }
    
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        //Shareモード移行で不要に
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }



    public void OnDisconnectedFromServer(NetworkRunner runner) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

    public void OnSceneLoadDone(NetworkRunner runner) 
    {
        _isGameSceneLoaded = true;
    }

    public void OnSceneLoadStart(NetworkRunner runner) { }
}
