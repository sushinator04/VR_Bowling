using System;
using UnityEngine;

public class BallSoundController : MonoBehaviour
{
    private AudioSource audioSource;

    public string lastTouched = "none";
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            lastTouched = "Player";
            audioSource.volume = GameVariables.gameVariables.playerBallSoundVolume;
        }else if (collision.gameObject.tag == "Bot")
        {
            lastTouched = "Bot";
            audioSource.volume = GameVariables.gameVariables.botBallSoundVolume;
        }
    }

    void Update()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        
        if (rb.velocity.magnitude > GameVariables.gameVariables.minBallSpeedUntilSound && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        else if (rb.velocity.magnitude <= GameVariables.gameVariables.minBallSpeedUntilSound && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
}