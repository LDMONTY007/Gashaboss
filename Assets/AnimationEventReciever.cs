using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventReciever : MonoBehaviour
{
    //The event to be called by an animation event.
    public UnityEvent animationEvent = new UnityEvent();

    public void InvokeEvent()
    {
        animationEvent.Invoke();
    }
}
