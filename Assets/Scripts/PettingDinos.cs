using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PettingDinos : MonoBehaviour
{
    private enum HandSide
    {
        Unknown,
        Left,
        Right
    }

    [Header("Animal")]
    [SerializeField] private XRSimpleInteractable interactable;

    [Header("Assign your hand interactors")]
    [SerializeField] private XRDirectInteractor leftHandInteractor;
    [SerializeField] private XRDirectInteractor rightHandInteractor;

    [Header("Petting detection")]
    [SerializeField] private float minPetSpeed = 0.02f;
    [SerializeField] private float maxPetSpeed = 0.25f;
    [SerializeField] private float smoothing = 12f;

    [Header("Haptics")]
    [SerializeField] private float pulseDuration = 0.02f;
    [SerializeField] private float pulseInterval = 0.04f;
    [SerializeField]
    private AnimationCurve amplitudeCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Debug")]
    [SerializeField] private bool logHandEvents = false;

    private class HoverState
    {
        public XRDirectInteractor interactor;
        public XRBaseControllerInteractor controllerInteractor;
        public HandSide handSide;
        public Vector3 lastPosition;
        public float smoothedSpeed;
        public float timer;
    }

    private readonly Dictionary<IXRHoverInteractor, HoverState> activeHoverStates = new();

    private void Reset()
    {
        interactable = GetComponent<XRSimpleInteractable>();
    }

    private void Awake()
    {
        if (interactable == null)
            interactable = GetComponent<XRSimpleInteractable>();
    }

    private void OnEnable()
    {
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
    }

    private void OnDisable()
    {
        interactable.hoverEntered.RemoveListener(OnHoverEntered);
        interactable.hoverExited.RemoveListener(OnHoverExited);
        activeHoverStates.Clear();
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f || activeHoverStates.Count == 0)
            return;

        var keys = new List<IXRHoverInteractor>(activeHoverStates.Keys);

        foreach (IXRHoverInteractor key in keys)
        {
            if (!activeHoverStates.TryGetValue(key, out HoverState state))
                continue;

            if (state.interactor == null || state.controllerInteractor == null)
                continue;

            Vector3 currentPosition = state.interactor.transform.position;
            float rawSpeed = (currentPosition - state.lastPosition).magnitude / dt;
            state.lastPosition = currentPosition;

            state.smoothedSpeed = Mathf.Lerp(
                state.smoothedSpeed,
                rawSpeed,
                1f - Mathf.Exp(-smoothing * dt)
            );

            state.timer += dt;
            if (state.timer < pulseInterval)
                continue;

            state.timer = 0f;

            float normalized = Mathf.InverseLerp(minPetSpeed, maxPetSpeed, state.smoothedSpeed);
            float amplitude = amplitudeCurve.Evaluate(Mathf.Clamp01(normalized));

            if (amplitude > 0.001f)
            {
                state.controllerInteractor.SendHapticImpulse(amplitude, pulseDuration);
            }
        }
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        Debug.Log("HOVER ENTERED RAW");

        Debug.Log($"Interactor type: {args.interactorObject.GetType().FullName}");
        IXRHoverInteractor hoverInteractor = args.interactorObject;
        XRDirectInteractor directInteractor = ExtractDirectInteractor(hoverInteractor);
        Debug.Log($"Direct interactor: {directInteractor}");

        XRBaseControllerInteractor controllerInteractor = directInteractor as XRBaseControllerInteractor;
        Debug.Log($"Controller interactor: {controllerInteractor}");

        if (directInteractor == null)
            return;

        if (controllerInteractor == null)
            return;

        HandSide handSide = GetHandSide(directInteractor);
        if (handSide == HandSide.Unknown)
            return;

        activeHoverStates[hoverInteractor] = new HoverState
        {
            interactor = directInteractor,
            controllerInteractor = controllerInteractor,
            handSide = handSide,
            lastPosition = directInteractor.transform.position,
            smoothedSpeed = 0f,
            timer = 0f
        };

        if (logHandEvents)
            Debug.Log($"{name}: {handSide} hand started petting.");
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        IXRHoverInteractor hoverInteractor = args.interactorObject;

        if (activeHoverStates.TryGetValue(hoverInteractor, out HoverState state) && logHandEvents)
            Debug.Log($"{name}: {state.handSide} hand stopped petting.");

        activeHoverStates.Remove(hoverInteractor);
    }

    private XRDirectInteractor ExtractDirectInteractor(IXRHoverInteractor hoverInteractor)
    {
        if (hoverInteractor is XRDirectInteractor direct)
            return direct;

        if (hoverInteractor is Component component)
            return component.GetComponent<XRDirectInteractor>();

        return null;
    }

    private HandSide GetHandSide(XRDirectInteractor interactorToCheck)
    {
        if (interactorToCheck == null)
            return HandSide.Unknown;

        if (leftHandInteractor != null && interactorToCheck == leftHandInteractor)
            return HandSide.Left;

        if (rightHandInteractor != null && interactorToCheck == rightHandInteractor)
            return HandSide.Right;

        return HandSide.Unknown;
    }
}