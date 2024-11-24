using UnityEngine;

public class SwitchToLowRes : MonoBehaviour
{
    bool isLowRes = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SwitchResolution();
        }
    }

    public void SwitchResolution()
    {
        isLowRes = !isLowRes;
        if (isLowRes )
        {
            gameObject.layer = LayerMask.NameToLayer("LowRes");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
        
    }
}
