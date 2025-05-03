using UnityEngine;

public class BossProjectile : Projectile{
    new public void OnCollisionEnter(Collision collision){
        if (doesSplashDamage)DealSplashDamage();
        else{
            //do damage to the object we collided with (or attempt to), but only if it's not a boss
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            BossController boss = collision.gameObject.GetComponent<BossController>();
            if (damageable != null && boss == null){
                DealDamage(damageable);
            }
        }
        if (destroyOnCollision){
            Destroy(gameObject);
        }
    }
}