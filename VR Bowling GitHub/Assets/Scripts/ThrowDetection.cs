using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowDetection : MonoBehaviour
{

    [SerializeField] AlleyHandler alleyHandler;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ball")
        {
            StartCoroutine(alleyHandler.OnThrowDetected(other.gameObject));
        }
    }
}
