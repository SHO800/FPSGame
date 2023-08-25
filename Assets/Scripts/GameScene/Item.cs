using Fusion;
using UnityEngine;

public class Item : NetworkBehaviour
{
    protected Rigidbody Rb;

    [HideInInspector] public ItemType itemType;
    public enum ItemType
    {
        Weapon,
        AidKit,
    }

    private void Start()
    {
        Rb = GetComponent<Rigidbody>();
    }

    protected virtual void Update()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 0.25f, transform.eulerAngles.z);
    }
}
