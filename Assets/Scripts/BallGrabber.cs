using System.Collections;
using UnityEngine;

//This class controls all of the player's actions that don't have to do expressly with input / movement
//It was original just to control grabbing the ball, but it kind of ballooned.
//Don't think too hard about the name, refactoring classes in C# is a pain
public class BallGrabber : MonoBehaviour
{
    //The development of this class was very iterative, so the variables are a bit all over the place
    public Vector3 ballOffset;
    
    //Other player classes and components
    public Rigidbody playerRB;
    public Collider playerCol;
    public PlayerMovement playerMov;
    
    public float kickStrength;
    public bool kickingBall = false;
    
    //Components related to the ball
    public GameObject ballObject;
    public BallHandler ballHandler;
    public Rigidbody ballrb;
    public Collider ballCollider;
    private bool hasBall;
    private Vector3 ballBasePosition;
    
    //Tackle related stuff
    public float tackleStrength = 30f;
    public BallGrabber tackleTarget;
    
    //Variables related to dribbling when the player has the ball
    public float dribbleAmount = 0.15f;
    public float dribbleSpeed = 8f;
    public float rollSpeed = 300f;
    public float rollAmount;
    
    //A particle object for instantiating particles when a player gets hit
    //Without these, the hits felt like they lacked impact
    public GameObject hitParticles;
    
    //The animator lets us control what animation cues are being played through the script
    public Animator animator;
    
    public PlayerAudio playerAudio;
    
    
    void Start()
    {
        //Get some stuff from the current player
        playerRB = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        playerMov = GetComponent<PlayerMovement>();
        playerAudio = GetComponent<PlayerAudio>();
    }
    
    /*
    The entire LateUpdate function here is dedicated to dribbling the ball when we have it.
    Because the ball is set to kinematic when we pick it up, physics doesn't interact with it,
    this caused the ball to just kind of float in front of the player when they picked it up.
    Decided the fake it till you make it approach was better for this, so we just fake the ball's
    rolling and dribbling with math that scales on the player's speed.
    */
    void LateUpdate()
    {
        if(ballObject == null || ballrb == null) return;
        
        float playerSpeed = new Vector3(playerRB.linearVelocity.x, 0f, playerRB.linearVelocity.z).magnitude;
        
        if(playerSpeed > 0.05f)
        {
            float frequency = dribbleSpeed + playerSpeed * 2f;
            
            //Sine function, my beloved
            float movement = Mathf.Sin(Time.time * frequency) * dribbleAmount;
            
            ballObject.transform.localPosition = ballBasePosition + Vector3.forward * movement;
            
            rollAmount += playerSpeed * rollSpeed * Time.deltaTime;
            
            ballObject.transform.localRotation = Quaternion.Euler(rollAmount, 0f, 0f);
        } else
        {
            //Don't roll the ball if the player is'nt moving
            ballObject.transform.localPosition = ballBasePosition;
        }
        
        
    }
    
    //The actual ball grabber logic started simple, but refused to remain that way and is now very messy
    void OnTriggerEnter(Collider other)
    {
        //if the object that entered our trigger is a ball...
        if (other.CompareTag("Ball"))
        {
            //Store it for reference...
            ballCollider = other;
            ballObject = other.gameObject;
            ballHandler = ballObject.GetComponent<BallHandler>();
        
            //If that ball has no current owner, it hasn't just been kicked, we haven't just kicked it ourselves, and we don't already have a ball...
            if (ballHandler.currentOwner == null && !ballHandler.beenKicked && !kickingBall && !hasBall)
            {
                //Then we can grab the ball
                //Wait until now to set the rigidbody, just in case. We don't want to give control of a ball we can't have grabbed
                ballrb = other.attachedRigidbody;
                GrabBall(ballObject, ballHandler);
            }
            return;
        }
        
        //If the object isn't a ball, we check if it also has a ballgrabber, i.e its a player
        BallGrabber otherPlayer = other.GetComponent<BallGrabber>();
        
        //If so, store that player as our current tackleTarget
        if(otherPlayer != null && otherPlayer != this)
        {
            tackleTarget = otherPlayer;
        }
        
    }

    //This function gets rid of any references we may have stored to an object when it entered our range
    //There ARE more efficient ways to do this, but it's my project and I wanted to do it this way!
    void OnTriggerExit(Collider other)
    {
        BallGrabber otherPlayer = other.GetComponent<BallGrabber>();
        
        if(otherPlayer != null && otherPlayer == tackleTarget)
        {
            tackleTarget = null;
        }
        
        
        if(!other.CompareTag("Ball")) return;
        if(ballHandler != null && ballHandler.currentOwner == this) return;
        
        ballCollider = null;
        ballObject = null;
        ballHandler = null;
        ballrb = null;
    }

    //This function takes the info we stored from the ball and uses it to give the ball to us
    public void GrabBall(GameObject ball, BallHandler bh)
    {
        hasBall = true;
        //The ball itself keeps track of its current owner
        //This is a leftover from when we had a stealing function, but the logic kinda depens on it, so it stays
        bh.currentOwner = this;
        //This sets the ball to not collide with it's owner and turns off the physics so it doesn't roll away
        ballrb.isKinematic = true;
        Physics.IgnoreCollision(playerCol, ballCollider, true);
        //We also lock the ball to it's position relative to the player while they're holding it
        ball.transform.SetParent(this.transform);
        ball.transform.localPosition = ballOffset;
        ballBasePosition = ballOffset;

    }
    
    public void performAction()
    {
        //If we press the action button, we want that action to change depending on if we have a ball or not
        if(ballHandler != null && ballHandler.currentOwner == this)
        {
            KickBall();
        } else
        {
            Tackle();
        }
    }
    
    //This ugly wall of code kicks the ball, while also keeping track of / removing references to it
    public void KickBall()
    {
        //If we somehow managed to get here without the needed ball information, just do nothing
        if(ballObject == null || ballCollider == null || ballrb == null || ballHandler == null) return;
        
        hasBall = false;
        //Play audio and animation for the kick
        playerAudio.playKick();
        animator.SetTrigger("Kick");
        
        //This cooldown prevents us from immedieately picking a ball back up after kicking it
        kickingBall = true;
        
        //This cooldown prevents the ball from being picked up in general right after a kick
        //This could probably be done more elegantly with a reference to the ball's velocity, but this works well enough
        StartCoroutine(ballHandler.KickCooldown());
        
        //Set the ball back to normal, and apply a kicking force to the ball's rigidbody
        Physics.IgnoreCollision(playerCol, ballCollider, false);
        ballrb.isKinematic = false;
        ballObject.transform.SetParent(null);
        Vector3 kickDir = transform.forward + playerRB.linearVelocity * 0.20f + new Vector3(0,0.15f,0);
        ballrb.AddForce(kickDir * kickStrength, ForceMode.Impulse);
        ballHandler.currentOwner = null;
        ballCollider = null;
        ballObject = null;
        ballHandler = null;
        StartCoroutine(KickCooldown());
    }
    
    public void Tackle()
    {
        //If the player is in a fallen state, they shouldn't be able to punch
        if(playerMov.fallen) return;
        //Play the animation for punching
        animator.SetTrigger("Tackle");
        //This small delay is to make the action line up with the animation
        StartCoroutine(TackleDelay());
    }
    
    //Delay the tackle effects from happening to line up with the animation
    IEnumerator TackleDelay()
    {
        yield return new WaitForSeconds(0.15f);
        //Now we can play the sound and do the action
        playerAudio.PlayPunch();
        tackleHit();
    }
    
    //This controls actually hitting a target, and what that does
    public void tackleHit()
    {
        //If we don't actually have anything to hit, then we don't need to run any logic here
        if(tackleTarget == null || tackleTarget == this) return;
        
        //Repurposing the kick cooldown so we don't directly steal a dropped ball, having it roll away instead
        kickingBall = true;
        StartCoroutine(KickCooldown());
        
        //Don't hit other player's while they're down
        if(tackleTarget.playerMov.fallen) return;
        
        //If the target currently has a ball, make them lose it and get pushed
        if(tackleTarget.ballObject != null)
        {
            tackleTarget.LoseBall(transform.forward * (tackleStrength / 5));
            tackleTarget.playerRB.AddForce(transform.forward * tackleStrength, ForceMode.Impulse);
        }
        //Otherwise we just push them
        else
        {
            tackleTarget.playerRB.AddForce(transform.forward * tackleStrength, ForceMode.Impulse);
        }
        
        //Add our hit particles to add impact
        Instantiate(hitParticles, tackleTarget.transform);
        //Stun the target for a little so they can't move, adds impact on the receiving player's end
        tackleTarget.playerMov.Stun(0.3f);
        //Play the audio of the other player getting hit
        tackleTarget.playerAudio.playHit();

        //If the other player is dancing, then we want to add some logic to make them fall over for a bit
        if (tackleTarget.playerMov.isDancing)
        {
            tackleTarget.playerMov.FallOver();
            tackleTarget.animator.SetBool("Fall", true);
        }
        //If they aren't dancing, then we just play the hurt animation and stun them for a smidge longer
        else
        {
            tackleTarget.animator.SetTrigger("Hit");
            tackleTarget.playerMov.Stun(0.3f);
        }
    }
    
    //This makes the player lose the ball, it works essentially the same as a kick, but with force applied from the tackle direction instead
    public void LoseBall(Vector3 tackleDirection)
    {
        if (ballObject == null || ballCollider == null || ballrb == null || ballHandler == null) return;
        
        hasBall = false;
        kickingBall = true;
        Physics.IgnoreCollision(playerCol, ballCollider, false);
        ballObject.transform.SetParent(null);
        ballrb.isKinematic = false;
        ballHandler.currentOwner = null;
        
        ballrb.AddForce(transform.forward + tackleDirection, ForceMode.Impulse);
        
        //StartCoroutine(ballHandler.KickCooldown());
        
        ballCollider = null;
        ballObject = null;
        ballHandler = null;
        ballrb = null;
        
        StartCoroutine(KickCooldown());
    }
    
    //This doesn't need to be here, I just didn't want to add references to the objects in their respective classes
    //So instead, I just kinda pass the message along between classes.
    //I'm pretty sure we ended up adding those references later anyways, so these are probably moot
    public void StartDance()
    {
        animator.SetBool("Dancing", true);
        playerAudio.StartDance();
    }
    
    public void EndDance()
    {
        animator.SetBool("Dancing", false);
        playerAudio.StopDance();
    }

    
    protected IEnumerator KickCooldown()
    {
        yield return new WaitForSeconds(0.2f);
        kickingBall = false;
    }
}
