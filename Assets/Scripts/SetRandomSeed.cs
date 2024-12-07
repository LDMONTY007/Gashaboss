using UnityEngine;

public class SetRandomSeed : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //LD Montello
        //Generate a random seed to randomize color when this object
        //is instantiated.
        float seed = Random.value;
        GetComponent<MeshRenderer>().materials[0].SetFloat("_Seed", seed);
    }
}
