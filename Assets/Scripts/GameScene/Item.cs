using System;
using Fusion;
using UnityEngine;

public class Item : NetworkBehaviour
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

    protected virtual void Update()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 0.25f, transform.eulerAngles.z);
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag.Contains("Player")) other.gameObject.GetComponent<PlayerController>().PickUpItemRPC(Object);
    }
}
