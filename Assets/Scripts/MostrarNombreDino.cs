using UnityEngine;
using TMPro;

public class MostrarNombreDino : MonoBehaviour
{
    [SerializeField] private TMP_Text textoNombre;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            textoNombre.text = GameManager.Instance.nombreDinosaurio;
        }
        else
        {
            textoNombre.text = "Dino";
            Debug.LogWarning("No se encontró GameManager en la escena.");
        }
    }
}