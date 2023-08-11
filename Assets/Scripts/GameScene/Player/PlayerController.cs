using Cinemachine;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    public static Transform SelfPlayer;
    
    public float normalSpeed = 3.0f;
    public float sprintSpeed = 5.0f;
    public float jump = 1f;
    public float mouseSensibilityHorizontal = 1f;
    public float mouseSensibilityVertical = 0.5f;
    


    [HideInInspector] public Transform headBone;
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
    private Camera _mainCamera;
    private Animator _anim;
    private SmallArm _heldItemScript;
    private bool _isOnGround;
    private string _nickName;
    private CinemachineVirtualCamera _cineMachine;
    private float _adsTime;
    private TextMeshProUGUI _messageTMP;
    private Slider _hpBar;

    public TextMeshPro text;

    private void Awake()
    {
        SelfPlayer = transform;
    }


    private void Start()
    {
        //キャッシュ
        _rb = GetComponent<Rigidbody>();
        _mainCamera = Camera.main;
        headBone = transform.Find("Root/Hips/Spine/Spine1/Neck/Head");
        heldItemSlot = headBone.Find("HeldItemSlot");
        _cineMachine = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();
        weaponDataUI = GameObject.Find("WeaponData");
        ammoTMP = weaponDataUI.transform.Find("Ammo").GetComponent<TextMeshProUGUI>();
        _messageTMP = GameObject.Find("MessageText").GetComponent<TextMeshProUGUI>();
        weaponImageTra = weaponDataUI.transform.Find("WeaponImage");
        _hpBar = GameObject.Find("HPBar").GetComponent<Slider>();


        // マウスカーソルを囚える
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        text = transform.Find("PlayerName").GetComponent<TextMeshPro>();
        
        if (photonView.IsMine) _nickName = PhotonNetwork.NickName;
        text.text = _nickName;
        ResisterGameManager(_nickName);
        if (PhotonNetwork.IsMasterClient && !GameManager.IsGameStarted)
        {
            _messageTMP.text = "あなたはゲームマスターです。Enterを押してゲームを開始します。\n";
        }
        else
        {
            _messageTMP.text = "ゲームマスターがゲームを開始するまでお待ち下さい。\n";
        }
        
        
        
    }
    
    

    private void Update()
    {
        if (!photonView.IsMine) return;
        
        if (PhotonNetwork.IsMasterClient && !GameManager.IsGameStarted && Input.GetKeyDown(KeyCode.Return))
        {
            photonView.RPC(nameof(Message), RpcTarget.All, "ゲームマスターがゲームを開始しました。");
            GameManager.GameStart();
        }
        Debug.Log(GameManager.Survivor.ToString());

        MovePosition();
        RotateHead();

        // 時間的に没
        // // キーが押されたら銃を取り出す
        // for (int i = 1; i <= 9; i++)
        // {
        //     if (Input.GetKeyDown(i.ToString())) // 数字キーiが押されていたら
        //     {
        //         // Debug.Log(_playerData.StoredItems);
        //         // Debug.Log(_playerData.StoredItems[i - 1]);
        //         ref GameObject storedItem = ref _playerData.StoredItems[i - 1]; // インベントリに格納中の方の選択された番号のアイテム
        //         
        //         if (heldItemSlot.childCount > 0) // もし手になにか持っているなら
        //         {
        //             GameObject heldItem = heldItemSlot.GetChild(0).gameObject; // 手に持ってたアイテム
        //             if (heldItem.name == storedItem.name) continue; // もし押されたキーと同じアイテムをすでに持っていたら何もせずにスキップ
        //             
        //             // Debug.Log("newItemSelected");
        //             
        //             if (heldItemSlot.childCount > 0) // なにかアイテムを持っていたら今持っているアイテムの状態を保存する (持っているアイテムを削除するのは各クライアント側で行う)
        //             {
        //                 storedItem = heldItem;
        //             }
        //         }
        //         
        //         // アイテムを各クライアントで呼び出す
        //         // photonView.RPC(nameof(SetHeldItem), RpcTarget.All, i-1);
        //         SetHeldItem(0);
        //     }
        // }
        
        
        if(heldItemSlot.childCount > 0) WeaponControl();
        else if (weaponDataUI.activeSelf) weaponDataUI.transform.localScale = Vector3.zero;

        _hpBar.value = hp;
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
            if (Input.GetKey(KeyCode.Space) && _isOnGround)
            {
                _rb.AddForce(new Vector3(0, jump, 0), ForceMode.VelocityChange);
                // _isOnGround = false;
            }
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

    [PunRPC]
    private void Message(string message)
    {
        _messageTMP.text += message + "\n";
    }
    
    [PunRPC]
    private void PickUpItem(int id)
    {
        // if
        GameObject gameObj = PhotonView.Find(id).gameObject;
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
    
    //没
    // private void SetHeldItem(int itemNum)
    // {
    //     if (heldItemSlot.childCount > 0) // もしすでに手になにか持っていたら
    //     {
    //         Destroy(heldItemSlot.GetChild(0).gameObject); // 抹消
    //     } 
    //     // GameObject newHeldItem = Instantiate(_playerData.StoredItems[itemNum], new Vector3(0f, 0f, 0f), Quaternion.identity); // 指定されたアイテムを呼び出す
    //     GameObject newHeldItem = PhotonNetwork.Instantiate(gun1, new Vector3(0f, 0f, 0f), Quaternion.identity, 0, new object[1]{photonView.ViewID}); // 指定されたアイテムを呼び出す
    //     // newHeldItem.transform.SetParent(_heldItemSlot, false); // 手持ちスロットに呼び出したアイテムを配置する
    //     _heldItemScript = newHeldItem.GetComponent<SmallArm>(); 
    // }
    //
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
        if (hp <= 0) Dead();
        
    }

    // [PunRPC]
    private void Dead()
    {
        Debug.Log("Dead");
        if(photonView.IsMine) photonView.RPC(nameof(Message), RpcTarget.All, $"{PhotonNetwork.NickName}が死亡");
        transform.localScale = Vector3.zero;
        _rb.useGravity = false;
        isDead = true;
        transform.position = new Vector3(transform.position.x, transform.position.y + 10, transform.position.z);
        GameManager.Survivor.Remove(PhotonNetwork.NickName);
        if (GameManager.Survivor.Count <= 1)
        {
            
            GameEnd(GameManager.Survivor[0]);
            // photonView.RPC(nameof(Message), RpcTarget.All, $"ゲーム終了: {GameManager.Survivor[0]}が生存しました。");
            // photonView.RPC(nameof(Message), RpcTarget.All, "5秒後にセレクター画面へ移行します...");
            // photonView.RPC(nameof(BackLobby), RpcTarget.All);
        }
        
    }
    
    [PunRPC]
    private void BackLobby()
    {

        void Back()
        {
            SceneManager.LoadScene("StartScene");
        }
        Invoke("Back", 5);
    }

    [PunRPC]
    private void ResisterGameManager(string nickname)
    {
        GameManager.Survivor.Add(nickname);
    }

    [PunRPC]
    private void GameEnd(string nickname)
    {
        ;
        Message($"ゲーム終了: {GameManager.Survivor[0]}が生存しました。");
        Message("5秒後にセレクター画面へ移行します...");
        BackLobby();
    }
    
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // stream.SendNext(hp);
            stream.SendNext(_nickName);
        }
        else
        {
            // _hpBar.value = (float)stream.ReceiveNext();
            text.text = (string)stream.ReceiveNext();
        }
    }
    
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag.Contains("Ground")) _isOnGround = true;
        if (other.gameObject.tag.Contains("Item") && Input.GetKey(KeyCode.E))
        {
            photonView.RPC(nameof(PickUpItem), RpcTarget.All, other.gameObject.GetPhotonView().ViewID);
        }
    }


    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag.Contains("Ground")) _isOnGround = false;
    }
    
}
