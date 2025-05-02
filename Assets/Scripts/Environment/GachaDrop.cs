using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GachaDrop : Collectible
{
    public string gachaName;

    private TurnTable turnTable;

    public GameObject gachaModel;

    public bool unlockOnStart = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //only true when this is already in the collection and loaded
        //on scene start.
        if (unlockOnStart)
        {
            UnlockEnvironmentalObject();
            //destroy after unlocking on start.
            Destroy(gameObject);
            return;
        }

        //Add a white point light to the object so that it emits light and never is in shadow.
        Light l = gameObject.AddComponent<Light>();
        l.intensity = 30f;
        l.range = 5;

        //make the gacha model rotate around.
        if (gachaModel != null)
        {
            turnTable = gachaModel.AddComponent<TurnTable>();
            turnTable.rotationSpeed = 30f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnCollect()
    {
        StartCoroutine(PickupAnimationCoroutine());
    }

    public IEnumerator PickupAnimationCoroutine()
    {
        //animation time is 1f.
        float totalAnimTime = 1f;
        float curAnimTime = 0f;

        float startSpeed = turnTable.rotationSpeed;
        //the rotation speed of the turntable at the end of the animation.
        float endSpeed = startSpeed + 2000f;

        float startHeight = gachaModel.transform.position.y;
        float endHeight = startHeight + 10;

        //loop until we complete the animation.
        while (totalAnimTime > curAnimTime)
        {
            curAnimTime += Time.deltaTime;

            //set speed based on how far in the animation we are.
            turnTable.rotationSpeed = Mathf.Lerp(startSpeed, endSpeed, curAnimTime / totalAnimTime);

            //lerp from start to end height to make it look like the item "Jumps" into the air.
            gachaModel.transform.position = new Vector3(gachaModel.transform.position.x, Mathf.Lerp(startHeight, endHeight, Mathf.SmoothStep(0, 1, curAnimTime / totalAnimTime)), gachaModel.transform.position.z);

            yield return null;
        }


        //call the on animation end function.
        OnCollectAnimationEnd();
    }

    public void UnlockEnvironmentalObject()
    {
        var scene = SceneManager.GetActiveScene();
        //If any MonoBehavior in this scene has a method
        //called "UnlockObject"
        //it will be called here.
        foreach (var g in scene.GetRootGameObjects())
        {
            g.BroadcastMessage("UnlockObject", gachaName, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void OnCollectAnimationEnd()
    {
        //unlock the object in the environment.
        UnlockEnvironmentalObject();

        
        
        CollectionManager.instance.AddToCollection(this);

        //if the popup manager exists
        if (UIManager.Instance.popupUIManager != null)
        {
            //display a popup that shows that this gashapon has been unlocked.
            UIManager.Instance.popupUIManager.DoPopup(collectibleName + " Unlocked!", Color.black, false);
        }

        //After unlocking the gacha machine,
        //destroy ourselves.
        Destroy(gameObject);


        //TODO: Code a UI popup script so you can make new stuff popup in the UI this way.
        Debug.Log(collectibleName + " Unlocked!");
    }
}
