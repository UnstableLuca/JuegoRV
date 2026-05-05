using UnityEngine;
using TMPro;

public class MostrarNombreDinosaurio : MonoBehaviour
{
    public TextMeshProUGUI textoNombre;

    private void Start()
    {
        textoNombre.text = GameManager.Instance.nombreDinosaurio;
    }
}
