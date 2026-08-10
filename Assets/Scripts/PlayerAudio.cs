using UnityEngine;


public class PlayerAudio : MonoBehaviour
{
    //Add two audio sources, so we can have one loop and one for oneshots
    public AudioSource audioSource;
    public AudioSource danceSource;
    
    //Add some audio clips for various sounds
    public AudioClip punchSound;
    public AudioClip hitSound;
    public AudioClip kickSound;
    public AudioClip danceMusic;
    
    //Add some functions to be called for when we want to play sounds
    public void PlayPunch()
    {
        audioSource.PlayOneShot(punchSound);
    }
    
    public void playHit()
    {
        audioSource.PlayOneShot(hitSound);
    }
    
    public void playKick()
    {
        audioSource.PlayOneShot(kickSound);
    }
    
    //These functions start and stop the looping dance audio, instead of just playing it as a oneshot
    public void StartDance()
    {
        danceSource.Play();
    }
    
    public void StopDance()
    {
        danceSource.Stop();
    }
}
