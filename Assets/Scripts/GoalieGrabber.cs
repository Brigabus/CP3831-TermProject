using UnityEngine;


//This extends ball grabber to allow it to have mostly the same functions, just with some of the tackle stuff removed
public class GoalieGrabber : BallGrabber
{

    public void performAction()
    {
        if(ballHandler != null && ballHandler.currentOwner == this)
        {
            KickBall();
            
        }

    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            ballCollider = other;
            ballObject = other.gameObject;
            ballHandler = ballObject.GetComponent<BallHandler>();
        
            if (ballHandler.currentOwner == null && !kickingBall)
            {
                ballrb = other.attachedRigidbody;
                GrabBall(ballObject, ballHandler);
            }
            return;
        }
        
    }
}
