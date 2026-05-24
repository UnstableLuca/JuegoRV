using UnityEngine;
using UnityEngine.Audio;

public class ControladorAudioPalanca : MonoBehaviour
{
    [Header("Audio Connection")]
    public AudioMixer audioMixer;
    public string exposedParameterName = "VolumenMusica";
    public void ModificarVolumenLogaritmico(float valorPalanca)
    {
        if (audioMixer == null) return;

        if (valorPalanca <= 0.001f)
        {
            audioMixer.SetFloat(exposedParameterName, -80f);
        }
        else
        {
            float dB = Mathf.Log10(valorPalanca) * 20f;
            audioMixer.SetFloat(exposedParameterName, dB);
        }
    }
}