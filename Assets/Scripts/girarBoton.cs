using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(HingeJoint))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(XRGrabInteractable))]
public class girarBoton : MonoBehaviour
{[Header("Referencias del Tocadiscos")]
    [Header("Referencias del Tocadiscos")]
    [SerializeField] private Animator platterAnimator; 
    [SerializeField] private string animationBoolName = "girando";

    [Header("Configuración del Dial")]
    [Range(0f, 1f)] 
    [SerializeField] private float activationPercent = 0.8f; 
    private HingeJoint hinge;
    private AudioSource audioSource;
    private XRGrabInteractable grabInteractable;
    
    private bool turntableIsOn = false;
    private float maxAngle;

    void Start()
    {
        hinge = GetComponent<HingeJoint>();
        audioSource = GetComponent<AudioSource>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        audioSource.playOnAwake = false;
        audioSource.Stop();

        maxAngle = hinge.limits.max;
    }

    void Update()
    {
        if (!grabInteractable.isSelected) return;

        float currentAngle = hinge.angle;
        float currentPercent = Mathf.Clamp01(currentAngle / maxAngle);

        CheckDialState(currentPercent);
    }

    private void CheckDialState(float currentPercent)
    {
        if (currentPercent >= activationPercent && !turntableIsOn)
        {
            turntableIsOn = true;
            audioSource.Play();
            
            if (platterAnimator != null) 
                platterAnimator.SetBool(animationBoolName, true);
        }
        else if (currentPercent < activationPercent && turntableIsOn)
        {
            turntableIsOn = false;
            audioSource.Stop();
            
            if (platterAnimator != null) 
                platterAnimator.SetBool(animationBoolName, false);
        }
    }
}
