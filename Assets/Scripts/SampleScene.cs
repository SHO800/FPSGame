using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using Shapes2D;
using UnityEngine;

// MonoBehaviourPunCallbacksを継承して、PUNのコールバックを受け取れるようにする
public class SampleScene : MonoBehaviourPunCallbacks
{
    public CinemachineVirtualCamera playerFollowCamera;
    private void Start()
    {
        if (GameManager.RoomName != null) return;
        // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();
    }

    // マスターサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnConnectedToMaster()
    {
        if (GameManager.RoomName != null) return;
        // "Room"という名前のルームに参加する（ルームが存在しなければ作成して参加する）
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions(), TypedLobby.Default);
    }

    // ゲームサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnJoinedRoom()
    {
        if (GameManager.RoomName != null) return;
        var position = new Vector3(0, -3f, 0);
        GameObject player = PhotonNetwork.Instantiate("Player", position, Quaternion.identity);
        GameObject cameraRoot = GameObject.FindWithTag("PlayerCameraRoot");
        Debug.Log(cameraRoot);
        playerFollowCamera.Follow = cameraRoot.transform;
    }
}
