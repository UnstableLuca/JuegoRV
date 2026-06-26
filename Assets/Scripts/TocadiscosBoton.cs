using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TocadiscosBoton : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private XRSimpleInteractable buttonInteractable;
    [SerializeField] private Animator tocadiscosAnimator;

    [Header("Configuración Animación")]
    [SerializeField] private string animationBoolName = "girando";

    private AudioSource audioSource;
    private bool isOn = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (buttonInteractable != null)
        {
            buttonInteractable.selectEntered.AddListener(OnButtonPressed);
        }
    }

    void OnDestroy()
    {
        if (buttonInteractable != null)
        {
            buttonInteractable.selectEntered.RemoveListener(OnButtonPressed);
        }
    }

    private void OnButtonPressed(SelectEnterEventArgs args)
    {
        isOn = !isOn;

        if (isOn)
        {
            audioSource.Play();
            
            if (tocadiscosAnimator != null)
                tocadiscosAnimator.SetBool(animationBoolName, true);
        }
        else
        {
            audioSource.Stop();
            
            if (tocadiscosAnimator != null)
                tocadiscosAnimator.SetBool(animationBoolName, false);
        }
    }
}
