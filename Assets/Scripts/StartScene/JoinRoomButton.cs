using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

public class JoinRoomButton : MonoBehaviourPunCallbacks
{
    public RoomSelector roomSelector;
    public float speed = 1f;

    [SerializeField]private GameObject windows;
    [SerializeField] private WindowManager windowManager;
    private TextMeshProUGUI _text;
    private Button _button;
    private Vector2 _normalPosition;
    private Vector2 _canvasSize;
    private float _elapsedTime;
    
    private bool _isWindowsMoving;

    private void Start()
    {
        _text = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        _button = GetComponent<Button>();
        
        _canvasSize = windows.transform.parent.GetComponent<RectTransform>().sizeDelta;
        _normalPosition.y = windows.transform.localPosition.y; 
    }

    private void Update(){
        
        if (roomSelector.SelectedButtonNum > -1){
            _button.interactable = true;
            _text.color = new Color(0.7215686f, 1f, 1f, 1f);
        }else{
            _button.interactable = false;
            _text.color = new Color(0f, 0f, 0f, 0.75f);
        }

        if (!_isWindowsMoving) return;
        _elapsedTime += Time.deltaTime;
        float duration = speed * _elapsedTime;
        windows.transform.localPosition = new Vector2(
            windows.transform.localPosition.x,
            Mathf.SmoothStep(_normalPosition.y, -_canvasSize.y, duration)
            );

        if (duration < 1f) return;
        _isWindowsMoving = false;
        _elapsedTime = 0;
        RoomJoin();
        

    }

    public void OnClick()
    {
        _isWindowsMoving = true;
        windowManager.isMoving = true;
        windowManager.elapsedTime = 0;
    }
    
    private void RoomJoin(){
        GameManager.RoomName =  roomSelector.cashRoomList.Rows[roomSelector.SelectedButtonNum][0].ToString();
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }
    
}