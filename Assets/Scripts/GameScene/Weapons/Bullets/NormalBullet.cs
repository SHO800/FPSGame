using Fusion;
using UnityEngine;

public class NormalBullet : NetworkBehaviour
{
    public float damage;
    [Networked] private TickTimer Life { get; set; }

    public void Init()
    {
        Life = TickTimer.CreateFromSeconds(Runner, 5.0f);
    }
    
    public override void FixedUpdateNetwork()
    {
        if (transform.position.y < -100 || Life.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

    // Triggerだと貫通することがあるのでCollisionにしてる
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag.Contains("Player"))
        {
            // other.gameObject.GetComponent<PlayerController>().GetDamage(damage);
        }
        
        if (Object is not null) Runner.Despawn(Object);
    }
    
    private void OnCollisionStay(Collision other)
    {
        Runner.Despawn(Object);
    }

    
}
