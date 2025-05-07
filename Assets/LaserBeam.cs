using UnityEngine;

public class LaserBeam : MonoBehaviour
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DealDamage(Collider other)
    {
        //ignore the gashamachines if we hit them.
        if (other.CompareTag("Gashamachine"))
        {
            return;
        }

        IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            //make the damageable take damage.
            //and tell it we gave it damage.
            damageable.TakeDamage(1, gameObject);

            Debug.Log(other.gameObject.name + " damaged by LaserBeam".Color("Cyan"));
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        DealDamage(other);   
    }
}
