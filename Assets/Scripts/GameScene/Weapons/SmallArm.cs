using Fusion;
using UnityEngine;

public class SmallArm : Item
{
    public float fireRate;
    public int damage;
    public float bulletSpeed;
    public int capacity;
    public float reloadTime;
    public GameObject bulletObject;
    // 発射する弾のオブジェクトに着弾時の効果とか仕込もう
    
    [HideInInspector] public int ammo;
    [HideInInspector] public bool isReloading;
    
    [HideInInspector, Networked] public int AmmoInMagazine { get; set; }
    [HideInInspector, Networked] public bool IsShooting { get; set; }
    [Networked] private TickTimer Interval { get; set; }
    [Networked] private bool IsPickUpped { get; set; }
    
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
        AmmoInMagazine = capacity; // 初期からマガジン一個分入ってる
        ammo = capacity; // 予備弾薬もついてくる
        itemType = ItemType.Weapon;
        _muzzle = transform.Find("Muzzle");
        _muzzleFlash = transform.Find("MuzzleFlashEffect").GetComponent<ParticleSystem>();
        Rb = GetComponent<Rigidbody>();
    }
    
    
    public void OnPickUp(Transform itemSlot)
    {
        transform.SetParent(itemSlot, false);
        // PhotonView.Find((int)info.photonView.InstantiationData[0]).GetComponent<PlayerController>().text.text = "a";
        _owner = transform.root;
        _ownerController = _owner.GetComponent<PlayerController>();
        _headBone = _ownerController.headBone;
    
        GetComponent<CapsuleCollider>().enabled = false;
        Rb.isKinematic = true;
        tag = "Weapon";
        IsPickUpped = true;
        transform.localPosition = Vector3.zero;
        transform.rotation = _headBone.rotation;
    }


    protected override void Update()
    {
        if (!IsPickUpped) base.Update();
    }

    public override void FixedUpdateNetwork()
    {
        if (_muzzleFlash is null) return;

        if (IsPickUpped) AsWeapon();
    }
    
    private void AsWeapon()
    {
        if (!HasStateAuthority) return;
        // 小銃弾をすべて同期することは難しいので、所有者が残弾管理等を行い他のクライアントは演出のみを行う
        if (IsShooting && Interval.ExpiredOrNotRunning(Runner) && AmmoInMagazine > 0 && !isReloading) // 発射中かつインターバルがないかつ残弾があるかつリロード中でない
        {   // インターバルがなくなったので撃つ
            Interval = TickTimer.CreateFromSeconds(Runner, fireRate); // インターバル設定

            NetworkObject bullet = Runner.Spawn(bulletObject, _headBone.position + _headBone.forward, _headBone.rotation, null,
                (runner, o) => { o.GetComponent<NormalBullet>().Init();}); // 弾スポーン
            bullet.gameObject.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletSpeed, ForceMode.VelocityChange); // 弾加速
            bullet.gameObject.GetComponent<NormalBullet>().damage = damage;
            
            DoEffectRPC();
            AmmoInMagazine--;
        }
        
        if(AmmoInMagazine == 0 && !isReloading) Reload(); //残弾がなくなったら自動的にリロード
            
            
        if (isReloading)
        {
            _ownerController.weaponImageTra.eulerAngles = new Vector3(0, 0, (Runner.SimulationRenderTime - _startReloadTime) / reloadTime * -360);
            _ownerController.weaponImageTra.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
        else
        {
            _ownerController.weaponImageTra.eulerAngles = new Vector3(0, 0, 0);
            _ownerController.weaponImageTra.localScale = new Vector3(0.2f, 0.2f, 0.1f);
                
        }
        if (isReloading && (Runner.SimulationRenderTime - _startReloadTime) >= reloadTime) //リロード終了時
        {
            isReloading = false;
                
            int amount = ammo >= capacity - AmmoInMagazine ? capacity - AmmoInMagazine : ammo;; // 予備弾薬が装填する数より多ければマガジンいっぱい、でなければ予備弾薬ぜんぶ
            AmmoInMagazine += amount; // 予備弾薬が1マガジン分以上あるならその分装填、無いならあるだけ装填
            ammo -= amount;
        }
        _ownerController.ammoTMP.text = $"{AmmoInMagazine} / {ammo}";
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void DoEffectRPC()
    {
        _muzzleFlash.Play();
        _audioSource.Play();
    }

    public void Reload()
    {
        if (ammo <= 0) return; // 予備弾薬がなければ無理
        isReloading = true;
        _startReloadTime = Runner.SimulationRenderTime;
    }
}
