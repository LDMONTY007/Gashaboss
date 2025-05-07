using System.Collections;
using UnityEngine;
public abstract class BossAction
{
    public bool didExecute = false;

    public bool active = false;

    //public abstract void Execute(BossController boss, float duration);

    public abstract IEnumerator ActionCoroutine(BossController boss, float duration);
}
