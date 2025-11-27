using System;
using System.Collections;
using System.Collections.Generic;
using NPC;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class AlleyHandler : MonoBehaviour
{
    [SerializeField] private float waitUntilCountPins;
    
    private BallRespawn ballRespawn;
    private PinRespawn pinRespawn;
    private ScoreCalculator scoreCalculator;
    private ThrowDetection throwDetection;
    private ScoreDisplayHandler scoreDisplayHandler;
    private BotHandler botHandler;
    private TextGrowWithParticles textGrowWithParticles;
    [SerializeField] private BallReturnChecker ballReturnChecker;
    [SerializeField] public bool isBotAlley;

    public int alleyIndex;

    public int totalPlayers;
    public int botCount;
    public int realPlayers;
    
    public int currentPlayer;
    
    int shotIndex = 0;

    private void Awake()
    {
        int start = name.IndexOf("(") + 1;
        int end = name.IndexOf(")");
        int.TryParse(name.Substring(start, end - start), out alleyIndex); 
        
        
        ballRespawn = GetComponent<BallRespawn>();
        pinRespawn = GetComponent<PinRespawn>();
        scoreCalculator = GetComponent<ScoreCalculator>();
        throwDetection = transform.Find("ThrowDetection").GetComponent<ThrowDetection>();
        scoreDisplayHandler = GetComponent<ScoreDisplayHandler>();
        botHandler = GetComponent<BotHandler>();
        textGrowWithParticles = GetComponent<TextGrowWithParticles>();
    }

    private void Start()
    {
        if (isBotAlley)
        {
            StartCoroutine(StartBotGameAfterRandomTime());
        }
    }

    public PinRespawn GetPinRespawn()
    {
        return pinRespawn;
    }

    public BallReturnChecker GetBallReturnChecker()
    {
        return ballReturnChecker;
    }

    private IEnumerator StartBotGameAfterRandomTime()
    {
        int waitTime = Random.Range(5, 20);
        yield return new WaitForSeconds(waitTime);
        StartGame(0, Random.Range(1,3), BotVariables.BotLevel.High, GameVariables.GameModes.Normal);
    }

    private IEnumerator EndGame()
    {
        yield return StartCoroutine(pinRespawn.EndGame());
        ballRespawn.EndGame(alleyIndex);

        if (isBotAlley)
        {
            StartCoroutine(StartBotGameAfterRandomTime());
        }
    }
    
    public void StartGame(int numPlayers, int numBots, BotVariables.BotLevel level, GameVariables.GameModes gameMode)
    {
        totalPlayers = numPlayers+numBots;
        realPlayers = numPlayers;
        botCount = numBots;
        
        StopAndResetAll();
        
        String[] playerNames = new string[totalPlayers];
        for (int i = 0; i < numPlayers; i++)
        {
            playerNames[i] = "Player " + (i+1);
        }

        for (int i = 0; i < numBots; i++)
        {
            playerNames[i+numPlayers] = "Bot " + i;
        }
        
        currentPlayer = 0;

        StartCoroutine(pinRespawn.InitializeNewGame(gameMode));

        
        if (numBots > 0)
        {
            StartCoroutine(botHandler.StartBots(numBots, level));
        }

        if (gameMode == GameVariables.GameModes.Normal)
        {
            StartCoroutine(ballRespawn.InitializeNewGame(alleyIndex, gameMode));
        }else if (gameMode == GameVariables.GameModes.Fun)
        {
            StartCoroutine(ballRespawn.InitializeNewGame(alleyIndex, gameMode)); // TODO: Maybe different balls?
            // TODO: Implement Fun Mode
        }
        scoreDisplayHandler.InitializeNewGame(playerNames);
        scoreCalculator.InitializeNewGame(totalPlayers);
    }
    
    public IEnumerator OnThrowDetected(GameObject thrownBall)
    {
        yield return new WaitForSeconds(waitUntilCountPins);

        List<Transform> fallenPins = pinRespawn.GetFallenPins();
        List<Transform> standingPins = pinRespawn.GetStandingPins();
        
        // Update Score
        scoreCalculator.OnThrowDetected(fallenPins.Count);

        // Flush and Respawn
        if (currentPlayer >= 0)
        {
            // game still running
            yield return StartCoroutine(pinRespawn.FlushAlley());
            if (shotIndex == 0 && fallenPins.Count != 10)
            {
                pinRespawn.PinsToResetter(fallenPins);
                StartCoroutine(pinRespawn.PlacePins(standingPins));
                shotIndex = 1;
            }
            else 
            {
                StartCoroutine(pinRespawn.PlaceAllPins());
                shotIndex = 0;
            }

            StartCoroutine(ballRespawn.RespawnBall(alleyIndex, thrownBall));
        }
        else
        {
            // game finished (playerIndex == -1)
            StartCoroutine(EndGame());
        }
        
    }

    public void MakeStrikeAnimation(int playerIndex)
    {
        // only make animation if its a real player
        if (playerIndex < realPlayers)
        {
            textGrowWithParticles.PlayStrikeAnimation();
        }
    }
    
    public void MakeSpareAnimation(int playerIndex)
    {
        // only make animation if its a real player
        if (playerIndex < realPlayers)
        {
            textGrowWithParticles.PlaySpareAnimation();
        }
    }



    public void StopAndResetAll()
    {
        pinRespawn.StopAllCoroutines();
        ballRespawn.StopAllCoroutines();
        throwDetection.StopAllCoroutines();
        this.StopAllCoroutines();
    }
    
}
