using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    private Rigidbody _rb;
    
    [HideInInspector] public ItemType itemType;
    public enum ItemType
    {
        Weapon,
        Medic,
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }
    
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag.Contains("Player")) other.gameObject.GetComponent<PlayerController>().PickUpItem(gameObject);
    }
}
