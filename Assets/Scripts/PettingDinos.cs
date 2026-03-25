using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static UnityEngine.Rendering.GPUSort;

public class PettingDinos : MonoBehaviour
{
    [SerializeField] private XRSimpleInteractable interactor;

    public float maxVibrationAmp = 1.0f;
    public float maxVibrationDuration = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void OnEnable()
    {
        interactor.hoverEntered.AddListener(OnHoveredEntered);
    }
    private void OnDisable()
    {
        interactor.hoverExited.RemoveListener(OnHoveredEntered);  
    }
    private void OnHoveredEntered(HoverEnterEventArgs args);
    {
        throw NotImplementedException();
    }
// Update is called once per frame
void Update()
    {

    }
}
