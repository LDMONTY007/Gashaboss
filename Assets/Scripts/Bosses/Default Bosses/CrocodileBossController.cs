using UnityEngine;

//LD Montello
public class CrocodileBossController : BossController
{
    public override void HandleStateMachine()
    {
        

        #region handling individual states

        switch (curState)
        {
            case BossState.idle:
                //bossRenderer.material.color = Color.white;
                HandleIdle();
                break;
            case BossState.attack:

                //set the attack trigger animation
                //so we do the attack animation.
                animator.SetTrigger("attack");
                //bossRenderer.material.color = Color.red;
                HandleAttack();
                break;
            case BossState.move:
                //bossRenderer.material.color = Color.blue;
                HandleMove();
                break;
            case BossState.stun:
                //bossRenderer.material.color = Color.yellow;
                break;
        }

        #endregion
    }

    public override void HandleAnimation()
    {
        //if our velocity is greater than zero,
        //then do our moving animation,
        //otherwise don't do our moving animation.
        if (rb.linearVelocity.magnitude > 0)
        {
            animator.SetBool("moving", true);
        }
        else
        {
            animator.SetBool("moving", false);
        }
    }
}
