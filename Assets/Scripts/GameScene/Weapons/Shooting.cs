using UnityEngine;

public class Shooting : MonoBehaviour {

    public GameObject bulletPrefab;
    public float shotSpeed;
    public int shotCount = 30;
    public float shotInterval = 1f;
    private float _interval;

    void Update()
    {
        _interval += Time.deltaTime;
        if (Input.GetKey(KeyCode.Mouse0)) // 左クリックされたら
        {
            if (shotInterval < _interval && shotCount > 0) // 間隔が空いていてかつ残弾がある
            {
                shotCount--; //残弾減らす
                _interval = 0; // インターバル最初から
                GameObject bullet = Instantiate(bulletPrefab, new Vector3(transform.position.x, transform.position.y + 2, transform.position.z), Quaternion.identity); //弾召喚
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>(); 
                bulletRb.AddForce(transform.forward * shotSpeed); // 飛ばす

                //射撃されてから3秒後に銃弾のオブジェクトを破壊する.

                // Destroy(bullet, 3.0f);
            }

        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            shotCount = 30;
           
        }

    }
}