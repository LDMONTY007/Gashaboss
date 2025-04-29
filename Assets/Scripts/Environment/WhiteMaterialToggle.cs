using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class WhiteMaterialToggle : MonoBehaviour
{
    public Material newMaterial;

    private bool _isWhite = true;

    public bool isWhite {  get { return _isWhite; } set 
        {  
            if (value == true)
            {
                SetMaterialsWhite();
            }
            else
            {
                SetMaterialsDefault();
            }

            _isWhite = value; 
        } 
    }

    List<Material> defaultMaterials = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Get all the materials in the children and store them in an array.
        GetAllMaterials();

        if (isWhite == true)
        {
            SetMaterialsWhite();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isWhite = !isWhite;
        }
    }

    Renderer[] childrenRenderers;

    public Material[] childMaterials;

    

    public void GetAllMaterials()
    {
        //get the child renderers
        childrenRenderers = GetComponentsInChildren<Renderer>();


        int total = 0;

        //get the total number of materials in the children of this object.
        foreach (Renderer renderer in childrenRenderers)
        {
            total += renderer.materials.Length;
        }

        //initialize childMaterials to the total number of materials in the children of this object.
        childMaterials = new Material[total];

        //this is the current index of the material in the childMaterials array.
        int i = 0;

        //loop through all the child renderers.
        foreach (Renderer childRenderer in childrenRenderers)
        {
            //if we have multiple materials
            if (childRenderer.materials != null && childRenderer.materials.Length > 0)
            {
                //loop through all materials in the renderer
                foreach (Material m in childRenderer.materials)
                {
                    //assign the material to this position so it can be retrieved later.
                    childMaterials[i] = m;

                    //increment the material index.
                    i++;
                }
            }
            else if (childRenderer.material != null)
            {
                childMaterials[i] = childRenderer.material;

                //increment the material index.
                i++;
            }
            else
            {
                Debug.LogWarning($"Child Renderer {childRenderer.gameObject.name} has no material assigned.");
            }
        }
    }

    public void SetMaterialsDefault()
    {
        Debug.Log("Setting Materials to Default");

        //get the child renderers
        childrenRenderers = GetComponentsInChildren<Renderer>();

        //this is the current index of the material in the childMaterials array.
        int i = 0;

        foreach (Renderer childRenderer in childrenRenderers)
        {
            //if we have multiple materials
            if (childRenderer.materials != null && childRenderer.materials.Length > 0)
            {
                Material[] newMats = new Material[childRenderer.materials.Length];
                for (int j = 0; j < childRenderer.materials.Length; j++)
                {
                    //assign the material in our newMats array.
                    newMats[j] = childMaterials[i];

                    //increment the material index.
                    i++;
                }
                //assign our newMats array.
                childRenderer.materials = newMats;

                //Make sure if it has a random seed material
                //that the random seed gets reassigned.
                CheckHasRandomSeed(childRenderer.gameObject);
            }
            else if (childRenderer.material != null)
            {
                //make sure to set the sharedMaterial.
                childRenderer.sharedMaterial = childMaterials[i];

                //increment the material index.
                i++;

                //Make sure if it has a random seed material
                //that the random seed gets reassigned.
                CheckHasRandomSeed(childRenderer.gameObject);
            }
            else
            {
                Debug.LogWarning($"Child Renderer {childRenderer.gameObject.name} has no material assigned.");
            }
        }
    }

    public void CheckHasRandomSeed(GameObject g)
    {
        //if a SetRandomSeed script exists we want to make sure
        //it assigns the seed to it's material.
        SetRandomSeed seed = g.GetComponent<SetRandomSeed>();
        if (seed != null)
        {
            seed.AssignSeed();
        }
    }

    public void SetMaterialsWhite()
    {
        if (newMaterial == null)
        {
            Debug.LogWarning("New material not set.  Please assign a material in the inspector.");
            return;
        }

        foreach (Renderer childRenderer in childrenRenderers)
        {
          
            //For objects with multiple materials
            if (childRenderer.materials != null && childRenderer.materials.Length > 0)
            {
                Material[] newMats = new Material[childRenderer.materials.Length];
                for (int i = 0; i < childRenderer.materials.Length; i++)
                {
                    newMats[i] = newMaterial;
                }
                childRenderer.materials = newMats;
            }
            // Check if the child renderer has a singular material
            else if (childRenderer.material != null)
            {
                childRenderer.sharedMaterial = newMaterial;
            }
            else
            {
                Debug.LogWarning($"Child Renderer {childRenderer.gameObject.name} has no material assigned.");
            }
        }
    }

}
