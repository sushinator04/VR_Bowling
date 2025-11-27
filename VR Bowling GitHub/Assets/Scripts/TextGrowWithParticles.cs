using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TextGrowWithParticles : MonoBehaviour
{
    [SerializeField] private GameObject strikeTextPrefab;
    [SerializeField] private GameObject spareTextPrefab;
    [SerializeField] private Vector3 localStart;
    [SerializeField] private Vector3 localEnd;
    [SerializeField] private float duration;




    public enum BowlingTexts
    {
        Strike,
        Spare
    }
    
    public IEnumerator MakeTextAnimation(BowlingTexts textType)
    {
        // Choose the appropriate prefab
        GameObject textPrefab = (textType == BowlingTexts.Strike) ? strikeTextPrefab : spareTextPrefab;

        // Instantiate the text prefab at the start position
        GameObject textInstance = Instantiate(textPrefab,transform.position-localStart, Quaternion.Euler(-90,0,0), transform);
        textInstance.transform.localPosition = localStart;
        textInstance.transform.localScale = Vector3.zero;

        ParticleSystem[] particleSystems = textInstance.GetComponentsInChildren<ParticleSystem>();
        foreach (var sys in particleSystems)
        {
            sys.Play();
        }
        

        // Animate the text movement and growth
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            // Apply ease-out cubic function
            float easedT = 1f - Mathf.Pow(1f - t, 3);

            // Lerp position and scale using easedT
            textInstance.transform.localPosition = Vector3.Lerp(localStart, localEnd, easedT);
            textInstance.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 100, easedT);

            yield return null;
        }

        // Ensure final position and scale
        textInstance.transform.localPosition = localEnd;
        textInstance.transform.localScale = Vector3.one * 40;
        
        Destroy(textInstance);
    }

    
    public void PlayStrikeAnimation()
    {
        StartCoroutine(MakeTextAnimation(BowlingTexts.Strike));
    }

    public void PlaySpareAnimation()
    {
        StartCoroutine(MakeTextAnimation(BowlingTexts.Spare));
    }
}
