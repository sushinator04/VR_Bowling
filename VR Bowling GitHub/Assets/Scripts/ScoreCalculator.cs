using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ScoreCalculator : MonoBehaviour
{
    private AlleyHandler alleyHandler;
    private ScoreDisplayHandler scoreDisplayHandler;

    private int[,] playerThrows; //[playerIndex, throwIndex]
    private int[,] score; //[playerIndex, throwIndex]
    private int[] playerThrowIndices;

    private void Awake()
    {
        alleyHandler = GetComponent<AlleyHandler>();
        scoreDisplayHandler = GetComponent<ScoreDisplayHandler>();
    }


    public void InitializeNewGame(int playerCount)
    {
        playerThrows = new int[playerCount, 21];
        score = new int[playerCount, 11];
        playerThrowIndices = new int[playerCount];

        alleyHandler.currentPlayer = 0;
    }

    public void OnThrowDetected(int pinCount)
    {
        int playerIndex = alleyHandler.currentPlayer;
        int throwIndex = playerThrowIndices[playerIndex];

        playerThrows[playerIndex, throwIndex] = pinCount;

        UpdatePlayerScore(playerIndex);

        if (throwIndex < 18)
        {
            scoreDisplayHandler.UpdateScoreTotals(playerIndex, (throwIndex+1)/2,score);
            if (throwIndex % 2 == 0)
            {
                // First throw
                if (pinCount == 10)
                {
                    // Strike
                    alleyHandler.MakeStrikeAnimation(playerIndex);
                    playerThrowIndices[playerIndex] += 2;
                    NextFrameOrPlayer();
                }
                else
                {
                    // has 2nd throw
                    playerThrowIndices[playerIndex]++;
                }
            }
            else
            {
                if (playerThrows[playerIndex, throwIndex - 1] + playerThrows[playerIndex, throwIndex] == 10)
                {
                    alleyHandler.MakeSpareAnimation(playerIndex);
                }

                // 2nd throw
                playerThrowIndices[playerIndex]++;
                NextFrameOrPlayer();
            }
        }
        else
        {
            if (playerThrowIndices[playerIndex] == 18)
            {
                if (pinCount == 10)
                {
                    // Strike -> additional 2 throw
                    alleyHandler.MakeStrikeAnimation(playerIndex);
                    playerThrowIndices[playerIndex] += 1;
                }
                else
                {
                    // has 1 throw left to to try spare and get additional throw else finished after next
                    playerThrowIndices[playerIndex]++;
                }
            }
            else if (playerThrowIndices[playerIndex] == 19)
            {
                // Check if spare or had strike before for additional shot!
                if (playerThrows[playerIndex, 18] + playerThrows[playerIndex, 19] == 10)
                {
                    alleyHandler.MakeSpareAnimation(playerIndex);
                    playerThrowIndices[playerIndex] += 1;
                }
                else if (playerThrows[playerIndex, 18] == 10)
                {
                    if (playerThrows[playerIndex, 19] == 10)
                    {
                        alleyHandler.MakeStrikeAnimation(playerIndex);
                    }
                    playerThrowIndices[playerIndex] += 1;
                }
                else
                {
                    score[playerIndex, 10] = score[playerIndex, 9];
                    scoreDisplayHandler.UpdateScoreTotals(playerIndex, 11, score);
                    NextFrameOrPlayer();
                }
            }
            else
            {
                // Bonus throws used up
                if (playerThrowIndices[playerIndex] == 20 && playerThrows[playerIndex, 20] == 10)
                {
                    alleyHandler.MakeStrikeAnimation(playerIndex);
                    
                }
                
                
                score[playerIndex, 10] = score[playerIndex, 9];
                scoreDisplayHandler.UpdateScoreTotals(playerIndex, 11, score);
                NextFrameOrPlayer();
            }
        }
    }


    private void NextFrameOrPlayer()
    {
        alleyHandler.currentPlayer = (alleyHandler.currentPlayer + 1) % alleyHandler.totalPlayers;
        scoreDisplayHandler.DisplayCurrentPlayer(alleyHandler.currentPlayer);

        if (playerThrowIndices[alleyHandler.currentPlayer] >= 19)
        {
            // GAME ENDED
            Debug.Log("GAME FINISHED");
            scoreDisplayHandler.DisplayWinner(score);
            alleyHandler.currentPlayer = -1;
        }
    }


    public void UpdatePlayerScore(int playerIndex)
    {
        int accumulator = 0;
        int throwIndex = 0;
        int frame = 0;

        while (throwIndex <= playerThrowIndices[playerIndex])
        {
            frame = throwIndex / 2;

            if (IsStrike(playerIndex, throwIndex))
            {
                scoreDisplayHandler.UpdateScoreDisplayThrow(playerIndex, throwIndex, -1, isStrike: true);


                // Calculate bonuses for strike (max 2 throws)
                int b1 = 0;
                int b2 = 0;
                if (throwIndex < 18) // Bowling Rule: NO BONUS for last frame but get additional shot if strike or spare
                {
                    b1 = playerThrows[playerIndex, throwIndex + 2];
                    

                    if (IsStrike(playerIndex, throwIndex + 2))
                    {
                        if (throwIndex == 16)
                        {
                            b2 = playerThrows[playerIndex, throwIndex + 3];
                        }
                        else
                        {
                            b2 = playerThrows[playerIndex, throwIndex + 4];
                        }
                        
                    }
                    else
                    {
                        b2 = playerThrows[playerIndex, throwIndex + 3];
                    }
                }

                accumulator += 10 + b1 + b2;
                if (throwIndex == 18 || throwIndex == 19)
                {
                    throwIndex += 1;
                }
                else
                {
                    throwIndex += 2;
                }
            }
            else if (IsSpare(playerIndex, throwIndex))
            {
                scoreDisplayHandler.UpdateScoreDisplayThrow(playerIndex, throwIndex, -1, isSpare: true);

                int b1 = 0;
                if (throwIndex < 18) // Bowling Rule: NO BONUS for last frame but get additional shot if strike or spare
                {
                    b1 = playerThrows[playerIndex, throwIndex + 1];
                }

                // Calculate bonus for spare (1 throw)
                accumulator += playerThrows[playerIndex, throwIndex] + b1;
                throwIndex += 1;
            }
            else
            {
                scoreDisplayHandler.UpdateScoreDisplayThrow(playerIndex, throwIndex, playerThrows[playerIndex, throwIndex]);

                accumulator += playerThrows[playerIndex, throwIndex];
                throwIndex += 1;
            }

            if (frame == 10)
            {
                score[playerIndex, 9] = accumulator;
            }
            else
            {
                score[playerIndex, frame] = accumulator;
            }
        }
    }


    private bool IsStrike(int playerIndex, int throwIndex)
    {
        return (throwIndex % 2 == 0 || throwIndex == 19) && playerThrows[playerIndex, throwIndex] == 10;
    }

    private bool IsSpare(int playerIndex, int throwIndex)
    {
        return throwIndex % 2 == 1 && playerThrows[playerIndex, throwIndex] + playerThrows[playerIndex, throwIndex - 1] == 10;
    }
}