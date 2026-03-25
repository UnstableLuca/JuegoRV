using UnityEngine;
using UnityEngine.UI;

public class UIBehaviour : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Slider slider;
    [SerializeField] private Light light;

    void Start()
    {
        slider.onValueChanged.AddListener((v) => light.intensity = v);
    }
}
