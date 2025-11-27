using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameVariables : MonoBehaviour
{
    public static GameVariables gameVariables;
    
    
    // Prefabs
    [SerializeField] public GameObject bowlingPinFormation;
    [SerializeField] public GameObject bowlingBall;

    [SerializeField] public List<Material> normalBallMats;
    
    
    // Sound
    public float minBallSpeedUntilSound;
    
    public float playerPinSoundVolume;
    public float botPinSoundVolume;
    public float playerBallSoundVolume;
    public float botBallSoundVolume;
    public float backgroundMusicSoundVolume;

    public enum GameModes
    {
        Normal,
        Fun
    }

    private void Awake()
    {
        gameVariables = this;
    }
}
