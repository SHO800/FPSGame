using Photon.Pun;

public class PhotonLogin : MonoBehaviourPunCallbacks
{

    private void Start(){
        PhotonNetwork.ConnectUsingSettings();
    }

}