using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SetPokeToFingerAttachPoint : MonoBehaviour
{
    public Transform PokeAttachPoint;

    private XRPokeInteractor _xrPokeInteractor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _xrPokeInteractor = transform.parent.parent.GetComponentInChildren<XRPokeInteractor>();
        SetPokeAttachPoint();
    }

    void SetPokeAttachPoint()
    {
        if (_xrPokeInteractor != null && PokeAttachPoint != null)
        {
            _xrPokeInteractor.attachTransform = PokeAttachPoint;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
