using UnityEngine;

public class test : MonoBehaviour
{
    private Material _material;
    // Start is called before the first frame update
    void Start()
    {
        _material = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        _material.color = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
    }
}
