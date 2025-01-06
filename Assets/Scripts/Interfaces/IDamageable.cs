using UnityEngine;

//Interface used for any object when we want an object
//to recieve damage.
public interface IDamageable
{
    public void TakeDamage(int damage, GameObject other);
}
