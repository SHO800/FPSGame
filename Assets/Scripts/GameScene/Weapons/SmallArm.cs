using System;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class SmallArm : Item, IPunInstantiateMagicCallback
{
    public float fireRate;
    public float damage;
    public float bulletSpeed;
    public int capacity;
    public float reloadTime;
    public GameObject bulletObject;
    // 発射する弾のオブジェクトに着弾時の効果とか仕込もう

    [HideInInspector] public int ammoInMagazine;
    [HideInInspector] public int ammo;
    [HideInInspector] public bool isReloading;
    

    private bool _isPickUpped;
    private float _interval;
    private bool _isShooting;
    private Rigidbody _rb;
    private Transform _owner;
    private Transform _muzzle;
    private ParticleSystem _muzzleFlash;
    private Transform _headBone;
    private float _startReloadTime;
    private PlayerController _ownerController;
    private AudioSource _audioSource;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // transform.SetParent(PhotonView.Find((int)info.photonView.InstantiationData[0]).transform.GetComponent<PlayerController>().heldItemSlot, false);
    }

    public void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        
        tag = "Item"; // 初期でアイテム状態
        ammoInMagazine = capacity; // 初期からマガジン一個分入ってる
        ammo = capacity; // 予備弾薬もついてくる
        this.itemType = ItemType.Weapon;
    }
    
    
    public void OnPickUp(Transform itemSlot)
    {
        transform.SetParent(itemSlot, false);
        // PhotonView.Find((int)info.photonView.InstantiationData[0]).GetComponent<PlayerController>().text.text = "a";
        _owner = transform.root;
        _muzzle = transform.Find("Muzzle");
        _muzzleFlash = transform.Find("MuzzleFlashEffect").GetComponent<ParticleSystem>();
        _ownerController = _owner.GetComponent<PlayerController>(); 
        _headBone = _ownerController.headBone;

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
        if (_isShooting && _interval <= 0 && ammoInMagazine > 0 && !isReloading) // 発射中かつインターバルがないかつ残弾があるかつリロード中でない
        {   // インターバルがなくなったので撃つ
            _interval = fireRate; // インターバル設定
            
            // GameObject bullet = Instantiate(bulletObject, _muzzle.position, _owner.GetComponent<PlayerController>().headBone.rotation); // 弾スポーン
            GameObject bullet = Instantiate(bulletObject, _headBone.position + _headBone.forward, _headBone.rotation); // 弾スポーン
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletSpeed, ForceMode.VelocityChange); // 弾加速
            Destroy(bullet, 5f);
            _muzzleFlash.Play();
            _audioSource.Play();
            ammoInMagazine--;

            if (photonView.IsMine)
            { // 所有者なら 
            }
            else
            { // 他のクライアントなら
                
            }
        }

        if(ammoInMagazine == 0 && !isReloading) Reload(); //残弾がなくなったら自動的にリロード
        
        
        if (isReloading)
        {
            _ownerController.weaponImageTra.eulerAngles = new Vector3(0, 0, (Time.time - _startReloadTime) / reloadTime * -360);
            _ownerController.weaponImageTra.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
        else
        {
            _ownerController.weaponImageTra.eulerAngles = new Vector3(0, 0, 0);
            _ownerController.weaponImageTra.localScale = new Vector3(0.2f, 0.2f, 0.1f);
            
        }
        if (isReloading && (Time.time - _startReloadTime) >= reloadTime) //リロード終了時
        {
            isReloading = false;
            
            int amount = ammo >= capacity - ammoInMagazine ? capacity - ammoInMagazine : ammo;; // 予備弾薬が装填する数より多ければマガジンいっぱい、でなければ予備弾薬ぜんぶ
            ammoInMagazine += amount; // 予備弾薬が1マガジン分以上あるならその分装填、無いならあるだけ装填
            ammo -= amount;
        }
        _ownerController.ammoTMP.text = $"{ammoInMagazine} / {ammo}";
    }

    private void AsItem()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 0.25f, transform.eulerAngles.z);
    }
    
    public void OpenFire(bool status)
    {
        photonView.RPC(nameof(OpenFireRpc), RpcTarget.All, status);
    }

    public void Reload()
    {
        if (ammo <= 0) return; // 予備弾薬がなければ無理
        isReloading = true;
        _startReloadTime = Time.time;
    }

    [PunRPC]
    private void OpenFireRpc(bool status)
    {
        _isShooting = status;
    }
}
