using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkDataManager : NetworkBehaviour
{
    //ネットワーク変数を主に管理するクラス
    
    // public static NetworkDataManager Instance { get; private set; }

    [Networked(OnChanged = nameof(OnGameStateChanged))] public GameStates GameState { get; set; } = GameStates.Waiting;

    [Networked(OnChanged = nameof(OnSurvivorsChanged)), Capacity(64)] //一応ここで64人まで対応と定めてる
    public NetworkLinkedList<int> SurvivorsPlayerIds { get; } = new NetworkLinkedList<int>();
    [Networked] public PlayerRef Winner { get; set; } = PlayerRef.None;

    public override void Spawned()
    {
        Debug.Log($"NetworkDataManager Spawned {SceneManager.GetActiveScene().name}");
    }
    
    public static void OnGameStateChanged(Changed<NetworkDataManager> changed)
    { 
        if (changed.Behaviour.GameState == GameStates.InGame)
        {
            if (changed.Behaviour.Object.HasStateAuthority)
            {
                // ゲーム開始時点での生存者をidで保存しておく
                // 勝者取得など、生存者を取得する場合はActivePlayersからidを使って取得する
                foreach (var player in changed.Behaviour.Runner.ActivePlayers)
                {
                    changed.Behaviour.SurvivorsPlayerIds.Add(player.PlayerId);
                }
            }  
        }
    }

    public static void OnSurvivorsChanged(Changed<NetworkDataManager> changed)
    {
        if(changed.Behaviour.SurvivorsPlayerIds.Count == 1) //終了処理
        {
            changed.Behaviour.GameState = GameStates.Finished;
            changed.Behaviour.Winner = changed.Behaviour.SurvivorsPlayerIds[0];
            changed.Behaviour.GameOver();
        }
    }
    
    private void GameOver()
    {
        GameSlowlyEffect();
        // ここにゲーム終了演出
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
    public void RemoveFromSurvivorsListRPC(int playerId)
    {
        SurvivorsPlayerIds.Remove(playerId);
    } 
}
