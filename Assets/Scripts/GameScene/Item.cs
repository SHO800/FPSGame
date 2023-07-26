using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Item : MonoBehaviourPunCallbacks
{
    [HideInInspector] public ItemType itemType;
    public enum ItemType
    {
        Weapon,
        Medic,
    }
}
