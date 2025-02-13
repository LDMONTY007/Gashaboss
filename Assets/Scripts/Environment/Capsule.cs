using Unity.Engine;

//TODO: Add Capsule animation for spawning from machine
// want it to drop out of machine with a small bounce, need machine and capsule models to finish
public class Capsule: MonoBehavior, IDamageable{
    private GameObject objectHeld;

    public void SetObjectHeld(GameObject object){
        objectHeld = object;
    }
    public void TakeDamage(int damage, GameObject other){
        // TODO: Play Capsule Pop Animation/Sound
        Instantiate(objectHeld);
    }
}