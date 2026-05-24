using UnityEngine;
using UnityEngine.SceneManagement;
public class GambleDinosaurio : MonoBehaviour
{
    public string Escenaa = "Dinoo";
    public GameObject inputField;
    public GameObject boton1;
    public GameObject boton2;
    
    public GameObject mesa1;
    public GameObject mesa2;

    public GameObject canvas;

    public GameManager gameManager;
    public void HacerGamble()
    {
        gameManager.ElegirDinosaurioAleatorio();
        inputField.SetActive(true);
        boton1.SetActive(false);
        boton2.SetActive(false);
        mesa1.SetActive(false);
        mesa2.SetActive(false);
        canvas.SetActive(false);
        //SceneManager.LoadScene(Escenaa);
    }

    public void Salir()
    {
        Debug.Log("Saliendo...");
        Application.Quit();
    }
}
