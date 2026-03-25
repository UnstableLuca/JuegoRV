using UnityEngine;
using UnityEngine.InputSystem;

public class MyActionScript : MonoBehaviour
{
    private InputAction myAction;
    private Vector3 initPosition;
    [Space][SerializeField] private InputActionAsset myActionsAsset;
    void Start()
    {
        myAction = myActionsAsset.FindAction("XRI LeftHand/My Action");
        initPosition = transform.position;
        myAction.performed += OnMyAction;
    }


    // Update is called once per frame
    void Update()
    {
    }

    void OnMyAction(InputAction.CallbackContext context)
    {
        transform.position = initPosition;
    }

}
