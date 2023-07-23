using UnityEngine;

public class WindowProperty : MonoBehaviour
{
    private Vector3 _canvasSize;
    private Vector3 _selfSize;
    private Vector3 _targetPosition;
    private Vector3 _normalPosition;
    private Vector3 _rightHidePosition;
    private Vector3 _leftHidePosition;
    private Vector3 _beforeMovePosition;
    private bool _isActive;
    private bool _isMoveObject;
    private WindowManager _windowManager;


    public void Initialize(bool isFirst = false)
    {
        // 初期非アクティブでStartが使えないのでWindowManagerから呼び出してもらう
        
        // キャッシュ
        _beforeMovePosition = transform.localPosition;
        _targetPosition = transform.localPosition;
        _windowManager = WindowManager.windowManager;

        _canvasSize = transform.parent.parent.GetComponent<RectTransform>().sizeDelta;
        _selfSize = transform.GetComponent<RectTransform>().sizeDelta;

        _normalPosition.x = transform.localPosition.x; // 画面真ん中の表示位置
        _rightHidePosition.x = _normalPosition.x + (_canvasSize.x / 2 + _selfSize.x / 2); //右側待機位置 画面横幅半分 + ウィンドウ横幅半分だけ移動  
        _leftHidePosition.x = _normalPosition.x - (_canvasSize.x / 2 + _selfSize.x / 2); // 左側

        // 最初のウィンドウは表示しておく
        if (isFirst)
        {
            _isActive = true;
            gameObject.SetActive(true);
        }
        else
        {
            // 初手非表示のウィンドウは右側に寄せとく
            transform.localPosition = _rightHidePosition; // 右側に移動
            _targetPosition = _rightHidePosition; // 移動目標を右側にしとくことで移動しないようにする
        }
    }

    public void Show(bool isBack = false)
    {
        transform.localPosition = isBack ? _leftHidePosition : _rightHidePosition; // 戻るなら左にtpしてから出現、でなければ右にtp
        _targetPosition = _normalPosition;  // 真ん中に向かう
        
        _beforeMovePosition = transform.localPosition; // 移動開始地点

        _isMoveObject = true; // 移動中フラグを立てとく
        gameObject.SetActive(true); // 表示
        _isActive = true; // 表示フラグ立てとく
    }

    public void Hide(bool isBack = false)
    {
        transform.localPosition = _normalPosition; // 真ん中にtp
        _targetPosition = isBack ? _rightHidePosition : _leftHidePosition; // どっちに向かうか設定

        //フラグ等々
        _isMoveObject = true; 
        _beforeMovePosition = transform.localPosition;
        _isActive = false;
    }
    

    private void Update()
    {
        if (!_isMoveObject) return; // 移動中でないなら処理はスキップ
        transform.localPosition = Vector3.Lerp(_beforeMovePosition, _targetPosition, _windowManager.moveProgress); // 全体の進度に合わせてtp
        if (!_windowManager.isMoving) // 全体の移動が終わったらフラグ格納等々
        {
            _isMoveObject = false;
            var targetPosition = transform.localPosition;
            targetPosition.x = _targetPosition.x;
            transform.localPosition = targetPosition;
            if (!_isActive) gameObject.SetActive(false);
        }
        
        
        
    }
}
