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

        // Nos suscribimos a los eventos de cuando una mano intenta agarrar el objeto
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
    }

    void Update()
    {
        // Si hay menos de 2 manos sosteniéndolo, el objeto no se mueve
        if (grabInteractable.interactorsSelecting.Count < 2)
        {
            grabInteractable.trackPosition = false;
            grabInteractable.trackRotation = false;

            // Si solo queda una mano agarrándolo, lo obligamos a soltarse por completo
            if (grabInteractable.interactorsSelecting.Count == 1)
            {
                var interactor = grabInteractable.interactorsSelecting[0];
                grabInteractable.interactionManager.SelectExit(interactor, grabInteractable);
            }
        }
        else
        {
            // Si están las dos manos, el objeto se activa y se puede mover
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = true;
            rb.isKinematic = false;
        }
    }

    // Este método redirige dinámicamente cada mano a su punto de agarre correcto
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Comprobamos el nombre o etiquetas del interactor (mano) para saber cuál es
        string interactorName = args.interactorObject.transform.name.ToLower();

        if (interactorName.Contains("left"))
        {
            // Si es la mano izquierda, le asignamos su punto
            grabInteractable.attachTransform = leftHandAttachPoint;
        }
        else if (interactorName.Contains("right"))
        {
            // Si es la mano derecha, le asignamos su punto
            grabInteractable.attachTransform = rightHandAttachPoint;
        }
    }
}
