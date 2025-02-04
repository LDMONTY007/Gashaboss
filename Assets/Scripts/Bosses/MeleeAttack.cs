using UnityEngine;

public class MeleeAttack : BossAction
{
    public override void Execute(BossController boss, float duration)
    {
        //if the boss's weapon is null,
        //then we need to throw an error
        //because it should be null.
        if (boss.weapon != null)
        {
            boss.weapon.Attack();
        }
        else
        {
            Debug.LogError("The boss has no weapon equipped, please ensure there is one equipped.");
        }

        DashAwayMove dashAwayMove = new DashAwayMove();
        dashAwayMove.Execute(boss, 1f);
    }
}
