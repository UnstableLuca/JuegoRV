using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class ComidaController : MonoBehaviour
{
public GameObject foodObject;
    public Transform foodHoldPoint;
    public Comida dinosaur;

    private InputDevice rightHandDevice;
    private bool previousButtonState = false;
    private bool foodVisible = false;

    void Start()
    {
        if (foodObject != null)
        {
            foodObject.SetActive(false);
        }

        GetRightHandDevice();
    }

    void Update()
    {
        // ===== INPUT TECLADO (DEBUG) =====
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ToggleFood();
        }

        // ===== INPUT VR =====
        if (!rightHandDevice.isValid)
        {
            GetRightHandDevice();
        }

        bool primaryButtonPressed;
        if (rightHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonPressed))
        {
            if (primaryButtonPressed && !previousButtonState)
            {
                ToggleFood();
            }

            previousButtonState = primaryButtonPressed;
        }
    }

    void GetRightHandDevice()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

        if (devices.Count > 0)
        {
            rightHandDevice = devices[0];
        }
    }

    void ToggleFood()
    {
        foodVisible = !foodVisible;

        if (foodObject != null)
        {
            if (foodHoldPoint != null)
            {
                foodObject.transform.SetParent(foodHoldPoint);
                foodObject.transform.localPosition = Vector3.zero;
                foodObject.transform.localRotation = Quaternion.identity;
            }

            foodObject.SetActive(foodVisible);
        }

        if (dinosaur != null)
        {
            dinosaur.SetFollow(foodVisible);
        }
    }
}
