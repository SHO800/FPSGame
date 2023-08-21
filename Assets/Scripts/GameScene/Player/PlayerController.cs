using System;
using Cinemachine;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    public float mouseSensibilityHorizontal = 1.0f;
    public float mouseSensibilityVertical = 0.5f;
    [HideInInspector] public Transform headBone;
    [SerializeField] private float normalSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 10.0f;
    [SerializeField] private float jump = 1f;
    
    [HideInInspector] public Transform heldItemSlot;
    
    [HideInInspector] public float hp = 100f;
    [HideInInspector] public float damageFactor = 1f;
    [HideInInspector] public bool isOpenFire;
    [HideInInspector] public bool isAds;
    [HideInInspector] public bool isDead;
    [HideInInspector] public GameObject weaponDataUI;
    [HideInInspector] public TextMeshProUGUI ammoTMP;
    [HideInInspector] public Transform weaponImageTra;
    
    private Rigidbody _rb;
    private NetworkObject _networkObject;
    private Camera _mainCamera;
    private Animator _anim;
    private SmallArm _heldItemScript;
    private bool _isOnGround;
    private string _nickName;
    private CinemachineVirtualCamera _cineMachine;
    private float _adsTime;
    private TextMeshProUGUI _messageTMP;
    private Slider _hpBar;
    private bool _isAction;

    [Networked] public Vector2 Rotation { get; private set; }

    private void Awake()
    {
        headBone = transform.Find("Avatar/Root/Hips/Spine/Spine1/Neck/Head");
        heldItemSlot = headBone.Find("HeldItemSlot");
        weaponDataUI = GameObject.Find("WeaponData");
        ammoTMP = weaponDataUI.transform.Find("Ammo").GetComponent<TextMeshProUGUI>();
        weaponImageTra = weaponDataUI.transform.Find("WeaponImage");
        
        _networkObject = GetComponent<NetworkObject>();
        _rb = GetComponent<Rigidbody>();
        _cineMachine = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();
        _mainCamera = Camera.main;
        _messageTMP = GameObject.Find("MessageText").GetComponent<TextMeshProUGUI>();
        _hpBar = GameObject.Find("HPBar").GetComponent<Slider>();
        
    }

    private void Start()
    {
        if (!_networkObject.HasInputAuthority) return;
        // マウスカーソルを捕まえる
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        var cameraRoot = headBone.Find("PlayerCameraRoot");
        _cineMachine.Follow = cameraRoot;
        Debug.Log(_mainCamera);
    }

    private void Update()
    {
        
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData data) ) return;
        _isAction = data.IsAction;
        MovePosition(data);
        RotateHead(data);   
    }

    private void MovePosition(NetworkInputData data)
    {
        float speed = data.IsSprint ? sprintSpeed : normalSpeed; // 移動速度を取得
        Vector3 cameraForward = Vector3.Scale(
            headBone.transform.forward,
            new Vector3(1, 0, 1)
        ).normalized; // カメラの正面方向のベクトルを取得

        // 移動キーの入力から移動ベクトルを取得
        Vector3 moveZ = cameraForward * (data.MoveDirection.z * 1); // 前後(カメラ基準)
        Vector3 moveX = headBone.transform.right * (data.MoveDirection.x * 1); // 左右(カメラ基準)
        Vector3 moveDirection = moveX + moveZ;
        Vector3 move = moveDirection * speed;
        
        // Debug.Log($"datax: {data.MoveDirection.x}, dataz: {data.MoveDirection.z}, moveZ: {moveZ}, moveX: {moveX}, move: {move}");
        
        // 移動実行
        _rb.velocity = move  + new Vector3(0, _rb.velocity.y, 0);
        // 地面でスペースを押したらジャンプ
        
        if (isDead)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                _rb.AddForce(new Vector3(0, jump, 0), ForceMode.VelocityChange);
            }
            
            if (Input.GetKey(KeyCode.LeftShift))
            {
                _rb.AddForce(new Vector3(0, -jump, 0), ForceMode.VelocityChange);
            }
        }
        else
        {
            if (data.IsJump && _isOnGround)
            {
                _rb.AddForce(new Vector3(0, jump, 0), ForceMode.VelocityChange);
                _isOnGround = false;
            }
        }
    }
    
    private void RotateHead(NetworkInputData data)
    {
        
        // transform.Rotate( // 体を左右
        //     new Vector2(
        //         0,
        //         data.CameraDirection.x * mouseSensibilityHorizontal
        // ), Space.World);
        _rb.MoveRotation(Quaternion.Euler(0, data.CameraDirection.x * mouseSensibilityHorizontal, 0));
        //MoveRotationはrotationに代入するのと同じだから帰ってきたらこのRotateをそのまま移したコードを修正する
        if (Mathf.Abs(headBone.eulerAngles.x + data.CameraDirection.y * -mouseSensibilityVertical - 180f) > 95f) // 上下85°までのみ回転可
        {
            headBone.Rotate( // 頭を上下
                new Vector2(
                    data.CameraDirection.y * -mouseSensibilityVertical,
                    0
                ), Space.Self);
        }
    }
    
    public void PickUpItem(GameObject gameObj)
    {
        switch (gameObj.GetComponent<Item>().itemType)
        {
            case Item.ItemType.Weapon:
                if (heldItemSlot.childCount > 0 && gameObj.name == heldItemSlot.GetChild(0)?.name) // もし同じ武器を拾ったら
                {
                    SmallArm weaponScript = heldItemSlot.GetChild(0).GetComponent<SmallArm>();
                    weaponScript.ammo += weaponScript.capacity; // 1マガジン分弾薬増やす
                    Destroy(gameObj);
                }
                else // もし同じ武器ではなかったら
                {
                    if(!_isAction) return; // 拾うキーを押していなかったら何もしない
                    
                    if (heldItemSlot.childCount > 0) Destroy(heldItemSlot.GetChild(0)); // 他の武器を持ってたら消す
                    _heldItemScript = gameObj.GetComponent<SmallArm>();
                    _heldItemScript.OnPickUp(heldItemSlot);
                    weaponDataUI.transform.localScale = Vector3.one;
                    weaponImageTra.GetComponent<RawImage>().texture = (Texture)Resources.Load($"Images/Item/{gameObj.name.Replace("(Clone)", "")}");
                }
                
                break;
            case Item.ItemType.Medic:
                break;
        }
        
        
    }
    private void WeaponControl()
    {
        if (Input.GetKeyDown(KeyCode.R) && !_heldItemScript.isReloading)
        {
            _heldItemScript.Reload();
        }
        
        // 左クリックが押された・離されたときに状態更新
        if (Input.GetMouseButtonDown(0)) { _heldItemScript.OpenFire(true); }
        if (Input.GetMouseButtonUp(0)) { _heldItemScript.OpenFire(false); }
        
        if (Input.GetMouseButtonDown(1))
        {
            isAds = true;
            heldItemSlot.localPosition = new Vector3(0, -0.15f, 0.5f);
            _adsTime = Time.time;
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            isAds = false; 
            heldItemSlot.localPosition = new Vector3(0.3f, -0.3f, 0.5f);
            _adsTime = Time.time;
        }
        
        if (Time.time - _adsTime < 0.1)
        {
            _cineMachine.m_Lens.FieldOfView =
                isAds ? 40 - (Time.time - _adsTime) * 100 : 30 + (Time.time - _adsTime) * 100;
        }

    }
    public void GetDamage(float damage)
    {
        hp -= damage * damageFactor;
        // if (hp <= 0) photonView.RPC(nameof(Dead), RpcTarget.All);
        // if (hp <= 0) Dead();
        
    }
    
    
    
    private void OnCollisionStay(Collision other)
    {
        // なぜかこれはどのプレイヤーが当たってもホストで発火する
        
        // Debug.Log(other.collider.gameObject);
        // Debug.Log(other.rigidbody.);
        if (other.gameObject.tag.Contains("Ground")) _isOnGround = true;
        // if(other.gameObject.tag.Contains("Item")) Debug.Log("Collision item");
        // if (other.gameObject.tag.Contains("Item") && Input.GetKey(KeyCode.E))
        // {
        //     PickUpItem(other.gameObject);
        // }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag.Contains("Ground")) _isOnGround = false;
    }
}
