using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ActivarLinterna : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public float amplitude = 0.5f;
    [SerializeField] public float duration = 1f;
    void Start()
    {
        XRBaseInteractable interactable = GetComponent<XRBaseInteractable>();
        interactable.activated.AddListener(ActivateFlashlight);
    }
    void ActivateFlashlight(BaseInteractionEventArgs args)
    {
        if (args.interactorObject is XRBaseControllerInteractor interactor)
        {
            interactor.SendHapticImpulse(0.5f, 1);
        }
    }
}
