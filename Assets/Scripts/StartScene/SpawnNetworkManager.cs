using UnityEngine;

public class SpawnNetworkManager : MonoBehaviour
{
    public GameObject networkManagerPrefab;

    private void Start()
    {
        Instantiate(networkManagerPrefab, Vector3.zero, Quaternion.identity);
    }
}
