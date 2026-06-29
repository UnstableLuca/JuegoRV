using UnityEngine;

public class HerramientaDinoVRSensor : MonoBehaviour
{
    private HerramientaDinoVR herramienta;

    void Awake()
    {
        herramienta = GetComponentInParent<HerramientaDinoVR>();
    }

    void OnTriggerStay(Collider other)
    {
        if (herramienta != null)
        {
            herramienta.ProcesarTrigger(other);
        }
    }
}