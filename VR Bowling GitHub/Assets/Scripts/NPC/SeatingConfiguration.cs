using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SeatingConfiguration
{
    public Transform startThrowPos;
    public Transform endThrowPos;
    public Transform walkBackStartPos;
    public Transform walkBackEndPos;
    public Transform[] seatingPos;
}
