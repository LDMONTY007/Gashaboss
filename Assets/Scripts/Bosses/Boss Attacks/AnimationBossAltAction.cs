using UnityEngine;
public class AnimationAltBossAction: MonoBehaviour{
    [SerializeField] private BossWeapon weapon;

    public void AnimatorTrigger(){
        Debug.Log("Got trigger");
        StartCoroutine(weapon.AnimatorAltAction());
    }
}