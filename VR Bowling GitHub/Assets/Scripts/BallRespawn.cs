using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallRespawn : MonoBehaviour
{

    [SerializeField] private Transform ballRespawnModel;
    [SerializeField] private float forceAtRespawn;
    [SerializeField] private float ballRespawnTime;

    [SerializeField] private int ballCountNormalMode;

    private Dictionary<int, List<GameObject>> alleyBallDict;
    
    private Vector3 respawnPosition;
    private BallRespawnQueueHandler ballRespawnQueueHandler;
    
    private void Awake()
    {
        alleyBallDict = new Dictionary<int, List<GameObject>>();
        respawnPosition = ballRespawnModel.Find("BallRespawnPosition").transform.position;
        ballRespawnQueueHandler = ballRespawnModel.Find("BallRespawnPosition").GetComponent<BallRespawnQueueHandler>();
    }

    public IEnumerator RespawnBall(int alleyIndex, GameObject ball, bool isFirstTime = false)
    {
        if (!isFirstTime)
        {
            yield return new WaitForSeconds(ballRespawnTime);
        }

        while (ballRespawnQueueHandler.isCurrentlyUsed)
        {
            yield return new WaitUntil(() => !ballRespawnQueueHandler.isCurrentlyUsed);
        }

        ballRespawnQueueHandler.isCurrentlyUsed = true;

        ball.GetComponent<Rigidbody>().isKinematic = true;

        ball.transform.position = respawnPosition;

        ball.GetComponent<Rigidbody>().isKinematic = false;

        // Use alley index to make it more probable that ball gets to right alley side in return system
        int zDir = (alleyIndex % 2 == 0) ? 1 : -1; 
        Vector3 forceDirection = new Vector3(-5,0,zDir).normalized;
        ball.GetComponent<Rigidbody>().AddForce(forceDirection*forceAtRespawn, ForceMode.Acceleration);
        
        yield return new WaitForSeconds(2f);
        ballRespawnQueueHandler.isCurrentlyUsed = false;
    }


    public IEnumerator InitializeNewGame(int alleyIndex, GameVariables.GameModes gameMode)
    {
        // TODO: Different balls for different modes?
        if (alleyBallDict.ContainsKey(alleyIndex))
        {
            List<GameObject> ballsToDestroy = alleyBallDict[alleyIndex];
            for (int i = 0; i < ballsToDestroy.Count; i++)
            {
                Destroy(ballsToDestroy[i]);
            }

            alleyBallDict.Remove(alleyIndex);
        }
        
        List<GameObject> newBalls = new List<GameObject>();
        
        for (int i = 0; i < ballCountNormalMode; i++)
        {
            GameObject ball;
            
            if (gameMode == GameVariables.GameModes.Normal)
            {
                ball = Instantiate(GameVariables.gameVariables.bowlingBall, new Vector3(0,0,-200), Quaternion.identity); // -200 to not be visible at first for player
            }
            else
            {
                //TODO: Fun bowling ball
                ball = Instantiate(GameVariables.gameVariables.bowlingBall, new Vector3(0,0,-200), Quaternion.identity); // -200 to not be visible at first for player
            }
            
            int matIndex = i%GameVariables.gameVariables.normalBallMats.Count;
            ball.GetComponent<Renderer>().material = GameVariables.gameVariables.normalBallMats[matIndex];
            newBalls.Add(ball);
        }
        
        alleyBallDict.Add(alleyIndex, newBalls);

        foreach (var ball in newBalls)
        {
            yield return StartCoroutine(RespawnBall(alleyIndex, ball, true));
        }
    }

    public void EndGame(int alleyIndex)
    {
        List<GameObject> balls = alleyBallDict[alleyIndex];
        for (int i = 0; i < balls.Count; i++)
        {
            Destroy(balls[i]);
        }
    }

}
