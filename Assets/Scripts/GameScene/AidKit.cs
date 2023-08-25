using Fusion;
using UnityEngine;

public class AidKit : Item
{
    public int healAmount = 100;
    
    public void Start()
    {
        tag = "Item"; // 初期でアイテム状態
        itemType = ItemType.AidKit;
    }
}
