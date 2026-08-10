using UnityEngine;
using UnityEngine.InputSystem;

//This class functions almost exactly like the playerMovement script, just without allowing for left/right movement
//In hindsight, this probably could've been an inherited class, but oh well
public class GoalieMovement : MonoBehaviour
{
float speed;
    public float moveSpeed;
    public float runSpeed;

    
    public bool isPlayer2;

    public Rigidbody rb;
    public GoalieGrabber gg;
    Vector2 moveInput = Vector2.zero;
    public Animator animator;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogWarning("PlayerController needs a Rigidbody.");
        gg = GetComponent<GoalieGrabber>();
    }

    // Update is called once per frame
    void Update()
    {
        moveInput = Vector2.zero;
        
        if (isPlayer2)
        {
            moveInput = HandlePlayer2Input(moveInput);
        } else
        {
            moveInput = HandlePlayer1Input(moveInput);
        }
        
        
    }
    
    void FixedUpdate()
    {
        
        moveInput.Normalize();
        Vector3 movement =  moveInput * speed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, 0);
        animator.SetFloat("Speed", moveInput.x);
        
    }

    
    
    private Vector2 HandlePlayer1Input(Vector2 moveInput)
    {
        Vector2 keyInput = Vector2.zero;
        //Y input
        if(Keyboard.current.wKey.isPressed) keyInput.x = -1f;
        if(Keyboard.current.sKey.isPressed) keyInput.x = 1f;
        
        if (Keyboard.current.leftShiftKey.isPressed)
        {
            speed = runSpeed;
        } else
        {
            speed = moveSpeed;
        }
        
        if(Gamepad.all.Count > 0)
        {
            Gamepad gamepad = Gamepad.all[0];
            
            Vector2 stick = gamepad.leftStick.ReadValue();
            
            moveInput.x = -stick.y;


            if(gamepad.buttonSouth.wasPressedThisFrame)
            {
                gg.performAction();
            }

            if (gamepad.buttonWest.isPressed)
            {
                speed = runSpeed;
            }
            else
            {
                speed = moveSpeed;
            }
        }
        
        
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
        {
            gg.performAction();
        }
        
        return (moveInput + keyInput).normalized;
    }
    
    private Vector2 HandlePlayer2Input(Vector2 moveInput)
    {
        //Y input
        Vector2 keyInput = Vector2.zero;
        
        if(Keyboard.current.upArrowKey.isPressed) keyInput.x = -1f;
        if(Keyboard.current.downArrowKey.isPressed) keyInput.x = 1f;
        
        if (Keyboard.current.rightShiftKey.isPressed)
        {
            speed = runSpeed;
        } else
        {
            speed = moveSpeed;
        }
        
        if(Gamepad.all.Count > 1)
        {
            Gamepad gamepad = Gamepad.all[1];
            
            Vector2 stick = gamepad.leftStick.ReadValue();
            
            moveInput.x = -stick.y;


            if(gamepad.buttonSouth.wasPressedThisFrame)
            {
                gg.performAction();
            }

            if (gamepad.buttonWest.isPressed)
            {
                speed = runSpeed;
            }
            else
            {
                speed = moveSpeed;
            }
        }
        
        
        if (Keyboard.current.rightCtrlKey.wasPressedThisFrame)
        {
            gg.performAction();
        }
        
        return (moveInput + keyInput).normalized;
    }
}
