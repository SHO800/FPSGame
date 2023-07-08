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
    
    private float _interval;
    private bool _isShooting;
    private Rigidbody _rb;
    private Transform _owner;
    private Transform _muzzle;
    
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        transform.SetParent(PhotonView.Find((int)info.photonView.InstantiationData[0]).transform.GetComponent<PlayerController>().heldItemSlot, false);
        PhotonView.Find((int)info.photonView.InstantiationData[0]).GetComponent<PlayerController>().text.text = "a";
        _owner = transform.root;
        _muzzle = transform.Find("Muzzle");
    }
    
    private void Update()
    {
        // 小銃弾をすべて同期することは難しいので、所有者のみが残弾管理等を行い他のクライアントは演出的に発砲のみを行う
        
        if (_interval > 0) _interval -= Time.deltaTime; // インターバルを減らす
        if (_isShooting && _interval <= 0) // 発射中かつインターバルがなければ
        {   // インターバルがなくなったので撃つ
            _interval = fireRate; // インターバル設定
            
            GameObject bullet = Instantiate(bulletObject, _muzzle.position, _owner.GetComponent<PlayerController>().headBone.rotation); // 弾スポーン
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletSpeed, ForceMode.VelocityChange); // 弾加速
            Destroy(bullet, 5f);
            
            if (photonView.IsMine)
            { // 所有者なら 
            }
            else
            { // 他のクライアントなら
                
            }
        }
        
    }
    
    public void OpenFire(bool status)
    {
        Debug.Log($"OpenFire({status})");
        photonView.RPC(nameof(OpenFireRpc), RpcTarget.All, status);
    }

    [PunRPC]
    private void OpenFireRpc(bool status)
    {
        Debug.Log($"RPC({status})");
        _isShooting = status;
    }
}
