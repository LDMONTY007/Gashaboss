using UnityEngine;

public class StopWatch: Item {
    void Start(){
        this.name = "Stopwatch";
        this.timeTillReapply = Player.iFrameTime;
    }
    public void abstract ApplyEffect(){
        /* 
        The logic, If I only check iFrameTime for the window of invunerability
        Then I should be able to catch the iFrames when the player is hit
        It should also never proc twice from the same hit
        1.5f and .13f were arbirtrary choices
        should work fine so long as the player only goes into iframes on hit
        does not actually track hits though, maybe there is a better way to do?
        */ 
        if (Player.iFrameTime < 1.5f && Player.invincible){
            this.timeTillReapply = Player.iFrameTime += .13f;
        }
    }
}