using UnityEngine;

[ExecuteInEditMode]
public class BrighterGizmos : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Gizmos.color *= 2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
