using UnityEngine;

public class StrikeSoundController : MonoBehaviour
{
    public AudioSource audioSource1;
    public AudioSource audioSource2;

    void Start()
    {
        audioSource1 = GetComponent<AudioSource>();
        audioSource2 = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        // Play the audio each time the prefab is enabled
        audioSource1.Play();
        audioSource2.Play();
    }
    /*void Update()
    {
        
            audioSource.Play();
       
    }*/
}