using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{

    Color startCol;
    MeshRenderer meshRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        startCol = meshRenderer.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            //make the damageable take damage.
            //and tell it we gave it damage.
            damageable.TakeDamage(1, gameObject);

            //if we aren't currently changing our color,
            //then change our color to match that we did damage.
            if (meshRenderer.material.color == startCol)
            {
                //start color change coroutine.
                StartCoroutine(LDUtil.SetColorForDuration(meshRenderer, Color.red, 0.25f));
            }
            
        }

        
    }
}
