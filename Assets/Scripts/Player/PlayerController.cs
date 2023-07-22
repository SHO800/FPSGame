using Photon.Pun;
using TMPro;
using UnityEngine;
using Cursor = UnityEngine.Cursor;

public class PlayerController : MonoBehaviourPun
{
    public static Transform SelfPlayer;
    
    public float normalSpeed = 3.0f;
    public float sprintSpeed = 5.0f;
    public float jump = 1f;
    public float mouseSensibilityHorizontal = 1f;
    public float mouseSensibilityVertical = 0.5f;
    public string gun1 = "Weapons/assault1";
    public Transform headBone;
    public Transform heldItemSlot;

    [HideInInspector] public bool isOpenFire;

    private Rigidbody _rb;
    private Camera _mainCamera;
    private Animator _anim;
    private bool _isOnGround;
    private SmallArm _heldItemScript;
    private PlayerData _playerData;

    public TextMeshPro text;

    private void Awake()
    {
        SelfPlayer = transform;
    }


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _mainCamera = Camera.main;
        _playerData = GetComponent<PlayerData>();
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        headBone = transform.Find("Root/Hips/Spine/Spine1/Neck/Head");
        heldItemSlot = headBone.Find("HeldItemSlot");
        
        //Debug
        text = transform.GetChild(0).GetComponent<TextMeshPro>();
        // text.text = photonView.ViewID.ToString();
    }

    private void Update()
    {
        //TODO: 各プレイヤーが発射中かどうかだけを変数で同期して各クライアントで同期しない弾丸を生成する
        if (!photonView.IsMine) return;

        MovePosition();
        RotateHead();
    
        
        // キーが押されたら銃を取り出す
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(i.ToString())) // 数字キーiが押されていたら
            {
                Debug.Log(_playerData.StoredItems);
                Debug.Log(_playerData.StoredItems[i - 1]);
                ref GameObject storedItem = ref _playerData.StoredItems[i - 1]; // インベントリに格納中の方の選択された番号のアイテム
                
                if (heldItemSlot.childCount > 0) // もし手になにか持っているなら
                {
                    GameObject heldItem = heldItemSlot.GetChild(0).gameObject; // 手に持ってたアイテム
                    if (heldItem.name == storedItem.name) continue; // もし押されたキーと同じアイテムをすでに持っていたら何もせずにスキップ
                    
                    Debug.Log("newItemSelected");
                    
                    if (heldItemSlot.childCount > 0) // なにかアイテムを持っていたら今持っているアイテムの状態を保存する (持っているアイテムを削除するのは各クライアント側で行う)
                    {
                        storedItem = heldItem;
                    }
                }
                
                // アイテムを各クライアントで呼び出す
                // photonView.RPC(nameof(SetHeldItem), RpcTarget.All, i-1);
                SetHeldItem(0);
            }
        }
        
        // 左クリックが押された/離されたときに状態更新
        if (Input.GetMouseButtonDown(0)) { _heldItemScript.OpenFire(true); }
        if (Input.GetMouseButtonUp(0)) { _heldItemScript.OpenFire(false); }

    }

    private void MovePosition() // 移動
    {
        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : normalSpeed; // 移動速度を取得
        Vector3 cameraForward = Vector3.Scale(
            _mainCamera.transform.forward,
            new Vector3(1, 0, 1)
        ).normalized; // カメラの正面方向のベクトルを取得

        // 移動キーの入力から移動ベクトルを取得
        Vector3 moveZ = cameraForward * (Input.GetAxis("Vertical") * 1); // 前後(カメラ基準)
        Vector3 moveX = _mainCamera.transform.right * (Input.GetAxis("Horizontal") * 1); // 左右(カメラ基準)
        Vector3 moveDirection = moveX + moveZ;
        Vector3 move = moveDirection * speed;
        
        // 移動実行
        _rb.velocity = move  + new Vector3(0, _rb.velocity.y, 0);
        // 地面でスペースを押したらジャンプ
        // Debug.Log(_isOnGround);
        if (Input.GetKey(KeyCode.Space) && _isOnGround)
        {
            _rb.AddForce(new Vector3(0, jump, 0), ForceMode.VelocityChange);
            // _isOnGround = false;
        }
    }

    
    private void RotateHead() // 視点移動
    {
        transform.Rotate( // 体を左右
            new Vector2(
                0,
                Input.GetAxis("Mouse X") * mouseSensibilityHorizontal
            ), Space.World);
        if (Mathf.Abs(headBone.eulerAngles.x + Input.GetAxis("Mouse Y") * -mouseSensibilityVertical - 180f) > 95f) // 上下85°までのみ回転可
        {
            headBone.Rotate( // 頭を上下
                new Vector2(
                    Input.GetAxis("Mouse Y") * -mouseSensibilityVertical,
                    0
                ), Space.Self);
        }
    }

    // [PunRPC]
    private void SetHeldItem(int itemNum)
    {
        if (heldItemSlot.childCount > 0) // もしすでに手になにか持っていたら
        {
            Destroy(heldItemSlot.GetChild(0).gameObject); // 抹消
        } 
        // GameObject newHeldItem = Instantiate(_playerData.StoredItems[itemNum], new Vector3(0f, 0f, 0f), Quaternion.identity); // 指定されたアイテムを呼び出す
        GameObject newHeldItem = PhotonNetwork.Instantiate("Weapons/assault1", new Vector3(0f, 0f, 0f), Quaternion.identity, 0, new object[1]{photonView.ViewID}); // 指定されたアイテムを呼び出す
        // newHeldItem.transform.SetParent(_heldItemSlot, false); // 手持ちスロットに呼び出したアイテムを配置する
        _heldItemScript = newHeldItem.GetComponent<SmallArm>(); 
    }
    
    private void OnCollisionStay(Collision other)
    {
        Debug.Log("Enter");
        if (other.gameObject.tag.Contains("Ground")) _isOnGround = true;
    }

    private void OnCollisionExit(Collision other)
    {
        Debug.Log("Exit");
        if (other.gameObject.tag.Contains("Ground")) _isOnGround = false;
    }
}
