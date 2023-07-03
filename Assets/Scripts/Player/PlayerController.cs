using System;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class PlayerController : MonoBehaviourPun
{
    public static Transform SelfPlayer;
    
    public float normalSpeed = 3.0f;
    public float sprintSpeed = 5.0f;
    public float jump = 1f;
    public float mouseSensibilityHorizontal = 1f;
    public float mouseSensibilityVertical = 0.5f;
    public GameObject gun1;
    public Transform headBone;
    // public float handItem;

    private Rigidbody _rb;
    private Camera _mainCamera;
    private Animator _anim;
    private bool _isOnGround;
    private Transform _handSlot;
    private SmallArm _handItemScript;
    

    private void Awake()
    {
        SelfPlayer = transform;
    }


    private void Start()
    {
        _rb = transform.GetComponent<Rigidbody>();
        _mainCamera = Camera.main;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        headBone = GameObject.Find("Root/Hips/Spine/Spine1/Neck/Head").transform;
        _handSlot = headBone.Find("HandSlot");
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        MovePosition();
        RotateHead();
        


        // キーが押されたら銃を取り出す
        if (Input.GetKeyDown("1"))
        {
            if (_handSlot.childCount > 0) Destroy(_handSlot.GetChild(0)); // 手持ちスロットにすでになにか持っていたら消す
            GameObject handItem = Instantiate(gun1, new Vector3(0f, 0f, 0f), Quaternion.identity); // 銃を呼び出す
            handItem.transform.SetParent(_handSlot, false); // 銃の親を手持ちスロットにする
            _handItemScript = handItem.GetComponent<SmallArm>();
        }

        if (Input.GetMouseButtonDown(0))
        {
            _handItemScript.OpenFire(transform);
        }

        if (Input.GetMouseButtonUp(0))
        {
            _handItemScript.CloseFire();
        }
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
        
        // 移動実行
        _rb.velocity = moveDirection * speed + new Vector3(0, _rb.velocity.y, 0);
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
