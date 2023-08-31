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
    public float adsMagnification = 100;
    public float audioPitch = 1;
    // 発射する弾のオブジェクトに着弾時の効果とか仕込もう
    
    [HideInInspector] public int ammo;
    [HideInInspector] public bool isReloading;
    
    [HideInInspector, Networked] public int AmmoInMagazine { get; set; }
    [HideInInspector, Networked] public bool IsShooting { get; set; }
    [Networked] private TickTimer Interval { get; set; }
    [Networked] private bool IsPickUpped { get; set; }
    
    private bool _isShooting;
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
        _owner = transform.root;
        _ownerController = _owner.GetComponent<PlayerController>();
        _headBone = _ownerController.headBone;
    
        ChangeTagRPC("Weapon");
        GetComponent<CapsuleCollider>().enabled = false;
        Rb.isKinematic = true;
        IsPickUpped = true;
        transform.localPosition = Vector3.zero;
        transform.rotation = _headBone.rotation;
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void ChangeTagRPC(string changeTag)
    {
        tag = changeTag;
    }


    protected override void Update()
    {
        if (!IsPickUpped) base.Update();
        _audioSource.pitch = audioPitch * Time.timeScale; //ゲーム終了時のスローに音を合わせる
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

            NetworkObject bullet = Runner.Spawn(bulletObject, _headBone.position + _headBone.forward, _headBone.rotation, Runner.LocalPlayer,
                (runner, o) => { o.GetComponent<NormalBullet>().Init(_ownerController, damage);}); // 弾スポーン
            bullet.gameObject.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletSpeed, ForceMode.VelocityChange); // 弾加速

            DoEffectRPC();
            AmmoInMagazine--;
        }
        
        if(AmmoInMagazine == 0 && !isReloading) Reload(); //残弾がなくなったら自動的にリロード
            
            
        if (isReloading)
        {
            
            _ownerController.reloadProgressImage.enabled = true;
            _ownerController.reloadProgressImage.fillAmount = (Runner.SimulationRenderTime - _startReloadTime) / reloadTime;
            _ownerController.weaponImageTra.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
        else
        {
            _ownerController.reloadProgressImage.enabled = false;
            _ownerController.weaponImageTra.localScale = new Vector3(0.15f, 0.15f, 0.1f);
                
        }
        if (isReloading && (Runner.SimulationRenderTime - _startReloadTime) >= reloadTime) //リロード終了時
        {
            isReloading = false;
            PlayReloadEndSound();
            
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
        if (ammo <= 0 || AmmoInMagazine == capacity) return; // 予備弾薬がなければ無理
        isReloading = true;
        PlayReloadStartSound();
        _startReloadTime = Runner.SimulationRenderTime;
    }
    
    private void PlayReloadStartSound()
    {
        _ownerController.soundManager.PlayReloadStartSound();
    }
    
    private void PlayReloadEndSound()
    {
        _ownerController.soundManager.PlayReloadEndSound();
    }
}
