using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using System.Linq;

public class NetworkDataManager : NetworkBehaviour
{
    //ネットワーク変数を主に管理するクラス
    
    // public static NetworkDataManager Instance { get; private set; }

    [Networked(OnChanged = nameof(OnGameStateChanged))] public GameStates GameState { get; set; } = GameStates.Waiting;

    [Networked(OnChanged = nameof(OnSurvivorsChanged)), Capacity(8)]
    public NetworkDictionary<int, NetworkString<_32>>  SurvivorsPlayerDict { get; }
    
    [Networked] public PlayerRef Winner { get; set; } = PlayerRef.None;

    public override void Spawned()
    { }
    
    public static void OnGameStateChanged(Changed<NetworkDataManager> changed)
    { 

    }
    

    public static void OnSurvivorsChanged(Changed<NetworkDataManager> changed)
    {
        // if(!changed.Behaviour.HasStateAuthority) return;
        if(changed.Behaviour.GameState == GameStates.InGame && changed.Behaviour.SurvivorsPlayerDict.Count == 1) //終了処理
        {
            changed.Behaviour.GameState = GameStates.Finished;
            changed.Behaviour.Winner = changed.Behaviour.SurvivorsPlayerDict.First().Key;
            changed.Behaviour.GameOverRPC();
        }
    }
    
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void GameOverRPC()
    { 
        GameSlowlyEffect();
        
        var networkDataManager = NetworkManager.Instance.NetworkDataManager;
        var winner = networkDataManager.SurvivorsPlayerDict[networkDataManager.Winner.PlayerId].ToString();
        GameObject.Find("Blind").GetComponent<Blind>().SlideIn(winner);
    }

    private async void GameSlowlyEffect()
    {
        for (int i = 0; i < 10; i++)
        {
            Time.timeScale *= 0.5f;
            await Task.Delay(200);
        }

        Time.timeScale = 1;
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void AddToSurvivorsListRPC(int playerId, string nickName)
    {
        SurvivorsPlayerDict.Add(playerId, nickName);
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RemoveFromSurvivorsListRPC(int playerId)
    {
        SurvivorsPlayerDict.Remove(playerId);
    } 
}
