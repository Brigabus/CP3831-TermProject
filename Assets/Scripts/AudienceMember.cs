using System.Collections;
using UnityEngine;

//This class controls the audience members in the background
//It tells them to jump when a goal is scored

public class AudienceMember : MonoBehaviour
{
    
    
    public float jumpHeight;
    public float goalTime = 0.6f;
    
    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool celebrating;
    
    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void OnEnable()
    {
        /*
        To avoid having to specify both goals to all audience members, they instead
        subscribe to an static Action set up in the GoalTrigger class that is invoked when a goal is scored
        */
        GoalTrigger.goalScored += GoalReact;
    }

    void OnDisable()
    {
        GoalTrigger.goalScored -= GoalReact;
    }
    

    //This is called when the goalScored Action is triggered by the GoalTrigger script
    public void GoalReact()
    {
        //This if clause keeps the audience from starting the celebration while they're still celebrating
        if (!celebrating)
        {
            StartCoroutine(Celebration());
        }
        
    }
    
    IEnumerator Celebration()
    {
        //This coroutine makes the audience jump up and spin in a circle
        //Giving the audience rigidbodies impacted performance, so we do it manually instead
        celebrating = true;
        
        float time = 0f;
        
        while(time < goalTime)
        {
            time += Time.deltaTime;
            float progress = time / goalTime;
            
            float height = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
            
            transform.position = startPosition + Vector3.up * height;
            transform.rotation = startRotation * Quaternion.Euler(0f, progress * 360f, 0f);
            
            yield return null;
        }
        
        celebrating = false;
        transform.position = startPosition;
        transform.rotation = startRotation;
        
    }


}
