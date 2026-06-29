using UnityEngine;

public class LocomotionModeManager : MonoBehaviour
{
    public enum LocomotionMode
    {
        Joystick,
        Teleport
    }

    [Header("Initial Mode")]
    public LocomotionMode initialMode = LocomotionMode.Joystick;

    [Header("Joystick Mode")]
    public Behaviour[] joystickScripts;
    public GameObject[] joystickObjects;

    [Header("Teleport Mode")]
    public Behaviour[] teleportScripts;
    public GameObject[] teleportObjects;

    [Header("Debug")]
    [SerializeField] private LocomotionMode currentMode;

    void Start()
    {
        SetMode(initialMode);
    }

    public void CycleMode()
    {
        switch (currentMode)
        {
            case LocomotionMode.Joystick:
                SetMode(LocomotionMode.Teleport);
                break;

            case LocomotionMode.Teleport:
                SetMode(LocomotionMode.Joystick);
                break;
        }
    }

    public void SetJoystickMode()
    {
        SetMode(LocomotionMode.Joystick);
    }

    public void SetTeleportMode()
    {
        SetMode(LocomotionMode.Teleport);
    }

    public void SetMode(LocomotionMode mode)
    {
        currentMode = mode;

        bool joystickActive = currentMode == LocomotionMode.Joystick;
        bool teleportActive = currentMode == LocomotionMode.Teleport;

        SetBehavioursEnabled(joystickScripts, joystickActive);
        SetObjectsActive(joystickObjects, joystickActive);

        SetBehavioursEnabled(teleportScripts, teleportActive);
        SetObjectsActive(teleportObjects, teleportActive);

        Debug.Log("Current locomotion mode: " + currentMode);
    }

    void SetBehavioursEnabled(Behaviour[] scripts, bool enabled)
    {
        foreach (Behaviour script in scripts)
        {
            if (script != null)
            {
                script.enabled = enabled;
            }
        }
    }

    void SetObjectsActive(GameObject[] objects, bool active)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }
}