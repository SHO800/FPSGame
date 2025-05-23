using System;
using System.Threading.Tasks;
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
    [SerializeField] private GameObject deadEffect;
    
    [HideInInspector] public Transform heldItemSlot;
    [HideInInspector, Networked] public int Hp { set; get; } = 100;
    [HideInInspector] public float damageFactor = 1f;
    [HideInInspector] public bool isOpenFire;
    [HideInInspector] public bool isAds;
    [HideInInspector] public bool isDead;
    [HideInInspector] public GameObject weaponDataUI;
    [HideInInspector] public TextMeshProUGUI ammoTMP;
    [HideInInspector] public Transform weaponImageTra;
    [HideInInspector] public SoundManager soundManager;
    [HideInInspector] public Image reloadProgressImage;
    
    private NetworkDataManager _networkDataManager;
    private Rigidbody _rb;
    private NetworkObject _networkObject;
    private Camera _mainCamera;
    private Animator _anim;
    private SmallArm _heldItemScript;
    private bool _isOnGround;
    private string _nickName;
    private CinemachineVirtualCamera _cineMachine;
    private float _adsTime;
    private TextMeshPro _playerNameTMP;
    private TextMeshProUGUI _messageTMP;
    private Slider _hpBar;
    private Transform _reticle;
    

    private void Start()
    {
        headBone = transform.Find("Player/Head");
        heldItemSlot = transform.Find("Player/ItemSlot");
        weaponDataUI = GameObject.Find("WeaponData");
        ammoTMP = weaponDataUI.transform.Find("Ammo").GetComponent<TextMeshProUGUI>();
        weaponImageTra = weaponDataUI.transform.Find("WeaponImage");
        reloadProgressImage = weaponDataUI.transform.Find("Progress").GetComponent<Image>();
        soundManager = transform.Find("SoundManager").GetComponent<SoundManager>();
        
        _networkObject = GetComponent<NetworkObject>();
        _rb = GetComponent<Rigidbody>();
        _cineMachine = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();
        _mainCamera = Camera.main;
        _hpBar = GameObject.Find("HPBar").GetComponent<Slider>();
        _messageTMP = GameObject.Find("MessageText").GetComponent<TextMeshProUGUI>(); // それぞれの画面のオブジェクトがそれぞれ実行するのでそれぞれの画面に表示される
        _playerNameTMP = transform.Find("Player/PlayerName").GetComponent<TextMeshPro>();
        _reticle = GameObject.Find("Reticle").transform;
        
        if (!_networkObject.HasStateAuthority) return;
        // マウスカーソルを捕まえる
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        var cameraRoot = headBone.Find("PlayerCameraRoot");
        _cineMachine.Follow = cameraRoot;
    }

    private void Update()
    {
        if(!HasStateAuthority) return;
        
        //TODO: WeaponControlと折り合いをつける
        // 左クリックが押された・離されたときに状態更新
        if (_heldItemScript is not null && Input.GetMouseButtonDown(0)) _heldItemScript.IsShooting = true;
        if (_heldItemScript is not null && Input.GetMouseButtonUp(0)) _heldItemScript.IsShooting = false;
        
        if (Input.GetMouseButtonDown(1))
        {
            isAds = true;
            // heldItemSlot.localPosition = new Vector3(0, -0.15f, 0.5f);
            _adsTime = Runner.SimulationRenderTime; // TODO:lifeTimeに変更
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            isAds = false; 
            // heldItemSlot.localPosition = new Vector3(0.3f, -0.3f, 0.5f);
            _adsTime = Runner.SimulationRenderTime;// TODO:lifeTimeに変更
        }
        
        RotateHead();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        MovePosition();
        WeaponControl();

        _hpBar.value = Hp;
    }
    private void MovePosition()
    {
        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : normalSpeed; // 移動速度を取得
        Vector3 cameraForward = Vector3.Scale(
            _mainCamera.transform.forward,
            new Vector3(1, 0, 1)
        ).normalized; // カメラの正面方向のベクトルを取得

        // 移動キーの入力から移動ベクトルを取得
        Vector3 moveZ = cameraForward * Input.GetAxis("Vertical"); // 前後(カメラ基準)
        Vector3 moveX = headBone.transform.right * Input.GetAxis("Horizontal"); // 左右(カメラ基準)
        Vector3 moveDirection = moveX + moveZ;
        Vector3 move = moveDirection * speed;
        
        
        // 移動実行
        _rb.velocity = move  + new Vector3(0, _rb.velocity.y, 0);
        
        // 地面でスペースを押したらジャンプ
        if (Input.GetKey(KeyCode.Space) && _isOnGround)
        {
            _rb.AddForce(new Vector3(0, jump, 0), ForceMode.VelocityChange);
            _isOnGround = false;
        }
        
    }
    
    private void RotateHead()
    {
        transform.Rotate( // 体を左右
            new Vector2(
                0,
                Input.GetAxis("Mouse X") * mouseSensibilityHorizontal
            ), Space.World);
        if (Mathf.Abs(headBone.eulerAngles.x + Input.GetAxis("Mouse Y") * -mouseSensibilityVertical - 180f) > 95f) // 上下85°までのみ回転可 TODO:クランプに変更
        {
            headBone.Rotate( // 頭を上下
                new Vector2(
                    Input.GetAxis("Mouse Y") * -mouseSensibilityVertical,
                    0
                ), Space.Self);
        }

        heldItemSlot.rotation = headBone.rotation; // 武器も上下
    }
    
    private void WeaponControl()
    {
        if (_heldItemScript is null) return;
        
        if (Input.GetKey(KeyCode.R) && !_heldItemScript.isReloading)
        {
            _heldItemScript.Reload();
        }
        
        
        if (Runner.SimulationRenderTime - _adsTime < 0.1)
        {
            _cineMachine.m_Lens.FieldOfView =
                isAds ? 40 - (Runner.SimulationRenderTime - _adsTime) * _heldItemScript.adsMagnification : 30 + (Runner.SimulationRenderTime - _adsTime) * _heldItemScript.adsMagnification;// TODO:lifeTimeに変更する
        }
        
        // _heldItemScript.AsWeapon();
    }   
    
    public void PickUpItem(GameObject gameObj)
    {
        NetworkObject gameObjNetworkObject = gameObj.GetComponent<NetworkObject>();
        switch (gameObj.GetComponent<Item>().itemType)
        {
            case Item.ItemType.Weapon:
                if (heldItemSlot.childCount > 0 && gameObj.name == heldItemSlot.GetChild(0)?.name) // もし同じ武器を拾ったら
                {
                    _heldItemScript.ammo += _heldItemScript.capacity; // 1マガジン分弾薬増やす
                    soundManager.PlayPickupAmmoSound();
                    Runner.Despawn(gameObjNetworkObject);
                }
                else // もし同じ武器ではなかったら
                {
                    if(!Input.GetKey(KeyCode.E)) return; // 拾うキーを押していなかったら何もしない
                    
                    if (heldItemSlot.childCount > 0) Runner.Despawn(heldItemSlot.GetChild(0).GetComponent<NetworkObject>()); // 他の武器を持ってたら消す
                    // if(!gameObjNetworkObject.HasStateAuthority) gameObjNetworkObject.RequestStateAuthority();
                    _heldItemScript = gameObjNetworkObject.GetComponent<SmallArm>();
                    _heldItemScript.OnPickUp(heldItemSlot);
                    soundManager.PlayPickupGunSound();
                    weaponDataUI.transform.localScale = Vector3.one;
                    
                    RawImage weaponImage = weaponImageTra.GetComponent<RawImage>();
                    if (!weaponImage.enabled) weaponImage.enabled = true;
                    weaponImage.texture = (Texture)Resources.Load($"Images/Item/{gameObj.name.Replace("(Clone)", "")}");
                }
                
                break;
            case Item.ItemType.AidKit:
                int healAmount = gameObj.GetComponent<AidKit>().healAmount;
                Hp = Hp + healAmount > 100 ? 100 : Hp + healAmount;
                soundManager.PlayHealSound();
                Runner.Despawn(gameObjNetworkObject);
                
                break;
        }
        
        
    }

    //Debug
    [Networked(OnChanged = nameof(OnChangedMessage))] private string AvatarMessage { get; set; }
    private string _pastMessage;
    private int _num = 0;
    //引数に取った文字列を画面上に表示できるメソッド
    public void ShowMessage(string message)
    {
        _num = _num == 9 ? 0 : _num + 1;
        //messageの1文字目に0~9までの数字を挿入し、_pastMessageとmessageの内容が同じならそれに1を加えて表示する
        if (message == _pastMessage)
        {
            message = message + _num;
        }
        AvatarMessage = message;

        _pastMessage = message;
    }

    public static void OnChangedMessage(Changed<PlayerController> changed)
    {
        // changed.Behaviour._messageTMP.text = changed.Behaviour.AvatarMessage;
        if(changed.Behaviour._playerNameTMP is null) return;
        changed.Behaviour._playerNameTMP.text = changed.Behaviour.AvatarMessage;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void GetDamageRPC(int damage)
    {
        _networkDataManager ??= NetworkManager.Instance.NetworkDataManager;
        Hp -= (int)Math.Round(damage * damageFactor, MidpointRounding.AwayFromZero);
        

        if (Hp <= 0)
        {
            DeathRPC();
            _networkDataManager.RemoveFromSurvivorsListRPC(Runner.LocalPlayer.PlayerId);
        }
        else
        {
            soundManager.PlayGetDamageSound();
        }

    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void DeathRPC() //各クライアントで実行されるので演出とかにつかう
    {
        isDead = true;
        gameObject.layer = LayerMask.NameToLayer("Dead");
        soundManager.PlayDeadSound();
        transform.Find("Player/Model").gameObject.SetActive(false);
        Runner.Spawn(deadEffect, transform.position, Quaternion.identity, Runner.LocalPlayer);
    }
    
    
    public async void ShowHitEffect()
    {
        _reticle.localScale = new Vector3(0.75f, 0.75f, 0.75f);
        soundManager.PlayHitSound();
        await Task.Delay(100);
        _reticle.localScale = new Vector3(1, 1, 1);
    }
    
    
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag.Contains("Ground")) _isOnGround = true;
        if (other.gameObject.tag.Contains("Item"))
        {
            var netObj = other.gameObject.GetComponent<NetworkObject>();
            if (heldItemSlot.childCount > 0 && other.gameObject.name == heldItemSlot.GetChild(0)?.name || Input.GetKey(KeyCode.E) ||  (other.gameObject.TryGetComponent(out AidKit aidKit) && Hp < 100)) // 仮置きでPickUpItemと同じ条件も使う
            {
                if (netObj.HasStateAuthority) PickUpItem(other.gameObject);
                else netObj.RequestStateAuthority();
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag.Contains("Ground")) _isOnGround = false;
    }
}
