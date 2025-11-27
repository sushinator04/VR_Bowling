using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class BotVariables : MonoBehaviour
{
    [SerializeField] public Transform rightMiddleFingerBone;
    [SerializeField] public Transform targetHandIK;
    [SerializeField] public Transform rightIndexFinger;
    [SerializeField] public ChainIKConstraint handIKConstraint;

    [SerializeField] private float forceMin;
    [SerializeField] private float forceMax;
    [SerializeField] private float HighDirectionVariation;
    [SerializeField] private float MidDirectionVariation;
    [SerializeField] private float LowDirectionVariation;

    public PinRespawn pinRespawn;
    public BotLevel level;
    

    public enum BotLevel
    {
        Low,
        Mid,
        High
    }
    
    
    public void OnThrowAnimationEvent()
    {
        // GetBall
        if (rightIndexFinger.childCount > 0)
        {
            Transform ball = rightIndexFinger.GetChild(0);

            ball.parent = null;

            Rigidbody rigidbody = ball.GetComponent<Rigidbody>();
            rigidbody.isKinematic = false;

            SphereCollider sphereCollider = ball.GetComponent<SphereCollider>();
            sphereCollider.isTrigger = false;

            
            float force = Random.Range(forceMin, forceMax);
            Vector3 direction = Vector3.right;
            
            
            List<Transform> standingPins = pinRespawn.GetStandingPins();

            if (standingPins.Count > 0)
            {
                Transform target = standingPins[0];
                float randOffset;
                
                if (level == BotLevel.High)
                {
                    // perfect shot (probably :))
                    randOffset = Random.Range(-HighDirectionVariation, HighDirectionVariation);
                }else if (level == BotLevel.Mid)
                {
                    randOffset = Random.Range(-MidDirectionVariation, MidDirectionVariation);
                }
                else
                {
                    randOffset = Random.Range(-LowDirectionVariation, LowDirectionVariation);
                }
                
                direction = (target.position - rightMiddleFingerBone.position + new Vector3(0,0, randOffset)).normalized;
            }
            else
            {
                Debug.Log("Bot is throwing ball on empty alley!");
            }
            
            
            rigidbody.AddForce(direction * force, ForceMode.Impulse);
            
            transform.GetComponent<Animator>().SetBool("isHoldingBall", false);
        }
    }
}