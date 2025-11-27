using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PinRespawn : MonoBehaviour
{
    [SerializeField] private float pinSetTime = 5f;
    [SerializeField] private float pinFlushTime = 5f;
    
    
    
    private float fallingThreshold = 45f;
    
    private List<Transform> pins;
    private GameObject pinFormation;
    
    private Transform pinMover;
    private Transform pinResetter;
    
    private List<Vector3> pinsStartPositions;
    private Vector3 pinResetterStartPosition;
    private Vector3 pinMoverStartPosition;
    
    private void Awake()
    {
        pinMover = transform.Find("PinMover");
        pinResetter = transform.Find("PinResetter");

        pinMoverStartPosition = pinMover.transform.position;
        pinResetterStartPosition = pinResetter.transform.position;
        
    }
    

    private int GetPinIndex(Transform pin)
    {
        string pinName = pin.name;
        int startIndex = pinName.IndexOf("(") + 1;
        int endIndex = pinName.IndexOf(")");

        string substring = pinName.Substring(startIndex, endIndex - startIndex);

        int result = -1;

        int.TryParse(substring, out result);

        if (result == -1)
        {
            Debug.LogError("Couldn't parse pin index!!!");
            return 0;
        }
        else
        {
            return result;
        }
    }
    
    
    public IEnumerator FlushAlley()
    {
        pinMover.transform.DOMove(pinMoverStartPosition, pinFlushTime/4f);
        
        yield return new WaitForSeconds(pinFlushTime/4f);
        
        pinMover.transform.DOMove(new Vector3(pinMover.position.x + 1.5f, pinMover.position.y, pinMover.position.z), pinFlushTime/4f);
        
        yield return new WaitForSeconds(pinFlushTime/4f);
        
        pinMover.transform.DOMove(pinMoverStartPosition, pinFlushTime/4f);
        
        yield return new WaitForSeconds(pinFlushTime/4f);
        
        yield return StartCoroutine(OpenAlley());
    }

    private IEnumerator OpenAlley()
    {
        pinMover.transform.DOMove(pinMoverStartPosition + new Vector3(0,0.55f,0), pinFlushTime/4f);
        
        yield return new WaitForSeconds(pinFlushTime/4f);
    }
    
    private IEnumerator CloseAlley()
    {
        pinMover.transform.DOMove(pinMoverStartPosition, pinFlushTime/4f);
        
        yield return new WaitForSeconds(pinFlushTime/4f);
    }
    

    public IEnumerator PlaceAllPins()
    {
        yield return StartCoroutine(PlacePins(pins));
    }
    
    public IEnumerator PlacePins(List<Transform> pinsToPlace)
    {
        PinsToResetter(pinsToPlace);

        pinResetter.transform.DOMove(new Vector3(pinResetter.position.x, 0, pinResetter.position.z), pinSetTime/2f);
        
        foreach (Transform pin in pinsToPlace)
        {
            int index = GetPinIndex(pin);
            pins[index].transform.DOMove(new Vector3(pinsStartPositions[index].x, 0, pinsStartPositions[index].z), pinSetTime/2f);
        }

        yield return new WaitForSeconds(pinSetTime/2f);
        
        pinResetter.transform.DOMove(pinResetterStartPosition, pinSetTime/2f);

        yield return new WaitForSeconds(pinSetTime/2f);


        foreach (Transform pin in pinsToPlace)
        {
            pins[GetPinIndex(pin)].GetComponent<Rigidbody>().isKinematic = false;
        }
     
    }


    public List<Transform> GetFallenPins()
    {
        List<Transform> fallenPins = new List<Transform>();
        for (int i = 0; i < pins.Count; i++)
        {
            float angle = Vector3.Angle(pins[i].transform.up, Vector3.up);
            if (angle >= fallingThreshold || pins[i].position.y <= -0.05f)
            {
                fallenPins.Add(pins[i]);
            }
        }
        return fallenPins;
    }
    
    public List<Transform> GetStandingPins()
    {
        List<Transform> standingPins = new List<Transform>();
        for (int i = 0; i < pins.Count; i++)
        {
            float angle = Vector3.Angle(pins[i].transform.up, Vector3.up);
            if (angle < fallingThreshold && pins[i].position.y > -0.05f && pins[i].position.y < 0.2f)
            {
                standingPins.Add(pins[i]);
            }
        }

        return standingPins;
    }


    public IEnumerator InitializeNewGame(GameVariables.GameModes gameMode)
    {
        // Remove old pins
        if (pinFormation)
        {
            Destroy(pinFormation);
        }
        
        // Create Pins
        pinFormation = Instantiate(GameVariables.gameVariables.bowlingPinFormation, pinResetterStartPosition, Quaternion.identity);

        // Save Pin Transforms
        pins = new List<Transform>();
        for (int i = 0; i < 10; i++)
        {
            pins.Add(pinFormation.transform.GetChild(i));
        }
        
        // Save Start Positions of Pins for Respawn
        pinsStartPositions = new List<Vector3>();
        
        for (int i = 0; i < pins.Count; i++)
        {
            pinsStartPositions.Add(new Vector3(pins[i].transform.position.x, pins[i].transform.position.y, pins[i].transform.position.z));
        }
        // Place all pins
        yield return StartCoroutine(PlacePins(pins));
        
        // Open gate
        yield return OpenAlley();
    }

    public IEnumerator EndGame()
    {
        yield return FlushAlley();
        
        if (pinFormation)
        {
            Destroy(pinFormation);
        }
        
        
        yield return StartCoroutine(CloseAlley());
    }

    public void PinsToResetter(List<Transform> pinsToPlace)
    {
        // Teleport pins to pin placer
        foreach (Transform pin in pinsToPlace)
        {
            pin.GetComponent<Rigidbody>().isKinematic = true;
            pin.position = pinsStartPositions[GetPinIndex(pin)];
            pin.rotation = Quaternion.identity; 
        }
    }

}
