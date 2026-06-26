using UnityEngine;
using UnityEngine.AI;
public class Comida : MonoBehaviour
{
 [Header("Configuración de Entornos")]
    public GameObject entornoVirtualRV;      
    public GameObject componentePassthroughRA; 
    
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
        navMeshDino = dinosaurioActual.GetComponent<NavMeshAgent>();
        animDino = dinosaurioActual.GetComponent<Animator>();
    }

    public void IniciarSecuenciaAlimentacion()
    {
        entornoVirtualRV.SetActive(false);
        componentePassthroughRA.SetActive(true);

        Instantiate(prefabCuencoComida, puntoSueloRA.position, Quaternion.identity);

        navMeshDino.SetDestination(puntoSueloRA.position);
        animDino.SetTrigger("Caminar"); 
    }

    public void FinalizarComida()
    {
     
        navMeshDino.SetDestination(Camera.main.transform.position + Camera.main.transform.forward * 1.5f);
        esperandoCaricia = true;
    }

    // 3. LLAMAR A ESTO DESDE EL SCRIPT DE LAS MANOS (Hand Tracking) AL DETECTAR EL CONTACTO
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
        componentePassthroughRA.SetActive(false);
        entornoVirtualRV.SetActive(true);
    }
}
