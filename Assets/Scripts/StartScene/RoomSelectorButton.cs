using UnityEngine;
using UnityEngine.UI;

public class RoomSelectorButton : MonoBehaviour
{
    public float threshold = 1.2f;
    private float _maxWidthPositionY;
    private Image _image;
    private Toggle _toggle;
    private Transform _effectImageParent;
    private Transform _effectImageBlack;
    private Transform _effectImageWhite;
    private Canvas _buttonCanvas;

    private void Start()
    {
        _maxWidthPositionY = transform.parent.parent.GetComponent<RoomSelector>().maxWidthPositionY;
        _toggle = GetComponent<Toggle>();
        _image = GetComponent<Image>();
        _effectImageParent =transform.GetChild(0).GetChild(0);
        _effectImageBlack = _effectImageParent.GetChild(0);
        _effectImageWhite = _effectImageParent.GetChild(1);
        _buttonCanvas = transform.parent.GetComponent<Canvas>();

    }

    private void Update()
    {
        _image.color = _toggle.isOn ? new Color(0.5f, 1, 1) : new Color(0.7f, 1, 1);

        if (_toggle.isOn)
        {
            if (_effectImageWhite.localScale.x < threshold && _effectImageBlack.localScale.x < threshold) _effectImageWhite.localScale = new Vector3(threshold, 0, 0);
            if (_effectImageWhite.localScale.x >= threshold)
            {
                _effectImageBlack.localScale = new Vector3(_effectImageBlack.localScale.x + Time.deltaTime, 1, 1);
                if (_effectImageBlack.localScale.x >= threshold) {
                    _effectImageWhite.localScale = new Vector3(0, 1, 1);
                    _effectImageWhite.SetAsLastSibling();
                }
            }
            if (_effectImageBlack.localScale.x >= threshold)
            {
                _effectImageWhite.localScale = new Vector3(_effectImageWhite.localScale.x + Time.deltaTime, 1, 1);
                if (_effectImageWhite.localScale.x >= threshold) {
                    _effectImageBlack.localScale = new Vector3(0, 1, 1);
                    _effectImageBlack.SetAsLastSibling();
                }
            }
        }
        else
        {
            _effectImageBlack.localScale = Vector3.zero;
            _effectImageWhite.localScale = Vector3.zero;
        }
        

    }

    private void FixedUpdate() // RoomSelectorの処理のあとに行いたいのでFixed (Lateはうまく行かなかった)
    {
        ChangeButtonPositionZ();
        
        // 一応回転速度は一定にさせる
        // _effectImageBlack.localScale = _toggle.isOn ? new Vector3(Mathf.Sin(), 1, 1) : Vector3.zero;
        // _effectImageWhite.localScale = _toggle.isOn ? new Vector3() : Vector3.zero;
        // if (_toggle.isOn) _effectImage.Rotate(0, 0, 1);
    }

    private void ChangeButtonPositionZ()
    {
        // ボタンの大きさを変える演出
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            transform.localPosition.y,
            Mathf.Abs(_maxWidthPositionY - transform.position.y) * 20f // 真ん中の一番大きく表示される位置との距離に比例して奥に表示する
        );
        _buttonCanvas.sortingOrder = -(int)Mathf.Abs(_maxWidthPositionY - transform.position.y);
    }
}
