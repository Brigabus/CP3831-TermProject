using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

//This class handles universal game stuff like score
public class GameManager : MonoBehaviour
{
    //Keep a reference to the ball prefab if we ever need to spawn it
    public GameObject ball;
    
    //An array of all the players. This was added later to enable player 3 and 4
    public PlayerMovement[] players;
    
    public static int p1Score;
    public static int p2Score;
    
    //This text is shown on the ui canvas
    public TMP_Text scoreText;
    
    //This was added later to enable players 3 and 4
    public bool extraPlayersActive = true;
    
    void Update()
    {
        //Update the hud every frame in case it changes
        scoreText.text = p1Score + " - P1  :SCORE: P2 - " + p2Score;

        //Escape closes the game
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Application.Quit();
        }
        
        //This is a debug command to spawn balls. Multiple balls break the game. Don't spawn more balls!
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Instantiate(ball, new Vector3(0f, 10f, 0f), Quaternion.identity);
        }
        
        //Added later to allow toggling of player 3 and 4
        if(Keyboard.current.oKey.wasPressedThisFrame)
            {
                ToggleP3P4();
                
            }
    }
    
    void ToggleP3P4()
    {
        foreach(PlayerMovement player in players)
        {
            //When called, enable / disable player3 and 4 game objects, instead of destroying / instantiating every time
            if(player.isPlayer3 || player.isPlayer4)
            {
                player.gameObject.SetActive(extraPlayersActive);
            }
        }
        //Toggle the bool
        extraPlayersActive = !extraPlayersActive;
    }
    
    
}
