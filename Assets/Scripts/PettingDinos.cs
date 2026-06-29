using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PettingDinos : MonoBehaviour
{
    [Header("Animal")]
    [SerializeField] private XRSimpleInteractable interactable;
    [SerializeField] private StatsDinosaurios stats;

    [Header("Petting detection")]
    [SerializeField] private float minPetSpeed = 0.02f;
    [SerializeField] private float maxPetSpeed = 0.25f;
    [SerializeField] private float smoothing = 12f;

    [Header("Care gain")]
    [SerializeField] private bool increaseCareWhenPetting = true;
    [SerializeField] private float carePerSecond = 8f;

    [Header("Haptics")]
    [SerializeField] private bool useHaptics = true;
    [SerializeField] private float pulseDuration = 0.02f;
    [SerializeField] private float pulseInterval = 0.04f;

    [SerializeField]
    private AnimationCurve amplitudeCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Debug")]
    [SerializeField] private bool logHandEvents = true;

    private class HoverState
    {
        public XRDirectInteractor directInteractor;
        public XRBaseControllerInteractor controllerInteractor;

        public Vector3 lastPosition;
        public float smoothedSpeed;
        public float hapticTimer;
    }

    private readonly Dictionary<IXRHoverInteractor, HoverState> activeHoverStates =
        new Dictionary<IXRHoverInteractor, HoverState>();

    private void Reset()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        stats = GetComponent<StatsDinosaurios>();
    }

    private void Awake()
    {
        if (interactable == null)
            interactable = GetComponent<XRSimpleInteractable>();

        if (stats == null)
            stats = GetComponent<StatsDinosaurios>();
    }

    private void OnEnable()
    {
        if (interactable == null)
        {
            Debug.LogWarning($"{name}: Missing XRSimpleInteractable.");
            return;
        }

        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
    }

    private void OnDisable()
    {
        if (interactable != null)
        {
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
        }

        activeHoverStates.Clear();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        if (dt <= 0f || activeHoverStates.Count == 0)
            return;

        List<IXRHoverInteractor> keys =
            new List<IXRHoverInteractor>(activeHoverStates.Keys);

        foreach (IXRHoverInteractor key in keys)
        {
            if (!activeHoverStates.TryGetValue(key, out HoverState state))
                continue;

            if (state.directInteractor == null)
                continue;

            Vector3 currentPosition = state.directInteractor.transform.position;

            float rawSpeed =
                (currentPosition - state.lastPosition).magnitude / dt;

            state.lastPosition = currentPosition;

            state.smoothedSpeed = Mathf.Lerp(
                state.smoothedSpeed,
                rawSpeed,
                1f - Mathf.Exp(-smoothing * dt)
            );

            float normalizedSpeed = Mathf.InverseLerp(
                minPetSpeed,
                maxPetSpeed,
                state.smoothedSpeed
            );

            float intensity = Mathf.Clamp01(normalizedSpeed);

            if (increaseCareWhenPetting && stats != null && intensity > 0.05f)
            {
                stats.Care(carePerSecond * intensity * dt, false);
            }

            if (!useHaptics)
                continue;

            if (state.controllerInteractor == null)
                continue;

            state.hapticTimer += dt;

            if (state.hapticTimer < pulseInterval)
                continue;

            state.hapticTimer = 0f;

            float amplitude = amplitudeCurve.Evaluate(intensity);

            if (amplitude > 0.001f)
            {
                state.controllerInteractor.SendHapticImpulse(
                    amplitude,
                    pulseDuration
                );
            }
        }
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        IXRHoverInteractor hoverInteractor = args.interactorObject;

        XRDirectInteractor directInteractor =
            ExtractDirectInteractor(hoverInteractor);

        if (directInteractor == null)
        {
            if (logHandEvents)
                Debug.Log($"{name}: Hover entered, but it was not an XRDirectInteractor.");

            return;
        }

        XRBaseControllerInteractor controllerInteractor =
            ExtractControllerInteractor(directInteractor);

        activeHoverStates[hoverInteractor] = new HoverState
        {
            directInteractor = directInteractor,
            controllerInteractor = controllerInteractor,
            lastPosition = directInteractor.transform.position,
            smoothedSpeed = 0f,
            hapticTimer = 0f
        };

        if (logHandEvents)
        {
            Debug.Log($"{name}: Started petting with runtime hand: {directInteractor.name}");
        }
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        IXRHoverInteractor hoverInteractor = args.interactorObject;

        if (activeHoverStates.ContainsKey(hoverInteractor) && logHandEvents)
        {
            Debug.Log($"{name}: Stopped petting.");
        }

        activeHoverStates.Remove(hoverInteractor);
    }

    private XRDirectInteractor ExtractDirectInteractor(IXRHoverInteractor hoverInteractor)
    {
        if (hoverInteractor is XRDirectInteractor directInteractor)
            return directInteractor;

        if (hoverInteractor is Component component)
        {
            XRDirectInteractor foundDirectInteractor =
                component.GetComponent<XRDirectInteractor>();

            if (foundDirectInteractor != null)
                return foundDirectInteractor;

            return component.GetComponentInParent<XRDirectInteractor>();
        }

        return null;
    }
    private XRBaseControllerInteractor ExtractControllerInteractor(XRDirectInteractor directInteractor)
    {
        if (directInteractor == null)
            return null;

        XRBaseControllerInteractor controller =
            directInteractor as XRBaseControllerInteractor;

        if (controller != null)
            return controller;

        controller = directInteractor.GetComponent<XRBaseControllerInteractor>();

        if (controller != null)
            return controller;

        return directInteractor.GetComponentInParent<XRBaseControllerInteractor>();
    }
}