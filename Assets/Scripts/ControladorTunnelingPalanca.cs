using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ControladorTunnelingPalanca : MonoBehaviour
{
    [Header("XR Vignette Components")]
  
    public TunnelingVignetteController vignetteController;

    [Header("Invertir Palanca")]
    [Tooltip("Si es true: palanca arriba = pantalla completa. Si es false: palanca arriba = viñeta máxima.")]
    public bool masPalancaMenosViñeta = true;

    public void ModificarIntensidadTunneling(float valorPalanca)
    {
        if (vignetteController == null) return;

        float nuevoTamañoApertura;

        if (masPalancaMenosViñeta)
        {
            nuevoTamañoApertura = Mathf.Lerp(0f, 1f, valorPalanca);
        }
        else
        {
            nuevoTamañoApertura = Mathf.Lerp(1f, 0f, valorPalanca);
        }

        
        vignetteController.defaultParameters.apertureSize = nuevoTamañoApertura;
    }
}