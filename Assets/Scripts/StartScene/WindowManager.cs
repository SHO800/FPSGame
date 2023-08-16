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
        _camera = Camera.main; //キャッシュ
        // ウィンドウの初期化メソッド呼び出し
        for (int i = 0; i < windows.Length; i++)
        {
            windows[i].GetComponent<WindowProperty>().Initialize(i == 0);
        }
    }

    private void Update()
    {
        if (!isMoving) return; //移動中フラグが立っていなければスキップ
        
        elapsedTime += Time.deltaTime; //移動時間加算
        float duration = elapsedTime / moveTime; // 今どんだけ移動が進んだかの割合
        moveProgress = Mathf.SmoothStep(0, 1, duration); // 各ウィンドウが参照するための移動進捗、 0から1の間をいい感じに補完してくれる

        // 背景の明るさを変更
        if (_isChangingBackGroundHsv)
            _camera.backgroundColor = Color.HSVToRGB(0, 0, Mathf.SmoothStep(_bBackGroundHsvV, _aBackGroundHsvV, duration)); // moveProgressと同じように背景の明るさを変更

        if (duration <= 1) return; // もしまだ移動が完了してない(移動が進んだ割合が1を超えていない)ならスキップ
        //移動が完了したら各フラグや変数を片付ける
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
