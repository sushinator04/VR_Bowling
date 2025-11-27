using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class BallReturnChecker : MonoBehaviour
{
    [SerializeField] private List<Transform> readyBalls;
    private Dictionary<Transform, bool> ballIsAvailable;

    public TaskCompletionSource<Transform> ballReadyTcs;
    public event Action OnBallReady;

    private void Awake()
    {
        ballReadyTcs = new TaskCompletionSource<Transform>();
        ballIsAvailable = new Dictionary<Transform, bool>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            readyBalls.Add(other.transform);
            ballReadyTcs.TrySetResult(other.transform);
            ballReadyTcs = new TaskCompletionSource<Transform>();
            OnBallReady?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            readyBalls.Remove(other.transform);
        }
    }

    public Task<Transform> WaitForBallReady()
    {
        return ballReadyTcs.Task;
    }
    

    public Transform GetBallTargetIfAvailable()
    {
        int ballCount = readyBalls.Count;

        if (ballCount == 0)
        {
            return null;
        }
        else
        {
            int randomBallIndex = Random.Range(0, ballCount);
            
            return readyBalls[randomBallIndex];
        }
        
    }
    
}