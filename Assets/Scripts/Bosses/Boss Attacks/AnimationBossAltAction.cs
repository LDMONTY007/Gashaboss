using UnityEngine;
public class AnimationAltBossAction: MonoBehaviour{
    [SerializeField] private BossWeapon weapon;

    public void AnimatorTrigger(){
        weapon.AnimatorAltAction();
    }
}