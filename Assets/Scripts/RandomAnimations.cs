using UnityEngine;
using System.Collections;

public class RandomAnimations : MonoBehaviour
{public float velocidad = 2.0f;
    public float tiempoCambioMax = 4.0f;
    public float tiempoCambioMin = 2.0f;

    private Animator animator;
    private bool caminando;
    private Vector3 direccionMovimiento;
    private float temporizador;

    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(CambiarEstadoAleatorio());
    }

    void Update()
    {
        if (caminando)
        {
            // Se mueve siempre hacia adelante (su "adelante" local)
            transform.Translate(Vector3.forward * velocidad * Time.deltaTime);
        }
    }   
    IEnumerator CambiarEstadoAleatorio()
    {
        while (true)
        {
            // Seleccionar aleatoriamente si camina o se queda quieto
            caminando = Random.value > 0.5f;

            if (caminando)
            {
                // 1. Elegir una rotación aleatoria en el eje Y (0 a 360 grados)
                float anguloAleatorio = Random.Range(0f, 360f);
                
                // 2. Aplicar la rotación al modelo
                transform.rotation = Quaternion.Euler(0, anguloAleatorio, 0);
                // Elegir una dirección aleatoria (2D/3D horizontal)
               /* direccionMovimiento = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                transform.rotation = Quaternion.Euler(0, anguloAleatorio, 0);*/
            }

            // Actualizar Animator
            animator.SetBool("EstaCaminando", caminando);

            // Esperar un tiempo aleatorio antes del próximo cambio
            float tiempoEspera = Random.Range(tiempoCambioMin, tiempoCambioMax);
            yield return new WaitForSeconds(tiempoEspera);
        }
    }
}
