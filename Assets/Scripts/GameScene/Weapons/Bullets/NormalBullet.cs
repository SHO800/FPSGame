using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class NormalBullet : NetworkBehaviour
{
    public int damage;
    [Networked] private TickTimer Life { get; set; }

    public void Init()
    {
        Life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        Debug.Log("Init in bullet" + Object.HasStateAuthority);
    }

    public override void Spawned()
    {
        Debug.Log("Spawned in bullet");    
    }

    private void Update()
    {
        Debug.Log("Update in bullet" + Object.HasStateAuthority);
    }
    
    public override void FixedUpdateNetwork()
    {
        if(!HasStateAuthority) return;
        
        Debug.Log("FixedUpdateNetwork in bullet");
        
        if (transform.position.y < -100 || Life.Expired(Runner))
        {
            Debug.Log("Despawn in FixedUpdateNetwork");
            Runner.Despawn(Object);
        }
    }

    // Triggerだと貫通することがあるのでCollisionにしてる
    // Enterだと貫通することがあるのでStayにしてる
    private void OnCollisionStay(Collision other)
    {
        if(!HasStateAuthority) return;
        
        Debug.Log("collision in bullet");
        
        if (other.gameObject.tag.Contains("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().GetDamageRPC(damage);
        }
        if (Object is not null) Runner.Despawn(Object);
    }

    
}
