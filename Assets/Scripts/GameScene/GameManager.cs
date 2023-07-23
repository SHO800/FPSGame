using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public CinemachineVirtualCamera playerFollowCamera;
    void Start()
    {
        SpawnSelfPlayer();
    }
    
    void Update()
    {
        
    }

    private void SpawnSelfPlayer()
    {
        var position = new Vector3(0, -3f, 0);
        GameObject player = PhotonNetwork.Instantiate("Player", position, Quaternion.identity);
        GameObject cameraRoot = GameObject.FindWithTag("PlayerCameraRoot");
        Debug.Log(cameraRoot);
        playerFollowCamera.Follow = cameraRoot.transform;
    } 
}
