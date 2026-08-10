using UnityEngine;

//This class goes on our GameManager object and is used for any non-local sounds like music etc
public class AudioManager : MonoBehaviour
{
    
    public AudioSource audioSource;
    //We use a seperate audio source for music so that it can be quieter / loop
    public AudioSource musicSource;
    
    public AudioClip goalSound;
    public AudioClip audienceReact;
    public AudioClip newBall;
    public AudioClip backgroundMusic;
    
    void Start()
    {
        //Set some parameters for our background music source
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.Play();
        PlayNewBall();
    }
    
    //These functions just play their specified sounds when called
    public void PlayAudienceReact()
    {
        audioSource.PlayOneShot(audienceReact, 0.2f);
    
    }
    
    public void PlayGoalSound()
    {
        audioSource.PlayOneShot(goalSound, 0.5f);
    }
    
    public void PlayNewBall()
    {
        audioSource.PlayOneShot(newBall, 0.3f);
    }
}
