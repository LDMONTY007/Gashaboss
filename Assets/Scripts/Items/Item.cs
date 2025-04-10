using System.Collections;
using UnityEngine;
// Used to attach to the game object prefabs for items
// Passes logic to the ScriptableObject for that item
// Primarily handles adding to inventory and gameobject deletion
public class Item: Collectible
{
    [SerializeField] private ItemData item; // Assign scriptable object via inspector

    [SerializeField] private GameObject itemModel; // Assign item model via inspector.

    //turn table that is used to rotate the object
    //and then used in the animation when the item is picked up.
    private TurnTable turnTable;

    public override void OnCollect(){
        if(item == null) {
            Debug.LogError("Item Instance Has No Object Assigned: Please Add One.\n Aborting Item Pickup.");
            Destroy(gameObject);
            return;
        }
        CollectionManager.instance.AddToCollection(this);
        //Start the animation for the item being picked up.
        StartCoroutine(PickupAnimationCoroutine());
    }

    public void OnCollectAnimationEnd()
    {
        // Only call pickup if the player doesn't already have item in inventory(unique items?)
        if (Player.instance.AddItemToInventory(item)) item.OnPickup();
        Destroy(gameObject); // Destroy only the physical object, not the script
        Debug.Log("Deleting Item Prefab, Successfully added to inventory.");
    }
    

    private void Start()
    {
        //make the item model rotate around.
        if (itemModel != null)
        {
            turnTable = itemModel.AddComponent<TurnTable>();
            turnTable.rotationSpeed = 30f;
        }

        //Add a white point light to the object so that it emits light and never is in shadow.
        Light l = gameObject.AddComponent<Light>();
        l.intensity = 30f;
        l.range = 5;
    }

    public IEnumerator PickupAnimationCoroutine()
    {
        //animation time is 1f.
        float totalAnimTime = 1f;
        float curAnimTime = 0f;

        float startSpeed = turnTable.rotationSpeed;
        //the rotation speed of the turntable at the end of the animation.
        float endSpeed = startSpeed + 2000f;

        float startHeight = itemModel.transform.position.y;
        float endHeight = startHeight + 10;

        //loop until we complete the animation.
        while (totalAnimTime > curAnimTime)
        {
            curAnimTime += Time.deltaTime;

            //set speed based on how far in the animation we are.
            turnTable.rotationSpeed = Mathf.Lerp(startSpeed, endSpeed, curAnimTime / totalAnimTime);

            //lerp from start to end height to make it look like the item "Jumps" into the air.
            itemModel.transform.position = new Vector3(itemModel.transform.position.x, Mathf.Lerp(startHeight, endHeight, Mathf.SmoothStep(0, 1, curAnimTime / totalAnimTime)), itemModel.transform.position.z);

            yield return null;
        }


        //call the on animation end function.
        OnCollectAnimationEnd();
    }
}