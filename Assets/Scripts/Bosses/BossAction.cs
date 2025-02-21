using System.Collections;
using UnityEngine;
public abstract class BossAction
{
    //public abstract void Execute(BossController boss, float duration);

    public abstract IEnumerator ActionCoroutine(BossController boss, float duration);
}
