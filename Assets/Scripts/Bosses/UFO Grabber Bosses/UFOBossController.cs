using UnityEngine;

public class UFOBossController : BossController
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
}
