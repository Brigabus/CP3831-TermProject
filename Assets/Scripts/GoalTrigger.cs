using System.Collections;
using UnityEngine;
using System;

//This script controls the logic for scoring a goal
public class GoalTrigger : MonoBehaviour
{
    //Similar to playermovement, we just use a bool to seperate logic between players
    public bool isPlayer2Net;
    
    //Keep a reference to the ball prefab so we can spawn more when needed
    public GameObject ballPrefab;
    //Keep a public offset so we can easily change wheree the ball is spawned
    public Vector3 ballSpawnPos;
    
    //Store a particle object to be instantiated when a goal is scored
    public GameObject goalParticles;
    
    //This action is used to allow the audience members to react to when a goal is scored.
    //If this wasn't added so late, it might have been useful for other functions too
    public static event Action goalScored;
    
    public AudioManager audioManager;


    void OnTriggerEnter(Collider other)
    {
        //Check if the object entering is a ball
        if (other.CompareTag("Ball"))
        {
            //Increase the score depending on who's net it is
            if (isPlayer2Net)
            {
                GameManager.p1Score += 1;
            }
            else
            {
                GameManager.p2Score += 1;
            }
            
            //Destroy the ball and leave confetti in its place
            Instantiate(goalParticles, other.transform.position, Quaternion.LookRotation(Vector3.up));
            Destroy(other.gameObject);
            
            //Wait a bit, then spawn a new ball
            StartCoroutine(SpawnNewBall());
            
        }
    }
    
    IEnumerator SpawnNewBall()
    {
        //Before we wait, we'll play a sound for the goal being scored.
        //Why is this in the coroutine?
        audioManager.PlayGoalSound();
        //Wait just a smidge before letting the audience react, otherwise the noises get overlapped
        yield return new WaitForSeconds(0.2f);
        goalScored?.Invoke();
        audioManager.PlayAudienceReact();
        
        //Wait a bit longer before spawning a new ball
        yield return new WaitForSeconds(4);
        Instantiate(ballPrefab, ballSpawnPos, Quaternion.identity);
        //Play a sound to let players know a new ball has appeared, in case it is offscreen somehow
        audioManager.PlayNewBall();
    }


}
