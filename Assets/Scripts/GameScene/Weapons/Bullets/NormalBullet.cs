using UnityEngine;

public class NormalBullet : MonoBehaviour
{
    public float damage;

    private void Update()
    {
        if (transform.position.y < -100)
        {
            Destroy(gameObject);
        }
    }

    // Triggerだと貫通することがあるのでCollisionにしてる
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag.Contains("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().GetDamage(damage);
        }
        Destroy(gameObject);
    }
    
    private void OnCollisionStay(Collision other)
    {
        Destroy(gameObject);
    }

    
}
