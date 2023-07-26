using System;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class SmallArm : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public float fireRate;
    public float damage;
    public float bulletSpeed;
    public float capacity;
    public GameObject bulletObject;
    // 発射する弾のオブジェクトに着弾時の効果とか仕込もう

    [HideInInspector]
    public float remainingBullets;

    private bool _isPickUpped;
    private float _interval;
    private bool _isShooting;
    private Rigidbody _rb;
    private Transform _owner;
    private Transform _muzzle;
    private ParticleSystem _muzzleFlash;
    private Transform _headBone;
    
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // transform.SetParent(PhotonView.Find((int)info.photonView.InstantiationData[0]).transform.GetComponent<PlayerController>().heldItemSlot, false);
    }

    public void Start()
    {
        tag = "Item";
    }
    
    
    public void OnPickUp(Transform itemSlot)
    {
        transform.SetParent(itemSlot, false);
        // PhotonView.Find((int)info.photonView.InstantiationData[0]).GetComponent<PlayerController>().text.text = "a";
        _owner = transform.root;
        Debug.Log(itemSlot);
        _muzzle = transform.Find("Muzzle");
        _muzzleFlash = transform.Find("MuzzleFlashEffect").GetComponent<ParticleSystem>();
        _headBone = _owner.GetComponent<PlayerController>().headBone;

        GetComponent<CapsuleCollider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        
        photonView.RequestOwnership();
        tag = "Weapon";
        _isPickUpped = true;
        transform.localPosition = Vector3.zero;
    }
    
    private void Update()
    {
        if (_isPickUpped)
        {
            AsWeapon();
        }
        else
        {
            AsItem();
        }
    }

    private void AsWeapon()
    {
        transform.rotation = _headBone.rotation;
        transform.position = transform.parent.position;
        // 小銃弾をすべて同期することは難しいので、所有者のみが残弾管理等を行い他のクライアントは演出的に発砲のみを行う
        
        if (_interval > 0) _interval -= Time.deltaTime; // インターバルを減らす
        if (_isShooting && _interval <= 0) // 発射中かつインターバルがなければ
        {   // インターバルがなくなったので撃つ
            _interval = fireRate; // インターバル設定
            
            // GameObject bullet = Instantiate(bulletObject, _muzzle.position, _owner.GetComponent<PlayerController>().headBone.rotation); // 弾スポーン
            GameObject bullet = Instantiate(bulletObject, _headBone.position + _headBone.forward, _headBone.rotation); // 弾スポーン
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletSpeed, ForceMode.VelocityChange); // 弾加速
            Destroy(bullet, 5f);
            _muzzleFlash.Play();
            
            if (photonView.IsMine)
            { // 所有者なら 
            }
            else
            { // 他のクライアントなら
                
            }
        }
    }

    private void AsItem()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 0.25f, transform.eulerAngles.z);
    }
    
    public void OpenFire(bool status)
    {
        photonView.RPC(nameof(OpenFireRpc), RpcTarget.All, status);
    }

    [PunRPC]
    private void OpenFireRpc(bool status)
    {
        _isShooting = status;
    }
}
