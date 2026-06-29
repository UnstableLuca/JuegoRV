using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class agarrarDosManos : MonoBehaviour
{
    [Header("Puntos de Agarre")]
    public Transform leftHandAttachPoint;
    public Transform rightHandAttachPoint;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; 

        grabInteractable.selectEntered.AddListener(OnSelectEntered);
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
    }

    void Update()
    {
        if (grabInteractable.interactorsSelecting.Count < 2)
        {
            grabInteractable.trackPosition = false;
            grabInteractable.trackRotation = false;

            if (grabInteractable.interactorsSelecting.Count == 1)
            {
                var interactor = grabInteractable.interactorsSelecting[0];
                grabInteractable.interactionManager.SelectExit(interactor, grabInteractable);
            }
        }
        else
        {
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = true;
            rb.isKinematic = false;
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        string interactorName = args.interactorObject.transform.name.ToLower();

        if (interactorName.Contains("left"))
        {
            grabInteractable.attachTransform = leftHandAttachPoint;
        }
        else if (interactorName.Contains("right"))
        {
            grabInteractable.attachTransform = rightHandAttachPoint;
        }
    }
}
