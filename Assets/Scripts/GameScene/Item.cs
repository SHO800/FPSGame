using UnityEngine;

public class Item : MonoBehaviour
{
    [HideInInspector] public ItemType itemType;
    public enum ItemType
    {
        Weapon,
        Medic,
    }
}
