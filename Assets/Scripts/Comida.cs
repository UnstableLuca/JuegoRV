using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation; 
public class Comida : MonoBehaviour
{
 [Header("Configuración de Entornos")]
    public GameObject entornoVirtualRV;      
    [Header("Componentes de RA")]
    public ARPlaneManager planeManagerRA;
    public ARRaycastManager raycastManagerRA;
        
    [Header("Elementos de Juego")]
    public GameObject prefabCuencoComida;    
    public Transform puntoSueloRA;          
    
    [Header("Referencias del Dinosaurio")]
    public GameObject dinosaurioActual;     
    private NavMeshAgent navMeshDino;
    private Animator animDino;

    private bool esperandoCaricia = false;


    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.dinosaurioVivoEnEscena != null)
        {
            dinosaurioActual = GameManager.Instance.dinosaurioVivoEnEscena;
            
            navMeshDino = dinosaurioActual.GetComponent<UnityEngine.AI.NavMeshAgent>();
            animDino = dinosaurioActual.GetComponent<Animator>();
        }
        else
        {
            Debug.LogError("¡No se encontró ningún dinosaurio vivo registrado en el GameManager!");
        }
    }

    public void IniciarSecuenciaAlimentacion()
    {
        entornoVirtualRV.SetActive(false);

        planeManagerRA.enabled = true;
        raycastManagerRA.enabled = true;

        Instantiate(prefabCuencoComida, puntoSueloRA.position, Quaternion.identity);

        navMeshDino.SetDestination(puntoSueloRA.position);
        animDino.SetTrigger("Caminar"); 
    }

    public void FinalizarComida()
    {
     
        navMeshDino.SetDestination(Camera.main.transform.position + Camera.main.transform.forward * 1.5f);
        esperandoCaricia = true;
    }

    public void RegistrarCaricia()
    {
        if (esperandoCaricia)
        {
            esperandoCaricia = false;
          //  animDino.SetTrigger("Felicidad"); // Animación de alegría
            
            Invoke("RegresarAlMundoVirtual", 3.0f);
        }
    }

    void RegresarAlMundoVirtual()
    {
        // Enciende la detección del suelo real
        planeManagerRA.enabled = false;
        raycastManagerRA.enabled = false;
        entornoVirtualRV.SetActive(true);
    }
}
