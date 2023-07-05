using System.Security.Cryptography.X509Certificates;
using Photon.Pun;
using UnityEngine;

public class SmallArm : MonoBehaviour
{
    public float fireRate;
    public float damage;
    public float bulletSpeed;
    public float capacity;
    public string bulletObject = "Weapons/Bullets/NormalBullet";
    // 発射する弾のオブジェクトに着弾時の効果とか仕込もう

    [HideInInspector]
    public float remainingBullets;
    
    private float _interval;
    private bool _isShooting;
    private Rigidbody _rb;
    private Transform _owner;
    private Transform _muzzle;


    private void Start()
    {
        _muzzle = transform.Find("Muzzle");
    }
    
    private void Update()
    {
        if (_interval > 0) _interval -= Time.deltaTime; // インターバルを減らす
        if (_isShooting && _interval <= 0) // 発射中かつインターバルがなければ
        {
            _interval = fireRate; // インターバル設定
            GameObject bullet = PhotonNetwork.Instantiate(bulletObject, _muzzle.position, _owner.GetComponent<PlayerController>().headBone.rotation); // 弾スポーン
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletSpeed, ForceMode.VelocityChange); // 弾加速
            Destroy(bullet, 5f);
        }
        
    }

    public void OpenFire(Transform owner)
    {
        _owner = owner; // 銃を使った人
        _isShooting = true;
    }

    public void CloseFire()
    {
        _isShooting = false;
    }
}
