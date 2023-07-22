using UnityEngine;

public class Shooting : MonoBehaviour {

    public GameObject bulletPrefab;
    public float shotSpeed;
    public int shotCount = 30;
    public float shotInterval = 1f; // 変更
    private float _interval; // 変更

    void Update()
    {
        _interval += Time.deltaTime; //変更
        if (Input.GetKey(KeyCode.Mouse0))
        {
            // Debug.Log(_interval);
            if (shotInterval < _interval && shotCount > 0) //変更
            {
                shotCount--;
                _interval = 0; // 変更
                GameObject bullet = Instantiate(bulletPrefab, new Vector3(transform.position.x, transform.position.y + 2, transform.position.z), Quaternion.identity);
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                bulletRb.AddForce(transform.forward * shotSpeed);

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