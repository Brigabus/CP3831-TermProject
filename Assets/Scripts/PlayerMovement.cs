using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

//This class controls ALL of player movement
//It was originally copied from the lab4 project, so there may be some remnants from that
//Kind of a ship of theseus situation though
public class PlayerMovement : MonoBehaviour
{
    //Variables controlling player parameters
    float speed;
    public float moveSpeed;
    public float runSpeed;
    public float rotationSpeed;

    /*
    These bools allow you to specifically set which player this is.
    This was originally only one bool for player two, but we wanted to add more players cause it seemed fun.
    Because they were added so late, players 3 and 4 steal the logic from player 2.
    Otherwise I wouldn't use a bool to set them like this, as it's technically possible for a player
    object to be players 2 3 and 4 at the same time.
    
    Players 3 and 4 are just neat little additions in case you want to enable them (with O), otherwise
    they are not intended to be part of the graded project, just a fun addition :)
    */
    public bool isPlayer2;
    public bool isPlayer3;
    public bool isPlayer4;
    
    //Dancing is another just for fun addition that adds a neat taunt mechanic to the game.
    //It's not currently working with keyboard inputs though, as it's just a fun extra and not considered part of the main game
    public bool isDancing;

//Add some player componenet references
    public Rigidbody rb;
    public BallGrabber bg;
    
    //Keep a Vector2 input reference so we can pass it between functions if needed.
    Vector2 moveInput = Vector2.zero;
    
    //Add an Animator to be able to control some player animations
    public Animator animator;
    
    //Some different bools to keep track of player states
    private bool sprinting;
    public bool stunned;
    public bool fallen;
    void Start()
    {
        //Get some componenets from the player.
        //Probably could've automated more of these, but unity makes it so much easier to drag and drop
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogWarning("PlayerController needs a Rigidbody.");
        bg = GetComponent<BallGrabber>();
    }

    // Update is called once per frame
    void Update()
    {
        //Input handling was originally in FixedUpdate, but that meant inputs were sometimes dropped, so we moved it here
        moveInput = Vector2.zero;
        
        //Use different functions depending on the player's number
        //Again, if player 3 and 4 weren't added so late, this would be done in a much more efficient way
        if (isPlayer2)
        {
            moveInput = HandlePlayer2Input(moveInput);
        } else if(isPlayer3)
        {
            moveInput = HandlePlayer3Input(moveInput);
        } else if (isPlayer4)
        {
            moveInput = HandlePlayer4Input(moveInput);
        }
        else
        {
            moveInput = HandlePlayer1Input(moveInput);
        }
        
        
    }
    
    //All of our physics handling is done in FixedUpdate
    void FixedUpdate()
    {
        //We normalize the input so that players can't move faster diagonally, all inputs are normalized between -1 and 1
        moveInput.Normalize();
        //Multiply our inputs by our movement speed...
        Vector3 movement =  moveInput * speed;
        //Apply that movement speed to our rigidbody to move the player
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.y);
        
        //Send our movement velocity to the animator, allowing it to apply our walking animation when moving
        animator.SetFloat("Speed", movement.magnitude);
        //If we're sprinting, we'll also send that to the animator
        animator.SetBool("Sprinting", sprinting);
        
        //This clause rotates the player towards their movement direction over a specific rotation speed
        //ChatGPT helped with the math for this one because Quaternions are scary
        if(movement.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(movement.x, 0, movement.y));
            
            Quaternion newRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            
            rb.MoveRotation(newRotation);
        }
        
        
    }

    /*
    The bulk of this class is devoted to handling each player's input.
    All players have this code, but only the one who's player number matches the function will use that function
    This is by no means a good way of doing this, and is extremely inneficient for space, as most of these are just copy-pastes
    but it works, which is good enough for now probably
    */
    private Vector2 HandlePlayer1Input(Vector2 moveInput)
    {
        //We use a seperate Vector2 here to track keyboard inputs to allow simultaneous keyboard / controller movement.
        //This is done for both player 1 and 2
        Vector2 keyInput = Vector2.zero;
        //Y input
        if(Keyboard.current.wKey.isPressed) keyInput.x = -1f;
        if(Keyboard.current.sKey.isPressed) keyInput.x = 1f;
        
        //X input
        if(Keyboard.current.aKey.isPressed) keyInput.y = -1f;
        if(Keyboard.current.dKey.isPressed) keyInput.y = 1f;
        
        //Handle Sprinting
        if (Keyboard.current.leftShiftKey.isPressed)
        {
            speed = runSpeed;
            //Set the bool for animation purposes
            sprinting = true;
        } else
        {
            speed = moveSpeed;
            sprinting = false;
        }
        
        //We wanted to add gamepad support because using a keyboard for two people is cramped
        //Also I wanted to play on the TV and I get what I want >:C
        //First, we make sure that there are at least enough controllers connected for the current player to use one.
        if(Gamepad.all.Count > 0)
        {
            //Then, we assign the corresponding gamepad to the player here
            Gamepad gamepad = Gamepad.all[0];
            
            //We use the stick input to set our vector2, the same way we do with the keyboard inputs
            Vector2 stick = gamepad.leftStick.ReadValue();
            
            //We had to change the vector2 for keyboard inputs above, as if there were any controllers connected
            //This part would fire and set the moveInput vector to nothing, even if keyboard inputs were happening
            moveInput.x = -stick.y;
            moveInput.y = stick.x;


            //These check if specific buttons are pressed, and perform actions if they are
            if(gamepad.buttonSouth.wasPressedThisFrame)
            {
                bg.performAction();
            }
            
            //Same sprinting logic, but with gamepad buttons
            if (gamepad.buttonWest.isPressed)
            {
                speed = runSpeed;
                sprinting = true;
            }
            //This if clause was added for the same reason that the keyInput vector was made, otherwise keyboard sprinting wouldn't work
            else if(!Keyboard.current.leftShiftKey.isPressed)
            {
                speed = moveSpeed;
                sprinting = false;
            }

            //Dance
            //Dancing was added later on and only works with controllers
            //We don't want to be able to dance if we're downed
            if (gamepad.buttonEast.wasPressedThisFrame && !fallen)
            {
                //Send the startdance commands through the ballgrabber
                bg.StartDance();
                //Set tthe dnacing state to true. This allows us to track if a dancing player is punched and knock them over
                isDancing = true;
            }

            //Stop dancing if the dance button is released
            if (gamepad.buttonEast.wasReleasedThisFrame && !fallen)
            {
                bg.EndDance();
                isDancing = false;
            }
            
        }
        
        //These don't currently work, most likely for the same reason as the keyInput vector thing,
        //but with dnacing being an extra thing, I don't mind leaving it as controller exclusive for now
        if (Keyboard.current.rightAltKey.wasPressedThisFrame)
            {
                bg.StartDance();
                isDancing = true;
            }

            if (Keyboard.current.rightAltKey.wasReleasedThisFrame)
            {
                bg.EndDance();
                isDancing = false;
            }
        
        //This has the BallGrabber class perform an appropriate action when the action button is pressed
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
        {
            bg.performAction();
        }
        
        //If we're dancing, stunned, or downed, we shouldn't be able to move...
        if(isDancing || stunned || fallen)
        {
            //So we simply return the no input was detected
            return Vector2.zero;
        }
        else
        {
            //Otherwise, send the normalized input between the keyboard and controller back
            //Without the normalize, pressing directions on both controller and keyboard at once would double your speed
            return (moveInput + keyInput).normalized;
        }
        
    }
    
    //Players 2 inputs are a copy-paste of player 1's input function, they just run for different inputs depending on the player
    private Vector2 HandlePlayer2Input(Vector2 moveInput)
    {
        
        Vector2 keyInput = Vector2.zero;
        //Y input
        if(Keyboard.current.upArrowKey.isPressed) keyInput.x = -1f;
        if(Keyboard.current.downArrowKey.isPressed) keyInput.x = 1f;
        
        //X input
        if(Keyboard.current.leftArrowKey.isPressed) keyInput.y = -1f;
        if(Keyboard.current.rightArrowKey.isPressed) keyInput.y = 1f;
        

        if (Keyboard.current.rightShiftKey.isPressed)
        {
            speed = runSpeed;
            sprinting = true;
        } else
        {
            speed = moveSpeed;
            sprinting = false;
        }
        
        if(Gamepad.all.Count > 1)
        {
            Gamepad gamepad = Gamepad.all[1];
            
            Vector2 stick = gamepad.leftStick.ReadValue();
            
            moveInput.x = -stick.y;
            moveInput.y = stick.x;


            if(gamepad.buttonSouth.wasPressedThisFrame)
            {
                bg.performAction();
            }

            if (gamepad.buttonWest.isPressed)
            {
                speed = runSpeed;
                sprinting = true;
            }
            else if(!Keyboard.current.rightShiftKey.isPressed)
            {
                speed = moveSpeed;
                sprinting = false;
            }
            
                        //Dance
            if (gamepad.buttonEast.wasPressedThisFrame && !fallen)
            {
                bg.StartDance();
                isDancing = true;
            }


            if (gamepad.buttonEast.wasReleasedThisFrame && !fallen)
            {
                bg.EndDance();
                isDancing = false;
            }
        }
        

        if (Keyboard.current.rightCtrlKey.wasPressedThisFrame)
        {
            bg.performAction();
        }
        
        
        if(isDancing || stunned || fallen)
        {
            return Vector2.zero;
        }
        else
        {
            return (moveInput + keyInput).normalized;
        }
    }
    
    //Player 3 and 4's inputs are also copy pasted, but without any of the keyboard componenets, as they are controller playable only
    //If you've got 4 controllers available though, it's pretty fun!
    private Vector2 HandlePlayer3Input(Vector2 moveInput)
    {       
        if(Gamepad.all.Count > 2)
        {
            Gamepad gamepad = Gamepad.all[2];
            
            Vector2 stick = gamepad.leftStick.ReadValue();
            
            moveInput.x = -stick.y;
            moveInput.y = stick.x;


            if(gamepad.buttonSouth.wasPressedThisFrame)
            {
                bg.performAction();
            }

            if (gamepad.buttonWest.isPressed)
            {
                speed = runSpeed;
                sprinting = true;
            }
            else
            {
                speed = moveSpeed;
                sprinting = false;
            }
            
                        //Dance
            if (gamepad.buttonEast.wasPressedThisFrame && !fallen)
            {
                bg.StartDance();
                isDancing = true;
            }


            if (gamepad.buttonEast.wasReleasedThisFrame && !fallen)
            {
                bg.EndDance();
                isDancing = false;
            }
        }

        
        if(isDancing || stunned || fallen)
        {
            return Vector2.zero;
        }
        else
        {
            return moveInput;
        }
    }
    
        private Vector2 HandlePlayer4Input(Vector2 moveInput)
    {       
        if(Gamepad.all.Count > 3)
        {
            Gamepad gamepad = Gamepad.all[3];
            
            Vector2 stick = gamepad.leftStick.ReadValue();
            
            moveInput.x = -stick.y;
            moveInput.y = stick.x;


            if(gamepad.buttonSouth.wasPressedThisFrame)
            {
                bg.performAction();
            }

            if (gamepad.buttonWest.isPressed)
            {
                speed = runSpeed;
                sprinting = true;
            }
            else
            {
                speed = moveSpeed;
                sprinting = false;
            }
            
                        //Dance
            if (gamepad.buttonEast.wasPressedThisFrame && !fallen)
            {
                bg.StartDance();
                isDancing = true;
            }

            if (gamepad.buttonEast.wasReleasedThisFrame && !fallen)
            {
                bg.EndDance();
                isDancing = false;
            }
        }
        
        
        if(isDancing || stunned || fallen)
        {
            return Vector2.zero;
        }
        else
        {
            return moveInput;
        }
    }
    
    //This function stuns the player for a specified amount of time
    public void Stun(float seconds)
    {
        //we set the stunned flag to true
        stunned = true;
        //Then start a coroutine that waits for the specified amount of seconds before disabling the flag
        StartCoroutine(StunTimer(seconds));
    }
    
    //FallOver is similar to stun, but has a specific amount of time that it waits
    public void FallOver()
    {
        //Set the flag to true
        fallen = true;
        //Stop the player from dancing so the music stops
        bg.EndDance();
        isDancing = false;
        //Start a coroutine that waits for a few seconds before turning the flag to false
        StartCoroutine(FallTimer());
        
    }
    
    IEnumerator FallTimer()
    {
        yield return new WaitForSeconds(5);
        fallen = false;
        //Let the animator know we aren't stunned anymore, so we can get up
        animator.SetBool("Fall", false);
    }
    
    IEnumerator StunTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        stunned = false;
    }
    
}
