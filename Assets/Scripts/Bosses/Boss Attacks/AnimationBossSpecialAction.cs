using UnityEngine;
public class AnimationSpecialBossAction: MonoBehaviour{
    [SerializeField] private BossWeapon weapon;

    public void AnimatorTrigger(){
        StartCoroutine(weapon.AnimatorSpecialAction());
    }
}