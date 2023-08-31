using Fusion;
using UnityEngine;

public class NormalBullet : NetworkBehaviour
{
    [HideInInspector] public int damage;
    [HideInInspector] public PlayerController owner;
    [SerializeField] private GameObject hitEffect;
    [Networked] private TickTimer Life { get; set; }

    public void Init(PlayerController owner, int damage)
    {
        this.owner = owner;
        this.damage = damage;
        Life = TickTimer.CreateFromSeconds(Runner, 5.0f);
    }

    public override void FixedUpdateNetwork()
    {
        if(!HasStateAuthority) return;
        
        
        if (transform.position.y < -100 || Life.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

    // Triggerだと貫通することがあるのでCollisionにしてる
    // Enterだと貫通することがあるのでStayにしてる
    private void OnCollisionStay(Collision other)
    {
        
        if(!HasStateAuthority) return;
        Runner.Spawn(hitEffect, transform.position, Quaternion.LookRotation(other.contacts[0].normal)); // 何かにぶつかったら着弾エフェクト
        
        if (other.gameObject.tag.Contains("Player") && other.gameObject != owner.gameObject) //敵にヒットしたとき
        {
            other.gameObject.GetComponent<PlayerController>().GetDamageRPC(damage); // ダメージを与える
            owner.ShowHitEffect();
        }

        if (Object is not null)
        {
            Runner.Despawn(Object);
        }
    }

    
}
