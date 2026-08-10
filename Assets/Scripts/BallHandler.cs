using System.Collections;
using UnityEngine;

//This small script attaches to the ball and allows it to keep track of who currently owns it and if it was just kicked
public class BallHandler : MonoBehaviour
{
    public BallGrabber currentOwner = null;
    

    
    public bool beenKicked;
    
    public IEnumerator KickCooldown()
    {
        beenKicked = true;
        yield return new WaitForSeconds(0.2f);
        beenKicked = false;

    }
    
}
