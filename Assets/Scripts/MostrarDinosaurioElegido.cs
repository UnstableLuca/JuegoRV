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
                break;

            case DinosauriosTipos.Pterodactilo:
                Pterodactilo.SetActive(true);
                break;

            case DinosauriosTipos.Anquilosaurio:
                Anquilosaurio.SetActive(true);
                break;

            default:
                Debug.LogWarning("No se ha reconocido el dinosaurio elegido.");
                break;
        }
    }

    private void OcultarTodos()
    {
        Triceratops.SetActive(false);
        Pterodactilo.SetActive(false);
        Anquilosaurio.SetActive(false);
    }
}