using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplayHandler : MonoBehaviour
{
    [SerializeField] private Transform playerEntriesParent;
    [SerializeField] private Transform playerEntryPrefab;

    private Transform[] playerEntries;
    private ScorePlayerEntry_Variables[] scoreVariables;

    public void InitializeNewGame(String[] playerNames)
    {
        if (playerEntries != null)
        {
            foreach (var entry in playerEntries)
            {
                Destroy(entry);
            }
        }

        playerEntries = new Transform[playerNames.Length];
        scoreVariables = new ScorePlayerEntry_Variables[playerNames.Length];

        for (int i = 0; i < playerNames.Length; i++)
        {
            Transform newEntry = Instantiate(playerEntryPrefab, playerEntriesParent);
            newEntry.name = "PlayerEntry" + i;
            scoreVariables[i] = newEntry.GetComponent<ScorePlayerEntry_Variables>();
            scoreVariables[i].nameText.text = playerNames[i];
            newEntry.GetComponent<RectTransform>().anchoredPosition = new Vector2(1280, -180 * i);
            playerEntries[i] = newEntry;
        }

        if (playerEntries.Length > 0)
        {
            DisplayCurrentPlayer(0);
        }
    }

    public void UpdateScoreDisplayThrow(int playerIndex, int throwIndex, int value, bool isStrike = false, bool isSpare = false)
    {
        if (isStrike)
        {
            if (throwIndex < 18)
            {
                scoreVariables[playerIndex].throwTexts[throwIndex + 1].text = "X";
            }
            else
            {
                scoreVariables[playerIndex].throwTexts[throwIndex].text = "X";
            }
        }
        else if (isSpare)
        {
            scoreVariables[playerIndex].throwTexts[throwIndex].text = "/";
        }
        else
        {
            scoreVariables[playerIndex].throwTexts[throwIndex].text = value.ToString();
        }
    }

    public void UpdateScoreTotals(int playerIndex, int untilFrame, int[,] score)
    {
        // Updates all completed frames
        for (int i = 0; i < untilFrame; i++)
        {
            scoreVariables[playerIndex].scoreTexts[i].text = score[playerIndex, i].ToString();
        }
    }

    public void DisplayWinner(int[,] score)
    {
        for (int i = 0; i < playerEntries.Length; i++)
        {
            playerEntries[i].Find("Player").GetComponent<Image>().color = new Color(0, 66 / 255f, 255 / 255f);
        }

        List<int> winners = new List<int>();

        int curMax = -1;
        for (int i = 0; i < score.GetLength(0); i++)
        {
            if (score[i, 10] > curMax)
            {
                curMax = score[i, 10];
                winners.Clear();
                winners.Add(i);
            }
            else if (score[i, 10] == curMax)
            {
                winners.Add(i);
            }
        }


        foreach (var winner in winners)
        {
            Image[] images = playerEntries[winner].GetComponentsInChildren<Image>();
            foreach (var image in images)
            {
                image.color = new Color(217 / 255f, 193 / 255f, 6 / 255f);
            }
        }
    }

    public void DisplayCurrentPlayer(int playerIndex)
    {
        for (int i = 0; i < playerEntries.Length; i++)
        {
            if (i == playerIndex)
            {
                playerEntries[i].Find("Player").GetComponent<Image>().color = new Color(36 / 255f, 217 / 255f, 71 / 255f);
            }
            else
            {
                playerEntries[i].Find("Player").GetComponent<Image>().color = new Color(0, 66 / 255f, 255 / 255f);
            }
        }
    }
}