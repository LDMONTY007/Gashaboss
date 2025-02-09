using System.Collections;
using UnityEngine;

public class DashAwayMove : BossAction
{
    public override IEnumerator ActionCoroutine(BossController boss, float duration)
    {
        //the distance the player is going to dash at.
        //this is here just for readability.
        float dashDist = 10f;

        Vector3 dirFromPlayer = boss.playerObject.transform.position - boss.transform.position;
        Vector3 dashAwayPos = -1f * dirFromPlayer.normalized * dashDist;
        boss.curState = BossController.BossState.move;

        //just yield and return the move to position
        //call.
        yield return boss.MoveToPosition(dashAwayPos, 50f, 1f, 0.5f);
    }
}
