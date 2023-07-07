using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerData : MonoBehaviourPunCallbacks
{
    public GameObject[] ItemSlot {  get; set; }

    [PunRPC]
    public void OpenFire()
    {
        // _owner = owner; // 銃を使った人
        // IsShooting = true;
    }

    [PunRPC]
    public void CloseFire()
    {
        // IsShooting = false;
    }
}
