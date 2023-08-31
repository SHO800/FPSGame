using System.Linq;
using TMPro;
using UnityEngine;

public class SystemMessage : MonoBehaviour
{
    private TextMeshProUGUI _tmp;
    private NetworkDataManager _networkDataManager;
    private void Start()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        _networkDataManager ??= NetworkManager.Instance.NetworkDataManager;
        if (_networkDataManager is null) return;
        if (_networkDataManager.GameState == GameStates.Waiting)
        {
            _tmp.text = NetworkManager.Instance.Runner.IsSharedModeMasterClient ? "あなたはマスターです。2人以上のプレイヤーがいる状態でEnterキーを押すとゲームを開始できます。\n" : "マスターのゲーム開始を待っています。\n";
            _tmp.text += $"参加人数: {_networkDataManager.SurvivorsPlayerDict.Count()} / {NetworkManager.Instance.Runner.SessionInfo.MaxPlayers}";
        }else
        if (_networkDataManager.GameState == GameStates.InGame)
        {
            _tmp.text = "ゲームが開始されました。\n";
            _tmp.text += $"残り人数: {_networkDataManager.SurvivorsPlayerDict.Count()}\n";
        }
    }
}
