using UnityEngine;

public class PinSoundController : MonoBehaviour
{
    public AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
     
        if (collision.gameObject.tag == "Ball")
        {
            string lastTouched = collision.gameObject.GetComponent<BallSoundController>().lastTouched;
            
            if (lastTouched == "Player")
            {
                audioSource.volume = GameVariables.gameVariables.playerPinSoundVolume;
            }else if (lastTouched == "Bot")
            {
                audioSource.volume = GameVariables.gameVariables.botPinSoundVolume;
            }
            
            audioSource.Play();
        }
    }
}