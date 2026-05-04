using UnityEngine;
using UnityEngine.UI;
public class StatsDinosaurios : MonoBehaviour
{
    [Header("Hambre")]
    public float hunger = 100f;
    public float hungerDecreaseSpeed = 1f;
    public Slider hungerBar;
    public Image hungerFill;

    [Header("Diversión / Cuidados")]
    public float care = 100f;
    public float careDecreaseSpeed = 0.5f;
    public Slider careBar;
    public Image careFill;

    [Header("Vida")]
    public float health = 100f;
    public Slider healthBar;
    public Image healthFill;

    [Header("Limpieza")]
    public float cleanliness = 100f;
    public float cleanlinessDecreaseSpeed = 0.3f;
    public Slider cleanlinessBar;
    public Image cleanlinessFill;

    [Header("General")]
    public float maxValue = 100f;

    void Update()
    {
        DecreaseStatsOverTime();
        CheckHealthDamage();
        UpdateAllBars();
    }

    void DecreaseStatsOverTime()
    {
        hunger -= hungerDecreaseSpeed * Time.deltaTime;
        care -= careDecreaseSpeed * Time.deltaTime;
        cleanliness -= cleanlinessDecreaseSpeed * Time.deltaTime;

        hunger = Mathf.Clamp(hunger, 0f, maxValue);
        care = Mathf.Clamp(care, 0f, maxValue);
        cleanliness = Mathf.Clamp(cleanliness, 0f, maxValue);
    }

    void CheckHealthDamage()
    {
        if (hunger <= 0f || care <= 0f || cleanliness <= 0f)
        {
            health -= 2f * Time.deltaTime;
        }

        health = Mathf.Clamp(health, 0f, maxValue);
    }

    void UpdateAllBars()
    {
        UpdateBar(hungerBar, hungerFill, hunger);
        UpdateBar(careBar, careFill, care);
        UpdateBar(healthBar, healthFill, health);
        UpdateBar(cleanlinessBar, cleanlinessFill, cleanliness);
    }

    void UpdateBar(Slider bar, Image fill, float value)
    {
        if (bar != null)
        {
            bar.maxValue = maxValue;
            bar.value = value;
        }

        if (fill != null)
        {
            float percentage = value / maxValue;

            if (percentage < 0.3f)
                fill.color = Color.red;
            else if (percentage < 0.7f)
                fill.color = Color.yellow;
            else
                fill.color = Color.green;
        }
    }

    public void Feed(float amount)
    {
        hunger += amount;
        hunger = Mathf.Clamp(hunger, 0f, maxValue);
    }

    public void Care(float amount)
    {
        care += amount;
        care = Mathf.Clamp(care, 0f, maxValue);
    }

    public void Heal(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0f, maxValue);
    }

    public void Clean(float amount)
    {
        cleanliness += amount;
        cleanliness = Mathf.Clamp(cleanliness, 0f, maxValue);
    }
}
