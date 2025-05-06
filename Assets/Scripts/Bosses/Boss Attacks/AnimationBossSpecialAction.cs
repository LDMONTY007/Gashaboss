using UnityEngine;
public class AnimationSpecialBossAction: MonoBehaviour{
    [SerializeField] private BossWeapon weapon;

    public void SpecialAnimatorTrigger(){
        StartCoroutine(weapon.AnimatorSpecialAction());
    }
}