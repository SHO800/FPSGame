using UnityEngine;

public class WindowManager : MonoBehaviour
{
    public float moveTime = 1f;
    
    public static WindowManager windowManager;
    public GameObject[] windows;
    [HideInInspector] public bool isMoving;
    [HideInInspector] public float moveProgress;
    [HideInInspector] public float elapsedTime;

    private float _bBackGroundHsvV;
    private float _aBackGroundHsvV;
    private bool _isChangingBackGroundHsv;
    private int _currentWindowNum;
    private Camera _camera;

    private void Awake()
    {
        windowManager = this;
    }

    private void Start()
    {
        _camera = Camera.main;
        for (int i = 0; i < windows.Length; i++)
        {
            windows[i].GetComponent<WindowProperty>().Initialize(i == 0);
        }
    }

    private void Update()
    {
        if (!isMoving) return;
        
        elapsedTime += Time.deltaTime;
        float duration = elapsedTime / moveTime;
        moveProgress = Mathf.SmoothStep(0, 1, duration);

        // 背景の明るさを変更
        if (_isChangingBackGroundHsv)
            _camera.backgroundColor = Color.HSVToRGB(0, 0, Mathf.SmoothStep(_bBackGroundHsvV, _aBackGroundHsvV, duration));

        if (!(duration >= 1)) return;
        moveProgress = 0;
        isMoving = false;
        elapsedTime = 0;
        _isChangingBackGroundHsv = false;

    }

    public void ChangeBackGroundHsvV(bool isPrev)
    {
        _isChangingBackGroundHsv = true;
        _bBackGroundHsvV = _aBackGroundHsvV;
        _aBackGroundHsvV += isPrev ? -0.3f : 0.3f;
    }
    
    public void ChangeWindow(bool isPrev = false)
    {
        if (isPrev)
        {
            windows[_currentWindowNum].GetComponent<WindowProperty>().Hide(true);
            _currentWindowNum--;
            windows[_currentWindowNum].GetComponent<WindowProperty>().Show(true);
        }
        else
        {
            windows[_currentWindowNum].GetComponent<WindowProperty>().Hide();
            _currentWindowNum++;
            windows[_currentWindowNum].GetComponent<WindowProperty>().Show();
        }

        //Todo: ルームセレクターで0.3, 参加処理中で0.6, 参加完了アニメーションで0.9

        isMoving = true;
    }
}
