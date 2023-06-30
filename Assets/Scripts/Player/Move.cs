// using System;
// using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public float normalSpeed = 5f;
    public float sprintSpeed = 10f;
    public float jump = 8f;
    
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

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        headHorizontal = transform.Find("HeadHorizontal");
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
        if(rb.velocity.magnitude <= speed) rb.AddForce(moveDirection, ForceMode.VelocityChange);
        
        // 体を回転させる
        /*
        float headHorizontalAngleY = headHorizontal.eulerAngles.y;
        float bodyAngleY = transform.eulerAngles.y;
        if (moveDirection != Vector3.zero)
        {
            transform.Rotate(0, headHorizontal.localEulerAngles.y, 0);
        }
        Debug.Log(
            $"head: {headHorizontalAngleY.ToString()}\n body: {bodyAngleY.ToString()}\n headlocal:{headHorizontal.localEulerAngles.y}"
            );
        
        if (Mathf.Abs(headHorizontalAngleY - bodyAngleY) > 90f)
        {
            transform.Rotate(0, headHorizontalAngleY, 0);
        }
        */
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

