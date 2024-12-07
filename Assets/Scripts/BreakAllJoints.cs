using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BreakAllJoints : MonoBehaviour
{
    List<Joint> jointList = new List<Joint>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        jointList = GetComponentsInChildren<Joint>().ToList();
    }

    // Update is called once per frame
    void Update()
    {
        //LD Montello
        //set the joints to be allowed to break.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < jointList.Count; i++) 
            {
                jointList[i].breakForce = 1;
            }
        }
    }
}
