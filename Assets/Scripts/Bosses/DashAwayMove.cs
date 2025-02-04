using UnityEngine;

public class DashAwayMove : BossAction
{
    public override void Execute(BossController boss, float duration)
    {
        Vector3 dirFromPlayer = boss.playerObject.transform.position - boss.transform.position;
        Vector3 doubledDashAwayPosition = -1f * dirFromPlayer.normalized * dirFromPlayer.magnitude * 2f;
        boss.curState = BossController.BossState.move;
        
        
        boss.StartCoroutine(boss.MoveToPosition(doubledDashAwayPosition));

    }
}
