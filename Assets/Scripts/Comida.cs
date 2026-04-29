using UnityEngine;
using UnityEngine.UI;

public class Comida : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;
    public float stopDistance = 2f;

    [Header("Hambre")]
    public float hunger = 50f;
    public float maxHunger = 100f;
    public float hungerDecreaseSpeed = 5f;
    public float hungerIncreaseAmount = 25f;
    public Slider hungerBar;

    private bool shouldFollow = false;

    void Update()
    {
        DecreaseHungerOverTime();

        if (shouldFollow && player != null)
        {
            MoveTowardsPlayer();
        }

        UpdateHungerUI();
    }

    public void SetFollow(bool value)
    {
        shouldFollow = value;
    }

    public void Eat()
    {
        hunger += hungerIncreaseAmount;
        hunger = Mathf.Clamp(hunger, 0f, maxHunger);

        shouldFollow = false;
    }

    void DecreaseHungerOverTime()
    {
        hunger -= hungerDecreaseSpeed * Time.deltaTime;
        hunger = Mathf.Clamp(hunger, 0f, maxHunger);
    }

    void UpdateHungerUI()
    {
        if (hungerBar != null)
        {
            hungerBar.maxValue = maxHunger;
            hungerBar.value = hunger;
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

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
