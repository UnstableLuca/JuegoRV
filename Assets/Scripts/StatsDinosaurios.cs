using UnityEngine;
using UnityEngine.UI;
using System;

public class StatsDinosaurios : MonoBehaviour
{
    [Header("Identidad")]
    public string dinoId = "Dino_01";

    [Header("Hambre")]
    public float hunger = 100f;
    public float hungerDecreaseSpeed = 0.001157f; // Aproximadamente 24h
    public Slider hungerBar;
    public Image hungerFill;

    [Header("Diversión / Cuidados")]
    public float care = 100f;
    public float careDecreaseSpeed = 0.000772f; // Aproximadamente 36h
    public Slider careBar;
    public Image careFill;

    [Header("Vida")]
    public float health = 100f;
    public float healthDamageSpeed = 2f;
    public Slider healthBar;
    public Image healthFill;

    [Header("Limpieza")]
    public float cleanliness = 100f;
    public float cleanlinessDecreaseSpeed = 0.000579f; // Aproximadamente 48h
    public Slider cleanlinessBar;
    public Image cleanlinessFill;

    [Header("General")]
    public float maxValue = 100f;

    [Header("Guardado")]
    public float saveInterval = 5f;

    [Header("Offline Simulation")]
    public float maxOfflineHours = 240f;

    private float saveTimer;

    private string Prefix
    {
        get
        {
            if (string.IsNullOrEmpty(dinoId))
                return gameObject.name;

            return dinoId;
        }
    }

    private string HungerKey => Prefix + "_Hunger";
    private string CareKey => Prefix + "_Care";
    private string HealthKey => Prefix + "_Health";
    private string CleanlinessKey => Prefix + "_Cleanliness";
    private string LastSaveTimeKey => Prefix + "_LastSaveTime";

    void Start()
    {
        LoadStats();
        SimulateOfflineProgress();
        UpdateAllBars();
    }

    void Update()
    {
        DecreaseStatsOverTime();
        CheckHealthDamage(Time.deltaTime);
        UpdateAllBars();

        saveTimer += Time.deltaTime;

        if (saveTimer >= saveInterval)
        {
            saveTimer = 0f;
            SaveStats();
        }
    }

    void DecreaseStatsOverTime()
    {
        hunger -= hungerDecreaseSpeed * Time.deltaTime;
        care -= careDecreaseSpeed * Time.deltaTime;
        cleanliness -= cleanlinessDecreaseSpeed * Time.deltaTime;

        ClampAllStats();
    }

    void CheckHealthDamage(float deltaTime)
    {
        if (hunger <= 0f || care <= 0f || cleanliness <= 0f)
        {
            health -= healthDamageSpeed * deltaTime;
        }

        health = Mathf.Clamp(health, 0f, maxValue);
    }

    void SimulateOfflineProgress()
    {
        if (!PlayerPrefs.HasKey(LastSaveTimeKey))
        {
            SaveStats();
            return;
        }

        string lastSaveTimeString = PlayerPrefs.GetString(LastSaveTimeKey);

        if (!long.TryParse(lastSaveTimeString, out long lastSaveBinary))
        {
            SaveStats();
            return;
        }

        DateTime lastSaveTime = DateTime.FromBinary(lastSaveBinary);
        DateTime currentTime = DateTime.Now;

        double offlineSeconds = (currentTime - lastSaveTime).TotalSeconds;

        if (offlineSeconds <= 0)
        {
            SaveStats();
            return;
        }

        float maxOfflineSeconds = maxOfflineHours * 60f * 60f;
        float simulatedSeconds = Mathf.Min((float)offlineSeconds, maxOfflineSeconds);

        ApplyOfflineStatLoss(simulatedSeconds);

        SaveStats();
    }

    void ApplyOfflineStatLoss(float simulatedSeconds)
    {
        float originalHunger = hunger;
        float originalCare = care;
        float originalCleanliness = cleanliness;

        hunger -= hungerDecreaseSpeed * simulatedSeconds;
        care -= careDecreaseSpeed * simulatedSeconds;
        cleanliness -= cleanlinessDecreaseSpeed * simulatedSeconds;

        hunger = Mathf.Clamp(hunger, 0f, maxValue);
        care = Mathf.Clamp(care, 0f, maxValue);
        cleanliness = Mathf.Clamp(cleanliness, 0f, maxValue);

        float hungerZeroTime = GetTimeUntilZero(originalHunger, hungerDecreaseSpeed);
        float careZeroTime = GetTimeUntilZero(originalCare, careDecreaseSpeed);
        float cleanlinessZeroTime = GetTimeUntilZero(originalCleanliness, cleanlinessDecreaseSpeed);

        float firstStatHitsZeroTime = Mathf.Min(hungerZeroTime, careZeroTime, cleanlinessZeroTime);

        if (firstStatHitsZeroTime < simulatedSeconds)
        {
            float healthDamageDuration = simulatedSeconds - firstStatHitsZeroTime;
            health -= healthDamageSpeed * healthDamageDuration;
        }

        ClampAllStats();
    }

    float GetTimeUntilZero(float currentValue, float decreaseSpeed)
    {
        if (decreaseSpeed <= 0f)
            return float.MaxValue;

        return currentValue / decreaseSpeed;
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

    void ClampAllStats()
    {
        hunger = Mathf.Clamp(hunger, 0f, maxValue);
        care = Mathf.Clamp(care, 0f, maxValue);
        cleanliness = Mathf.Clamp(cleanliness, 0f, maxValue);
        health = Mathf.Clamp(health, 0f, maxValue);
    }

    public void SaveStats()
    {
        PlayerPrefs.SetFloat(HungerKey, hunger);
        PlayerPrefs.SetFloat(CareKey, care);
        PlayerPrefs.SetFloat(HealthKey, health);
        PlayerPrefs.SetFloat(CleanlinessKey, cleanliness);

        PlayerPrefs.SetString(LastSaveTimeKey, DateTime.Now.ToBinary().ToString());

        PlayerPrefs.Save();
    }

    void LoadStats()
    {
        hunger = PlayerPrefs.GetFloat(HungerKey, hunger);
        care = PlayerPrefs.GetFloat(CareKey, care);
        health = PlayerPrefs.GetFloat(HealthKey, health);
        cleanliness = PlayerPrefs.GetFloat(CleanlinessKey, cleanliness);

        ClampAllStats();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveStats();
        }
    }

    void OnApplicationQuit()
    {
        SaveStats();
    }

    public void Feed(float amount, bool saveImmediately = true)
    {
        hunger += amount;
        hunger = Mathf.Clamp(hunger, 0f, maxValue);

        if (saveImmediately)
            SaveStats();

        UpdateAllBars();
    }

    public void Care(float amount, bool saveImmediately = true)
    {
        care += amount;
        care = Mathf.Clamp(care, 0f, maxValue);

        if (saveImmediately)
            SaveStats();

        UpdateAllBars();
    }

    public void Heal(float amount, bool saveImmediately = true)
    {
        health += amount;
        health = Mathf.Clamp(health, 0f, maxValue);

        if (saveImmediately)
            SaveStats();

        UpdateAllBars();
    }

    public void Clean(float amount, bool saveImmediately = true)
    {
        cleanliness += amount;
        cleanliness = Mathf.Clamp(cleanliness, 0f, maxValue);

        if (saveImmediately)
            SaveStats();

        UpdateAllBars();
    }

    public void ResetStats()
    {
        hunger = maxValue;
        care = maxValue;
        health = maxValue;
        cleanliness = maxValue;

        SaveStats();
        UpdateAllBars();
    }
}