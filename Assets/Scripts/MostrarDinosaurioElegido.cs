using UnityEngine;

public class MostrarDinosaurioElegido : MonoBehaviour
{
 [Header("Dinosaurios de la escena")]
    [SerializeField] private GameObject Triceratops;
    [SerializeField] private GameObject Pterodactilo;
    [SerializeField] private GameObject Anquilosaurio;

    private void Start()
    {
        OcultarTodos();

        if (GameManager.Instance == null)
        {
            Debug.LogError("No existe GameManager en la escena. Asegúrate de venir desde la escena anterior.");
            return;
        }

        switch (GameManager.Instance.dinosaurioElegido)
        {
            case DinosauriosTipos.Triceratops:
                Triceratops.SetActive(true);
                GameManager.Instance.dinosaurioVivoEnEscena = Triceratops;
                break;

            case DinosauriosTipos.Pterodactilo:
                Pterodactilo.SetActive(true);
                GameManager.Instance.dinosaurioVivoEnEscena = Pterodactilo;
                break;

            case DinosauriosTipos.Anquilosaurio:
                Anquilosaurio.SetActive(true);
                GameManager.Instance.dinosaurioVivoEnEscena = Anquilosaurio;
                break;

            default:
                Debug.LogWarning("No se ha reconocido el dinosaurio elegido.");
                break;
        }
        
        Debug.Log("¡Dinosaurio vinculado correctamente al sistema de alimentación: " + GameManager.Instance.dinosaurioVivoEnEscena.name);
    }

    private void OcultarTodos()
    {
        Triceratops.SetActive(false);
        Pterodactilo.SetActive(false);
        Anquilosaurio.SetActive(false);
    }
}