using System.Collections;
using UnityEngine;

public class JumpAttack : BossAction
{
    public float height;
    public bool stopAtApex;

    public override IEnumerator ActionCoroutine(BossController boss, float duration)
    {
        // Handle the jump physics using height and stopAtApex.
        Debug.Log("JUMP ATTACK".Color("Yellow"));

        yield return new WaitForSeconds(duration);
        //Debug.LogWarning("JUMP PERFORMED".Color("Blue"));

        //always set to idle after an attack.
        boss.curState = BossController.BossState.idle;
    }
}