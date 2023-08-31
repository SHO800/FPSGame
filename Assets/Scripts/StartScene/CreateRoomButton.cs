using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateRoomButton : MonoBehaviour
{
    public TextMeshProUGUI inputField;
    public float speed = 1f;

    [SerializeField]private GameObject windows;
    [SerializeField] private WindowManager windowManager;
    private TextMeshProUGUI _text;
    private Button _button;
    private Vector2 _normalPosition;
    private Vector2 _canvasSize;
    private float _elapsedTime;
    private string _roomName;
    private Image _loading;
    
    private bool _isWindowsMoving;

    private void Start()
    {
        _button = GetComponent<Button>();
        _text = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

        _canvasSize = windows.transform.parent.GetComponent<RectTransform>().sizeDelta;
        _normalPosition.y = windows.transform.localPosition.y; 
        
        _loading = GameObject.Find("Loading").GetComponent<Image>();
    }

    private void Update(){
        _roomName = inputField.text.Replace("\u200b", "");

        if (!String.IsNullOrWhiteSpace(_roomName)){
            _button.interactable = true;
            _text.color = new Color(0.7215686f, 1f, 1f, 1f);
        }else{
            _button.interactable = false;
            _text.color = new Color(0f, 0f, 0f, 0.75f);
        }
        
        
        // マジで時間なくなってきたからJoinRoomButton使いまわし
        if (!_isWindowsMoving) return;
        _elapsedTime += Time.deltaTime;
        float duration = speed * _elapsedTime;
        windows.transform.localPosition = new Vector2(
            windows.transform.localPosition.x,
            Mathf.SmoothStep(_normalPosition.y, -_canvasSize.y, duration)
        );
        _loading.enabled = true;


        if (duration < 1f) return;
        _isWindowsMoving = false;
        _elapsedTime = 0;
        
        _roomName = _roomName.Replace(" ", "_"); // 半角
        _roomName = _roomName.Replace("　", "_"); // 全角
        
        
        NetworkManager.Instance.CreateSession(_roomName, 4);
    }


    // private void RoomCreate(){
    //     _roomName = _roomName.Replace(" ", "_"); // 半角
    //     _roomName = _roomName.Replace("　", "_"); // 全角
    //
    //     //カスタムプロパティを設定 
    //     var customProperties = new Hashtable();
    //     customProperties["RoomName"] = _roomName;
    //
    //     //ルームの設定
    //     var roomOptions = new RoomOptions();
    //     roomOptions.MaxPlayers = 4;
    //     roomOptions.CustomRoomProperties = customProperties;
    //     roomOptions.CustomRoomPropertiesForLobby = new[] {"RoomName"};
    //
    //     GameManager.RoomOptions = roomOptions;
    //     GameManager.RoomName = "";
    //     SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    //     
    // }
    
    public void OnClick()
    {
        _isWindowsMoving = true;
        windowManager.isMoving = true;
        windowManager.elapsedTime = 0;
    }
}