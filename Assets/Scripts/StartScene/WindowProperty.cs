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
        _beforeMovePosition = transform.localPosition;
        _targetPosition = transform.localPosition;
        _windowManager = WindowManager.windowManager;
        
        
        _canvasSize = transform.parent.parent.GetComponent<RectTransform>().sizeDelta;
        _selfSize = transform.GetComponent<RectTransform>().sizeDelta;

        _normalPosition.x = transform.localPosition.x;
        _rightHidePosition.x = _normalPosition.x + (_canvasSize.x / 2 + _selfSize.x / 2);
        _leftHidePosition.x = _normalPosition.x - (_canvasSize.x / 2 + _selfSize.x / 2);

        if (isFirst)
        {
            _isActive = true;
            gameObject.SetActive(true);
        }
        else
        {
            transform.localPosition = _rightHidePosition;
            _targetPosition = _rightHidePosition;
        }
    }

    public void Show(bool isBack = false)
    {
        transform.localPosition = isBack ? _leftHidePosition : _rightHidePosition;
        _targetPosition = _normalPosition;
        
        _beforeMovePosition = transform.localPosition;

        _isMoveObject = true;
        gameObject.SetActive(true);
        _isActive = true;
    }

    public void Hide(bool isBack = false)
    {
        transform.localPosition = _normalPosition;
        _targetPosition = isBack ? _rightHidePosition : _leftHidePosition;

        _isMoveObject = true;
        _beforeMovePosition = transform.localPosition;
        _isActive = false;
    }
    

    private void Update()
    {
        if (!_isMoveObject) return;
        transform.localPosition = Vector3.Lerp(_beforeMovePosition, _targetPosition, _windowManager.moveProgress);
        if (!_windowManager.isMoving)
        {
            _isMoveObject = false;
            var targetPosition = transform.localPosition;
            targetPosition.x = _targetPosition.x;
            transform.localPosition = targetPosition;
            if (!_isActive) gameObject.SetActive(false);
        }
        
        
        
    }
}
