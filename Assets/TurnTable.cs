using UnityEngine;

public class TurnTable : MonoBehaviour
{
    public float rotationSpeed = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.up) * transform.rotation;
    }
}
