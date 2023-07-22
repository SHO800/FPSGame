using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomSelectorButton : MonoBehaviour
{
    public float maxWidthPositionY = 1f;
    void Update()
    {
        transform.localScale = new Vector2(
            1 - Mathf.Abs(maxWidthPositionY - transform.position.y) / 48f,
            transform.localScale.y);
    }
}
