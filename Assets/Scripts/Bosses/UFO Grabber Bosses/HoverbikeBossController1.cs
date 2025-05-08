using UnityEngine;

public class HoverbikeBossController : BossController
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

    public override void ApplyFinalMovements()
    {
        //when we aren't doing some kind of move, 
        //the boss can't fall unless we check here and allow them to fall.
        //we freeze the position otherwise. 
        //we just allow gravity to take over
        //so that it can fall back to the ground.
        if (curState != BossState.stun && curState != BossState.move && !isGrounded)
        {
            //freeze all rotation and position.
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        //Never apply gravity or y velocity to this boss because it should never move on the y axis.
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        //make sure the position of the rigidbody on the y is always zero.
        //again, we never want this boss to move on the y axis.
        rb.position = new Vector3(rb.position.x, 0f, rb.position.z);
    }
}
