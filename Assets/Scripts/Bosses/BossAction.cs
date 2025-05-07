using System.Collections;
using UnityEngine;
public abstract class BossAction
{
    public bool didExecute = false;

    public bool active = false;

    public bool dealMeleeDamage = false;

    public float meleeDamage = 1;

    //public abstract void Execute(BossController boss, float duration);

    public abstract IEnumerator ActionCoroutine(BossController boss, float duration);
}
