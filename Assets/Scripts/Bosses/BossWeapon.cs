using UnityEngine;

public class BossWeapon: Weapon{
    public override void AltAttack(){
        if (!canAttack)
        {
            //Debug.LogWarning("Cannot attack during cooldown");
            return;
        }
        StartCoroutine(AltAttackCoroutine());
    }
    public override IEnumerator AltAttackCoroutine(){
        
    }
}
