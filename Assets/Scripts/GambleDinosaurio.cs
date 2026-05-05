using UnityEngine;
using UnityEngine.SceneManagement;
public class GambleDinosaurio : MonoBehaviour
{
    public string Escenaa = "Dinoo";

    public void HacerGamble()
    {
        GameManager.Instance.ElegirDinosaurioAleatorio();

        SceneManager.LoadScene(Escenaa);
    }
}
