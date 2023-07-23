using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RoomSelectorButton : MonoBehaviour
{
    
    
    private float _maxWidthPositionY;
    private Image _image;
    private Toggle _toggle;
    
    private void Start()
    {
        _maxWidthPositionY = transform.parent.parent.GetComponent<RoomSelector>().maxWidthPositionY;
        _toggle = GetComponent<Toggle>();
        _image = GetComponent<Image>();
    }

    private void Update()
    {
        _image.color = _toggle.isOn ? new Color(0.5f, 1, 1) : new Color(0.7f, 1, 1);
    }

    private void FixedUpdate() // RoomSelectorの処理のあとに行いたいのでFixed (Lateはうまく行かなかった)
    {
        ChangeButtonPositionZ();
    }

    private void ChangeButtonPositionZ()
    {
        // ボタンの大きさを変える演出
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            transform.localPosition.y,
            Mathf.Abs(_maxWidthPositionY - transform.position.y) * 20f // 真ん中の一番大きく表示される位置との距離に比例して奥に表示する
        );
    }
}
