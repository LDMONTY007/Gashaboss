using System.ComponentModel;
using UnityEngine;

public class EnvironmentUnlockable : MonoBehaviour
{
    bool _unlocked = false;

    public bool unlocked 
    { 
        get 
        { 
            return _unlocked; 
        } 
        set 
        {     
            //call code for being locked/unlocked.
            if (value == true) 
            {
                OnUnlock();   
            }
            else
            {
                OnLock();
            }

            _unlocked = value; 
        } 
    }

    //the script we enable/disable when this object is unlocked/locked.
    public MonoBehaviour behaviourToLock;

    //the script that toggles an object and it's hierarchy to 
    //switch between white material and it's default colors.
    WhiteMaterialToggle materialToggle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        materialToggle = GetComponent<WhiteMaterialToggle>();

        //call code on start for locking/unlocking.
        if (unlocked == false)
        {
            OnLock();
        }
        else
        {
            OnUnlock();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            unlocked = !unlocked;
        }
    }

    void OnUnlock()
    {
        //we don't want to set this more than once in a row to the same value.
        if (materialToggle.isWhite != false)
            //set back to default colors.
            materialToggle.isWhite = false;

        //enable the script that controls this environment object.
        behaviourToLock.enabled = true;
    }

    void OnLock()
    {
        //we don't want to set this more than once in a row to the same value.
        if (materialToggle.isWhite != true)
            //set to be all white.
            materialToggle.isWhite = true;

        //disable the script that controls this environment object.
        behaviourToLock.enabled = false;
    }
}
