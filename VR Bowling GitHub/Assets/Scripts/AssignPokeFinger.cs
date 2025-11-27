using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AssignPokeFinger : MonoBehaviour
{

    [SerializeField] private Transform pokePosition;

    private XRPokeInteractor xrPokeInteractor;

    private void Start()
    {
        xrPokeInteractor = transform.parent.parent.GetComponentInChildren<XRPokeInteractor>();

        xrPokeInteractor.attachTransform = pokePosition;

    }
}
