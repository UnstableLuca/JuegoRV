using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public string nombreDinosaurio;
    public DinosauriosTipos dinosaurioElegido;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ElegirDinosaurioAleatorio()
    {
        int random = Random.Range(0, 3);

        dinosaurioElegido = (DinosauriosTipos)random;

        Debug.Log("Dinosaurio elegido: " + dinosaurioElegido);
    }
}
