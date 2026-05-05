using UnityEngine;
using System.Collections;
[RequireComponent(typeof(AudioSource))]
public class Rwar : MonoBehaviour
{
    private AudioSource audioSource;
    public float intervalo = 5f; // Tiempo en segundos entre sonidos
    public AudioClip sonido;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(ReproducirRwar());
    }

    IEnumerator ReproducirRwar()
    {
        while (true) 
        {
            audioSource.PlayOneShot(sonido);
            yield return new WaitForSeconds(intervalo); 
        }
    }
}
