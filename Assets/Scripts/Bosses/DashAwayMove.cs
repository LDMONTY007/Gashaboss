using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashAwayMove : BossAction
{
    public override IEnumerator ActionCoroutine(BossController boss, float duration)
    {
        //the distance the player is going to dash at.
        //this is here just for readability.
        float dashDist = 50f;

        Vector3 dirFromPlayer = boss.playerObject.transform.position - boss.transform.position;
        Vector3 dashAwayPos = -1f * dirFromPlayer.normalized * dashDist;
        dashAwayPos = boss.transform.position + dashAwayPos;
        Vector3 ogDashAwayPos = dashAwayPos;

        //get the pathable point on the navmesh for the boss.
        dashAwayPos = boss.GetPathablePoint(dashAwayPos, 100f);

        //boss.curState = BossController.BossState.move;

       /* #region collision avoidance
        //we are going to search and find
        //the closest unobstructed position
        //to dash towards.
        //we'll find the closest position
        //to the dash distance away from the player
        //direction. 
        //basically we scan an arc and find the closest position
        //in the arc that is unobstructed to our original desired target position which is ogDashAwayPos

        float searchAngle = 360f;

        //we want to do the entire vertical size of the boss collider
        for (int i = 0; i < boss.bossCollider.bounds.size.y; i++)
        {
            Vector3 rayOrigin = boss.transform.position + new Vector3(0, i - boss.bossCollider.bounds.size.y / 2, 0);

            //number of rays to shoot out in our cone like shape
            int rayCount = 10;

            for (int j = 0; j < rayCount; j++)
            {
                //we want the left most
                //angle to start
                //on the left side
                //of the up vector
                //and end on the ride side,
                //so we need to do (180 - avoidanceAngle) / 2
                //to get our offset in the 2 quadrant range
                //where we want to shoot rays.
                //then we add that offset
                //to our character's current rotation
                //angle to get a start angle.
                //then we use our start angle
                //plus our current angle along
                //the FOV to generate the desired
                //direction vector for the ray.
                float startAngle = boss.transform.rotation.eulerAngles.y + searchAngle / 2f;
                float angle = (boss.avoidanceAngle) * j / rayCount;
                Vector2 dir2 = LDUtil.AngleToDir2D(-1 * (startAngle + angle)).normalized;
                Vector3 dir3 = new Vector3(dir2.x, 0f, dir2.y);
                //Debug.DrawRay(rayOrigin, dir3 * avoidanceDistance * 2, Color.green, 1f);

                //ContactFilter2D contactFilter = new ContactFilter2D();

                //List<RaycastHit2D> results = new List<RaycastHit2D>();

                //Raycast while ignoring
                //Player and Ignore Raycast layers.
                //bossCollider.Raycast(dir3, contactFilter, results, distance * 2);
                RaycastHit[] hits = Physics.RaycastAll(rayOrigin, dir3, dashDist, ~LayerMask.GetMask("Ignore Raycast"));

                //if we didn't hit anything, 
                //check that there's enough room for us to go there.
                if (hits.Length < 1)
                {
                    //if we hit something, we need to move onto the next angle.
                    //ignore the ground layer.
                    if (Physics.OverlapSphere(rayOrigin + dir3 * dashDist, 5, ~LayerMask.GetMask("Ground")).Length > 0)
                    {
                        Debug.Log("MOVING TO NEXT ANGLE");
                        continue;
                    }

                    //if the new dash away position is closer
                    //than the current dash away position
                    //to our original desired position,
                    //update to the closest unobstructed desired position.
                    if (Vector3.Distance(ogDashAwayPos, dashAwayPos) > Vector3.Distance(ogDashAwayPos, rayOrigin + dir3 * dashDist))
                    {
                        dashAwayPos = rayOrigin + dir3 * dashDist;
                        dashAwayPos.y = boss.transform.position.y;
                        Debug.DrawRay(rayOrigin, dir3 * dashDist, Color.green, 1f);
                        //break from the current loop to continue the search.
                        break;
                    }
                }
            }
        }


        #endregion*/

        //make sure that we don't
        //target a y position that we can't
        //reach, so just stay at the same y position.
        dashAwayPos.y = boss.transform.position.y;

        //just yield and return the move to position
        //call.
        yield return boss.MoveToPosition(dashAwayPos, 50f, 1f, 0.5f);
    }
}
