using System.Collections;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public ParticleSystem death_particles;

    public float inflateAnimationTime = 1;

    public float inflateAnimationEndScaleMultiplier = 2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //destroy in a random amount of time between
        //1 and 5 seconds.
        StartCoroutine(DestroyCoroutine(1.5f + Random.value * 2));
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public float smoothstep(float time)
    {
        //Smoothstep is 3v^2 - 2v^3
        //gives us a nice ease-in, ease-out when lerping with this time value.
        return 3 * time * time - 2 * time * time * time;
    }

    float QuadStep(float t)
    {
            return 2.0f * t * t;
    }

    public IEnumerator DestroyCoroutine(float totalTime)
    {

        float cur_time = 0;
        //wait until 
        //there's only enough
        //time left for the inflate animation.
        while (totalTime - inflateAnimationTime > cur_time)
        {
            cur_time += Time.deltaTime;
            yield return null;
        }

        //Start the inflate animation
        //This whole inner coroutine
        //will execute before
        //we move to the destroy call.
        yield return inflateCoroutine(inflateAnimationTime, 1, inflateAnimationEndScaleMultiplier);

        //Disable the collider
        //so it doesn't interfere with
        //the particle collisions.
        GetComponent<Collider>().enabled = false;

        //Death particle's 
        //stop action is to destroy itself,
        //so when it finishes the system,
        //it'll destroy itself (free garbage collection).
        Instantiate(death_particles, transform.position, Quaternion.identity);

        //Actually destroy the object.
        Destroy(gameObject);
    }

    //inflate because we use smoothstep as part
    //of our lerp to make it look like it bursts.
    public IEnumerator inflateCoroutine(float total, float startScaleMultiplier, float endScaleMultiplier)
    {
        Vector3 startScale = transform.localScale;

        float cur_time = 0;

        //Subtract 0.5 from total
        //so it looks like we just hit the limit
        //of stretching the sphere and now it explodes.
        //this looks much better than before.
        while (total - 0.5 > cur_time)
        {
            cur_time += Time.deltaTime;

            float percent = cur_time / total;
            transform.localScale = Vector3.Lerp(startScale * startScaleMultiplier, startScale * endScaleMultiplier, QuadStep(percent));

            yield return null;
        }

        //finsh off by setting our scale to the end value.
        transform.localScale = startScale * endScaleMultiplier;



        //Coroutine will now exit.
        yield break;
    }
}
