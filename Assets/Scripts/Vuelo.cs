using UnityEngine;
using System.Collections;

public class Vuelo : MonoBehaviour
{
    public Animator animator;
    public float speed = 4.0f;
    public float landingSpeed = 2.0f;
    private bool volando = false;
    private bool aterrizando = false;

    void Start()
    {
        StartCoroutine(RandomStateChanger());
    }

    void Update()
    {
        if (volando)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        else if (aterrizando)
        {
            Vector3 pos = transform.position;
            if (pos.y > 0.05f) 
            {
                transform.position = new Vector3(pos.x, pos.y - landingSpeed * Time.deltaTime, pos.z);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, transform.eulerAngles.y, 0), Time.deltaTime * 2);
            }
            else
            {
                // Aterrizaje completado
                aterrizando = false;
                animator.SetBool("volando", false);
            }
        }
    }

    IEnumerator RandomStateChanger()
    {
        while (true)
        {
            // Decisión aleatoria
            bool proximaAccionVolar = Random.value > 0.5f;

            if (proximaAccionVolar)
            {
                // --- LÓGICA DE DESPEGUE ---
                volando = true;
                aterrizando = false;
                animator.SetBool("volando", true);

                float rotY = Random.Range(0f, 360f);
                float rotX = Random.Range(-20f, -40f);
                transform.rotation = Quaternion.Euler(rotX, rotY, 0);

                // Vuela durante un tiempo aleatorio
                yield return new WaitForSeconds(Random.Range(4f, 8f));
            }
            else
            {
                // --- LÓGICA DE ATERRIZAJE ---
                volando = false;
                aterrizando = true;

                // ESPERAR hasta que el Update termine de bajarlo al suelo
                // Mientras aterrizando sea true, esta línea detiene la corrutina aquí
                while (aterrizando) 
                {
                    yield return null; 
                }

                // Una vez en el suelo, se queda quieto un tiempo extra
                yield return new WaitForSeconds(Random.Range(3f, 6f));
                animator.SetBool("volando", false);
            }
        }
    }
}