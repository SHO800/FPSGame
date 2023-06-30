using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class PlayerController : MonoBehaviour
{
    public float normalSpeed = 3.0f;
    public float sprintSpeed = 5.0f;
    public float jump = 8f;
    public float mouseSensibilityHorizontal = 1f;
    public float mouseSensibilityVertical = 0.5f;

    private Transform headBone;
    private Rigidbody rb;
    private Animator anim;
    private float speed;
    private Camera mainCamera;
    private Vector3 cameraForward;
    private Vector3 moveX;
    private Vector3 moveZ;
    private Vector3 moveDirection = Vector3.zero;
    private Transform headHorizontal;
    private bool isOnGround;
    private float localHeadAngle;


    private void Start()
    {
        rb = transform.GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        headBone = GameObject.Find("Root/Hips/Spine/Spine1/Neck/Head").transform;
    }
    
    private void Update()
    {
        // 移動処理
        speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : normalSpeed; // 移動速度を取得
        cameraForward = Vector3.Scale(
            mainCamera.transform.forward, 
            new Vector3(1, 0, 1)
        ).normalized; // カメラの正面方向のベクトルを取得
        
        // 移動キーの入力から移動ベクトルを取得
        moveZ = cameraForward * (Input.GetAxis("Vertical") * 1); // 前後(カメラ基準)
        moveX = mainCamera.transform.right * (Input.GetAxis("Horizontal") * 1); // 左右(カメラ基準)
        moveDirection = moveX + moveZ;

        // 地面でスペースを押したらジャンプ
        if (Input.GetKey(KeyCode.Space) && isOnGround) moveDirection.y = jump;
        
        
        // 移動実行
        if(rb.velocity.magnitude <= speed) rb.AddForce(moveDirection * 0.25f, ForceMode.VelocityChange);
        
        // 視点移動
        transform.Rotate( // 体を左右
            new Vector2(
                0,
                Input.GetAxis("Mouse X") * mouseSensibilityHorizontal
            ), Space.World);
        headBone.Rotate( // 頭を上下
            new Vector2(
                Input.GetAxis("Mouse Y") * -mouseSensibilityVertical,
                0
            ), Space.Self);
        
        // 頭の角度を取得
        // localHeadAngle = headBone.eulerAngles.y - transform.eulerAngles.y;
        // localHeadAngle = localHeadAngle > 180f ? localHeadAngle - 360f : localHeadAngle;
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag.Contains("Ground"))
        {
            isOnGround = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag.Contains("Ground"))
        {
            isOnGround = false;
        }
    }
}
