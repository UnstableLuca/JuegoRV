using UnityEngine;

public class TurningModeManager : MonoBehaviour
{
    public enum TurnMode
    {
        ContinuousTurn,
        SnapTurn
    }

    [Header("Initial Mode")]
    public TurnMode initialMode = TurnMode.SnapTurn;

    [Header("Continuous Turn Mode")]
    public Behaviour[] continuousTurnScripts;
    public GameObject[] continuousTurnObjects;

    [Header("Snap Turn Mode")]
    public Behaviour[] snapTurnScripts;
    public GameObject[] snapTurnObjects;

    [Header("Debug")]
    [SerializeField] private TurnMode currentMode;

    void Start()
    {
        SetMode(initialMode);
    }

    public void CycleTurnMode()
    {
        switch (currentMode)
        {
            case TurnMode.ContinuousTurn:
                SetMode(TurnMode.SnapTurn);
                break;

            case TurnMode.SnapTurn:
                SetMode(TurnMode.ContinuousTurn);
                break;
        }
    }

    public void SetContinuousTurnMode()
    {
        SetMode(TurnMode.ContinuousTurn);
    }

    public void SetSnapTurnMode()
    {
        SetMode(TurnMode.SnapTurn);
    }

    public void SetMode(TurnMode mode)
    {
        currentMode = mode;

        bool continuousActive = currentMode == TurnMode.ContinuousTurn;
        bool snapActive = currentMode == TurnMode.SnapTurn;

        SetBehavioursEnabled(continuousTurnScripts, continuousActive);
        SetObjectsActive(continuousTurnObjects, continuousActive);

        SetBehavioursEnabled(snapTurnScripts, snapActive);
        SetObjectsActive(snapTurnObjects, snapActive);

        Debug.Log("Current turn mode: " + currentMode);
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