using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class VRLever : MonoBehaviour
{
    public enum HapticMode
    {
        Smooth,
        Detented
    }

    private HingeJoint hinge;
    private XRGrabInteractable grabInteractable;
    private IXRSelectInteractor currentInteractor;

    [Header("Lever Output")]
    public float leverOutput;
    public float minValue = 0f;
    public float maxValue = 1f;

    [Header("Haptics")]
    public HapticMode hapticMode = HapticMode.Smooth;

    [Header("Smooth Haptics")]
    public float movementThreshold = 0.25f;
    public float smoothAmplitude = 0.08f;
    public float smoothDuration = 0.02f;

    [Header("Detent Haptics")]
    public int detentCount = 5;
    public float detentAmplitude = 0.4f;
    public float detentDuration = 0.04f;

    private float lastAngle;
    private int lastDetent = -1;

    private void Awake()
    {
        hinge = GetComponent<HingeJoint>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void Start()
    {
        lastAngle = hinge.angle;
    }

    private void Update()
    {
        float normalized = Mathf.InverseLerp(
            hinge.limits.min,
            hinge.limits.max,
            hinge.angle
        );

        leverOutput = Mathf.Lerp(minValue, maxValue, normalized);

        if (currentInteractor == null)
            return;

        switch (hapticMode)
        {
            case HapticMode.Smooth:
                SmoothHaptics();
                break;

            case HapticMode.Detented:
                DetentHaptics(normalized);
                break;
        }
    }

    private void SmoothHaptics()
    {
        float angleDelta = Mathf.Abs(hinge.angle - lastAngle);

        if (angleDelta >= movementThreshold)
        {
            SendHaptic(smoothAmplitude, smoothDuration);
            lastAngle = hinge.angle;
        }
    }

    private void DetentHaptics(float normalized)
    {
        int currentDetent = Mathf.RoundToInt(normalized * detentCount);

        if (currentDetent == lastDetent)
            return;

        lastDetent = currentDetent;
        SendHaptic(detentAmplitude, detentDuration);
    }

    private void SendHaptic(float amplitude, float duration)
    {
        if (currentInteractor is XRBaseControllerInteractor inputInteractor)
        {
            inputInteractor.SendHapticImpulse(amplitude, duration);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        currentInteractor = args.interactorObject;

        lastAngle = hinge.angle;

        float normalized = Mathf.InverseLerp(
            hinge.limits.min,
            hinge.limits.max,
            hinge.angle
        );

        lastDetent = Mathf.RoundToInt(normalized * detentCount);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        currentInteractor = null;
    }
}
