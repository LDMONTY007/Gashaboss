using UnityEngine;

public class SetRandomSeed : MonoBehaviour
{
    public bool setSeed = true;
    public float seed = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (setSeed)
        {
            GenRandomSeed();
        }

        GetComponent<MeshRenderer>().materials[0].SetFloat("_Seed", seed);
    }

    public void GenRandomSeed()
    {
        //LD Montello
        //Generate a random seed to randomize color when this object
        //is instantiated.
        seed = Random.value;
        GetComponent<MeshRenderer>().materials[0].SetFloat("_Seed", seed);
    }
}
