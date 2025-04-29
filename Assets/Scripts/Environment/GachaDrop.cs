using UnityEngine;
using UnityEngine.SceneManagement;

public class GachaDrop : Collectible
{
    public string gachaName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var scene = SceneManager.GetActiveScene();
        //If any MonoBehavior in this scene has a method
        //called "UnlockObject"
        //it will be called here.
        foreach (var g in scene.GetRootGameObjects())
        {
            g.BroadcastMessage("UnlockObject", gachaName, SendMessageOptions.DontRequireReceiver);
        }

        //call on collect so this is collected.
        OnCollect();

        //After unlocking the gacha machine,
        //destroy ourselves.
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnCollect()
    {
        CollectionManager.instance.AddToCollection(this);
    }
}
