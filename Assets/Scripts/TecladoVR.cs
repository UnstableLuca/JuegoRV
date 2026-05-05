using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TecladoVR : MonoBehaviour
{
    [Header("Input")]
    public TMP_InputField inputNombre;

    [Header("Configuración")]
    public int maxCaracteres = 12;
    public string nombrePorDefecto = "Dino";
    public string Escenaa = "Dinoo";

    private void Start()
    {
        inputNombre.readOnly = true;
        inputNombre.text = "";
    }

    public void EscribirLetra(string letra)
    {
        if (inputNombre.text.Length >= maxCaracteres)
            return;

        inputNombre.text += letra;
    }

    public void Borrar()
    {
        if (inputNombre.text.Length <= 0)
            return;

        inputNombre.text = inputNombre.text.Substring(0, inputNombre.text.Length - 1);
    }

    public void Limpiar()
    {
        inputNombre.text = "";
    }

    public void ConfirmarNombre()
    {
        string nombreFinal = inputNombre.text.Trim();

        if (string.IsNullOrWhiteSpace(nombreFinal))
        {
            nombreFinal = nombrePorDefecto;
        }

        GameManager.Instance.nombreDinosaurio = nombreFinal;

        SceneManager.LoadScene(Escenaa);
    }
}