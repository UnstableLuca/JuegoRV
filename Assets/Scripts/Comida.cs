using UnityEngine;

public class Comida : MonoBehaviour
{
    public Transform player;     // asigna aquí el jugador en el Inspector
    public float speed = 3f;
    public float stopDistance = 2f;

    private bool shouldFollow = false;

    void Update()
    {
        // Pulsar tecla 2 → activar seguimiento
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            shouldFollow = !shouldFollow;
        }

        // Movimiento hacia el jugador
        if (shouldFollow && player != null)
        {
            MoveTowardsPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f; // evita que suba/baje

        float distance = direction.magnitude;

        if (distance > stopDistance)
        {
            transform.position += direction.normalized * speed * Time.deltaTime;

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    5f * Time.deltaTime
                );
            }
        }
    }
}
