using Unity.Engine;

public class Capsule: MonoBehavior, IDamageable{
    private GameObject objectHeld;

    public void setObjectHeld(GameObject object){
        objectHeld = object;
    }
    public void TakeDamage(int damage, GameObject other){
        // TODO: Play Capsule Pop Animation/Sound
        Instantiate(objectHeld);
    }
}