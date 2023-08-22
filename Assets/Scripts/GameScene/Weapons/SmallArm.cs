using Fusion;
using UnityEngine;

public class SmallArm : Item
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
    
    [Networked] private TickTimer Interval { get; set; }
    
    private bool _isPickUpped;
    private bool _isShooting;
    // private Rigidbody _rb;
    private Transform _owner;
    private Transform _muzzle;
    private ParticleSystem _muzzleFlash;
    private Transform _headBone;
    private float _startReloadTime;
    private PlayerController _ownerController;
    private AudioSource _audioSource;
    
    
    
    public void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        
        tag = "Item"; // 初期でアイテム状態
        ammoInMagazine = capacity; // 初期からマガジン一個分入ってる
        ammo = capacity; // 予備弾薬もついてくる
        itemType = ItemType.Weapon;
        _muzzle = transform.Find("Muzzle");
        _muzzleFlash = transform.Find("MuzzleFlashEffect").GetComponent<ParticleSystem>();
    }
    
    
    public void OnPickUp(Transform itemSlot)
    {
        transform.SetParent(itemSlot, false);
        // PhotonView.Find((int)info.photonView.InstantiationData[0]).GetComponent<PlayerController>().text.text = "a";
        _owner = transform.root;
        _ownerController = _owner.GetComponent<PlayerController>(); 
        _headBone = _ownerController.headBone;
    
        GetComponent<CapsuleCollider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        
        // photonView.RequestOwnership();
        tag = "Weapon";
        _isPickUpped = true;
        transform.localPosition = Vector3.zero;
        transform.rotation = _headBone.rotation;
        // transform.position = transform.parent.position;
    }
    
    protected override void Update()
    {
        if (_isPickUpped)
        {
            // AsWeapon();
        }
        else
        {
            base.Update();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (_muzzleFlash is null )return;

        if (_isPickUpped)
        {
            AsWeapon();
        }
    }
    
    private void AsWeapon()
    {
        
        // 小銃弾をすべて同期することは難しいので、所有者のみが残弾管理等を行い他のクライアントは演出的に発砲のみを行う
        
        if (_ownerController.IsShooting && Interval.ExpiredOrNotRunning(Runner) && ammoInMagazine > 0 && !isReloading) // 発射中かつインターバルがないかつ残弾があるかつリロード中でない
        {   // インターバルがなくなったので撃つ
            Interval = TickTimer.CreateFromSeconds(Runner, fireRate); // インターバル設定
            // GameObject bullet = Instantiate(bulletObject, _muzzle.position, _owner.GetComponent<PlayerController>().headBone.rotation); // 弾スポーン
            NetworkObject bullet = Runner.Spawn(bulletObject, _headBone.position + _headBone.forward, _headBone.rotation, null,
                (runner, o) => { o.GetComponent<NormalBullet>().Init();}); // 弾スポーン
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletSpeed, ForceMode.VelocityChange); // 弾加速
            bullet.GetComponent<NormalBullet>().damage = damage;
            Runner.Despawn(bullet.GetComponent<NetworkObject>());
            _muzzleFlash.Play();
            _audioSource.Play();
            ammoInMagazine--;
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

    public void OpenFire(bool status)
    {
        // photonView.RPC(nameof(OpenFireRpc), RpcTarget.All, status);
    }
    
    public void Reload()
    {
        if (ammo <= 0) return; // 予備弾薬がなければ無理
        isReloading = true;
        _startReloadTime = Time.time;
    }
}
