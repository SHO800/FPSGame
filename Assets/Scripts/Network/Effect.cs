using Fusion;

public class Effect : NetworkBehaviour
{
    public float lifeTime = 1.0f;
    [Networked] private TickTimer Life { get; set; }
    
    public override void Spawned()
    {
        Life = TickTimer.CreateFromSeconds(Runner, lifeTime);
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && Life.Expired(Runner) && Object is not null) Runner.Despawn(Object);
    }
}
