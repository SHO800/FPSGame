using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class PlayerData : MonoBehaviourPunCallbacks
{

    [SerializeField] public GameObject[] debugLoadOuts;
    
    public GameObject[] LoadOuts { get; set; }
    public GameObject[] StoredItems { get; set; }

    private void Start()
    {
        // デバッグ用
        LoadOuts = new GameObject[debugLoadOuts.Length];
        Array.Copy(debugLoadOuts, LoadOuts, debugLoadOuts.Length);
        
        StoredItems = new GameObject[LoadOuts.Length];
        Array.Copy(LoadOuts, StoredItems, LoadOuts.Length);
    }
}
