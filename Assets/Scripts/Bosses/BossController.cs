using System.Linq;
using UnityEngine;

public class BossController : MonoBehaviour
{

    bool doStateMachine = false;

    [Header("Attack Parameters")]
    public float attackCheckRadius = 10f;

    public enum BossState
    {
        idle,
        attack,
        move,
    }

    public BossState bossState;

    bool IsPlayerInAttackRange()
    {
        Collider[] objs = Physics.OverlapSphere(transform.position, attackCheckRadius);

        //if the player is in the attack range
        //return true. 
        if (objs.Any<Collider>(c => c.GetComponent<Player>() != null))
        {
            return true;
        }

        return false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (doStateMachine)
        {
            HandleStateMachine();
        }
    }

    public void HandleStateMachine()
    {
        #region boss state switching

        if (bossState != BossState.attack)
        { 
            if (IsPlayerInAttackRange())
            {
                bossState = BossState.attack;
            }
        }

        #endregion

        #region handling individual states

        switch (bossState)
        {
            case BossState.idle:
                break;
            case BossState.attack:
                HandleAttack();
                break;
            case BossState.move:
                break;
        }

        #endregion
    }

    public void HandleAttack()
    {

    }

   

    private void OnDrawGizmos()
    {
        //Store gizmos color.
        Color prevColor = Gizmos.color;

        //Draw a red sphere to show within
        //the radius the player must be for
        //our boss to initiate an attack.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackCheckRadius);

        //Set gizmos color back to the original color.
        Gizmos.color = prevColor;
    }
}
