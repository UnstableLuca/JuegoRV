using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SetPokeToFingerAttachPoint : MonoBehaviour
{
    public Transform PokeAttachPoint;

    private XRPokeInteractor _xrPokeInteractor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //_xrPokeInteractor = transform.parent.parent.GetComponentInChildren<XRPokeInteractor>();
        _xrPokeInteractor = GetComponentInParent<XROrigin>().GetComponentInChildren<XRPokeInteractor>(true);
        SetPokeAttachPoint();
    }

    void SetPokeAttachPoint()
    {
        if (_xrPokeInteractor != null && PokeAttachPoint != null)
        {
            _xrPokeInteractor.attachTransform = PokeAttachPoint;
        }
    }
}
