using UnityEngine;
using UnityEngine.InputSystem;

public class MyHandController : MonoBehaviour
{
    [SerializeField] InputActionReference actionGrip;
    [SerializeField] InputActionReference actionTrigger;
    private Animator handAnimator;
    void Awake()
    {
        actionTrigger.action.performed += TriggerPress;
        actionGrip.action.performed += GripPress;
        handAnimator = GetComponent<Animator>();
    }
    private void GripPress(InputAction.CallbackContext obj)
    {
        if (handAnimator != null)
            handAnimator.SetFloat("Grip", obj.ReadValue<float>());
    }
    private void TriggerPress(InputAction.CallbackContext obj)
    {
        if (handAnimator != null)
        {
            handAnimator.SetFloat("Trigger", obj.ReadValue<float>());
        }
    }
}
