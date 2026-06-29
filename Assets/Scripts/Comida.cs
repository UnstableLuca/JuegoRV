using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation;

public class Comida : MonoBehaviour
{
    private enum EstadoComida
    {
        Inactiva,
        YendoAlCuenco,
        Comiendo,
        YendoAlJugador,
        EsperandoCaricia
    }

    [Header("Configuración de Entornos")]
    public GameObject entornoVirtualRV;

    [Header("Jugador - Gravedad temporal")]
    public bool desactivarGravedadJugadorDuranteComida = true;

    [Tooltip("Si tu jugador/XR Rig usa Rigidbody, arrástralo aquí.")]
    public Rigidbody[] rigidbodiesJugador;

    [Tooltip("Si la gravedad/movimiento viene de scripts, arrástralos aquí. Ejemplo: Continuous Move Provider.")]
    public Behaviour[] scriptsGravedadJugador;

    private bool[] estadosOriginalesUseGravity;
    private bool[] estadosOriginalesScriptsGravedad;
    private bool gravedadJugadorModificada = false;

    [Header("Componentes de RA")]
    public ARPlaneManager planeManagerRA;
    public ARRaycastManager raycastManagerRA;

    [Header("Elementos de Juego")]
    public GameObject prefabCuencoComida;
    public Transform puntoSueloRA;

    [Header("Referencias del Dinosaurio")]
    public GameObject dinosaurioActual;

    private Animator animDino;
    private StatsDinosaurios statsDino;
    private CerebroDino cerebroDino;
    private NavMeshAgent agentDino;

    [Header("Movimiento Manual del Dinosaurio")]
    public float velocidadDino = 1.5f;
    public float velocidadRotacion = 5f;
    public float distanciaParada = 0.15f;

    [Header("Secuencia de Comida")]
    public float tiempoComiendo = 3f;
    public float distanciaFrenteJugador = 1.5f;
    public float cantidadComida = 35f;
    public float cantidadCuidadoPorCaricia = 10f;
    public float tiempoAntesDeVolver = 3f;

    [Header("Volver al mundo virtual")]
    public bool restaurarPosicionVirtualAlFinal = true;
    public float distanciaBuscarNavMesh = 5f;

    [Header("Animaciones")]
    public string parametroCaminar = "EstaCaminando";
    public string parametroComer = "EstaComiendo";
    public string parametroIdle = "EstaIdle";

    public string triggerFelicidad = "Felicidad";
    public bool usarTriggerFelicidad = false;

    private bool moviendoDino = false;
    private bool esperandoCaricia = false;

    private Vector3 destinoDino;
    private GameObject cuencoInstanciado;

    private EstadoComida estadoComida = EstadoComida.Inactiva;

    private Vector3 posicionVirtualInicial;
    private Quaternion rotacionVirtualInicial;
    private bool posicionVirtualGuardada = false;

    void Start()
    {
        BuscarDinosaurio();

        if (planeManagerRA != null)
            planeManagerRA.enabled = false;

        if (raycastManagerRA != null)
            raycastManagerRA.enabled = false;
    }

    void Update()
    {
        MoverDinosaurioManual();
    }

    void BuscarDinosaurio()
    {
        if (GameManager.Instance != null && GameManager.Instance.dinosaurioVivoEnEscena != null)
        {
            dinosaurioActual = GameManager.Instance.dinosaurioVivoEnEscena;
        }

        if (dinosaurioActual == null)
        {
            Debug.LogError("No se encontró ningún dinosaurio vivo registrado en el GameManager.");
            return;
        }

        animDino = dinosaurioActual.GetComponent<Animator>();
        statsDino = dinosaurioActual.GetComponent<StatsDinosaurios>();
        cerebroDino = dinosaurioActual.GetComponent<CerebroDino>();
        agentDino = dinosaurioActual.GetComponent<NavMeshAgent>();
    }

    public void IniciarSecuenciaAlimentacion()
    {
        if (dinosaurioActual == null)
        {
            BuscarDinosaurio();
        }

        if (dinosaurioActual == null || puntoSueloRA == null || prefabCuencoComida == null)
        {
            Debug.LogError("Faltan referencias para iniciar la alimentación.");
            return;
        }

        DesactivarGravedadJugadorTemporalmente();
        GuardarPosicionVirtualInicial();
        DesactivarControlVirtualDelDino();

        if (entornoVirtualRV != null)
            entornoVirtualRV.SetActive(false);

        if (planeManagerRA != null)
            planeManagerRA.enabled = true;

        if (raycastManagerRA != null)
            raycastManagerRA.enabled = true;

        if (cuencoInstanciado != null)
            Destroy(cuencoInstanciado);

        cuencoInstanciado = Instantiate(
            prefabCuencoComida,
            puntoSueloRA.position,
            Quaternion.identity
        );

        estadoComida = EstadoComida.YendoAlCuenco;
        esperandoCaricia = false;

        MoverDinoA(puntoSueloRA.position);
    }

    void GuardarPosicionVirtualInicial()
    {
        if (dinosaurioActual == null)
            return;

        posicionVirtualInicial = dinosaurioActual.transform.position;
        rotacionVirtualInicial = dinosaurioActual.transform.rotation;
        posicionVirtualGuardada = true;
    }

    void DesactivarControlVirtualDelDino()
    {
        if (cerebroDino != null)
            cerebroDino.enabled = false;

        if (agentDino != null)
        {
            if (agentDino.enabled && agentDino.isOnNavMesh)
            {
                agentDino.ResetPath();
            }

            agentDino.enabled = false;
        }
    }

    void ActivarControlVirtualDelDino()
    {
        if (dinosaurioActual == null)
            return;

        if (restaurarPosicionVirtualAlFinal && posicionVirtualGuardada)
        {
            dinosaurioActual.transform.position = posicionVirtualInicial;
            dinosaurioActual.transform.rotation = rotacionVirtualInicial;
        }

        if (agentDino != null)
        {
            NavMeshHit hit;

            bool encontroNavMesh = NavMesh.SamplePosition(
                dinosaurioActual.transform.position,
                out hit,
                distanciaBuscarNavMesh,
                NavMesh.AllAreas
            );

            if (encontroNavMesh)
            {
                dinosaurioActual.transform.position = hit.position;

                agentDino.enabled = true;
                agentDino.Warp(hit.position);
            }
            else
            {
                Debug.LogWarning("No se encontró NavMesh cerca del dinosaurio al volver al mundo virtual. El NavMeshAgent queda desactivado.");
                agentDino.enabled = false;
            }
        }

        if (cerebroDino != null)
            cerebroDino.enabled = true;
    }

    private void MoverDinoA(Vector3 destino)
    {
        if (dinosaurioActual == null)
            return;

        destino.y = dinosaurioActual.transform.position.y;

        destinoDino = destino;
        moviendoDino = true;

        SetBoolSeguro(parametroIdle, false);
        SetBoolSeguro(parametroComer, false);
        SetBoolSeguro(parametroCaminar, true);
    }

    private void MoverDinosaurioManual()
    {
        if (!moviendoDino || dinosaurioActual == null)
            return;

        Vector3 direccion = destinoDino - dinosaurioActual.transform.position;
        direccion.y = 0f;

        if (direccion.magnitude <= distanciaParada)
        {
            moviendoDino = false;
            SetBoolSeguro(parametroCaminar, false);

            AlLlegarDestino();
            return;
        }

        dinosaurioActual.transform.position += direccion.normalized * velocidadDino * Time.deltaTime;

        if (direccion != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);

            dinosaurioActual.transform.rotation = Quaternion.Slerp(
                dinosaurioActual.transform.rotation,
                rotacionObjetivo,
                velocidadRotacion * Time.deltaTime
            );
        }
    }

    void AlLlegarDestino()
    {
        switch (estadoComida)
        {
            case EstadoComida.YendoAlCuenco:
                IniciarComer();
                break;

            case EstadoComida.YendoAlJugador:
                IniciarEsperaCaricia();
                break;
        }
    }

    void IniciarComer()
    {
        estadoComida = EstadoComida.Comiendo;

        SetBoolSeguro(parametroCaminar, false);
        SetBoolSeguro(parametroIdle, false);
        SetBoolSeguro(parametroComer, true);

        if (statsDino != null)
        {
            statsDino.Feed(cantidadComida);
        }

        Invoke(nameof(FinalizarComida), tiempoComiendo);
    }

    public void FinalizarComida()
    {
        if (dinosaurioActual == null || Camera.main == null)
            return;

        CancelInvoke(nameof(FinalizarComida));

        SetBoolSeguro(parametroComer, false);

        Vector3 destino = Camera.main.transform.position + Camera.main.transform.forward * distanciaFrenteJugador;
        destino.y = dinosaurioActual.transform.position.y;

        estadoComida = EstadoComida.YendoAlJugador;
        MoverDinoA(destino);
    }

    void IniciarEsperaCaricia()
    {
        estadoComida = EstadoComida.EsperandoCaricia;
        esperandoCaricia = true;

        SetBoolSeguro(parametroCaminar, false);
        SetBoolSeguro(parametroComer, false);
        SetBoolSeguro(parametroIdle, true);
    }

    public void RegistrarCaricia()
    {
        if (!esperandoCaricia)
            return;

        esperandoCaricia = false;
        estadoComida = EstadoComida.Inactiva;

        if (statsDino != null)
        {
            statsDino.Care(cantidadCuidadoPorCaricia);
        }

        SetBoolSeguro(parametroCaminar, false);
        SetBoolSeguro(parametroComer, false);
        SetBoolSeguro(parametroIdle, false);

        if (usarTriggerFelicidad)
            SetTriggerSeguro(triggerFelicidad);

        Invoke(nameof(RegresarAlMundoVirtual), tiempoAntesDeVolver);
    }

    private void RegresarAlMundoVirtual()
    {
        CancelInvoke();

        moviendoDino = false;
        esperandoCaricia = false;
        estadoComida = EstadoComida.Inactiva;

        if (planeManagerRA != null)
            planeManagerRA.enabled = false;

        if (raycastManagerRA != null)
            raycastManagerRA.enabled = false;

        if (cuencoInstanciado != null)
            Destroy(cuencoInstanciado);

        if (entornoVirtualRV != null)
            entornoVirtualRV.SetActive(true);

        SetBoolSeguro(parametroCaminar, false);
        SetBoolSeguro(parametroComer, false);
        SetBoolSeguro(parametroIdle, false);

        ActivarControlVirtualDelDino();
        RestaurarGravedadJugador();
    }

    private void DesactivarGravedadJugadorTemporalmente()
    {
        if (!desactivarGravedadJugadorDuranteComida)
            return;

        if (gravedadJugadorModificada)
            return;

        gravedadJugadorModificada = true;

        if (rigidbodiesJugador != null)
        {
            estadosOriginalesUseGravity = new bool[rigidbodiesJugador.Length];

            for (int i = 0; i < rigidbodiesJugador.Length; i++)
            {
                Rigidbody rb = rigidbodiesJugador[i];

                if (rb == null)
                    continue;

                estadosOriginalesUseGravity[i] = rb.useGravity;
                rb.useGravity = false;

                ResetVerticalVelocity(rb);
            }
        }

        if (scriptsGravedadJugador != null)
        {
            estadosOriginalesScriptsGravedad = new bool[scriptsGravedadJugador.Length];

            for (int i = 0; i < scriptsGravedadJugador.Length; i++)
            {
                Behaviour script = scriptsGravedadJugador[i];

                if (script == null)
                    continue;

                estadosOriginalesScriptsGravedad[i] = script.enabled;
                script.enabled = false;
            }
        }
    }

    private void RestaurarGravedadJugador()
    {
        if (!gravedadJugadorModificada)
            return;

        gravedadJugadorModificada = false;

        if (rigidbodiesJugador != null && estadosOriginalesUseGravity != null)
        {
            for (int i = 0; i < rigidbodiesJugador.Length; i++)
            {
                Rigidbody rb = rigidbodiesJugador[i];

                if (rb == null)
                    continue;

                if (i < estadosOriginalesUseGravity.Length)
                {
                    rb.useGravity = estadosOriginalesUseGravity[i];
                }
            }
        }

        if (scriptsGravedadJugador != null && estadosOriginalesScriptsGravedad != null)
        {
            for (int i = 0; i < scriptsGravedadJugador.Length; i++)
            {
                Behaviour script = scriptsGravedadJugador[i];

                if (script == null)
                    continue;

                if (i < estadosOriginalesScriptsGravedad.Length)
                {
                    script.enabled = estadosOriginalesScriptsGravedad[i];
                }
            }
        }
    }

    private void ResetVerticalVelocity(Rigidbody rb)
    {
        if (rb == null)
            return;

#if UNITY_6000_0_OR_NEWER
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;
#else
        Vector3 velocity = rb.velocity;
        velocity.y = 0f;
        rb.velocity = velocity;
#endif
    }

    void SetBoolSeguro(string nombreParametro, bool valor)
    {
        if (animDino == null || string.IsNullOrEmpty(nombreParametro))
            return;

        foreach (AnimatorControllerParameter parametro in animDino.parameters)
        {
            if (parametro.name == nombreParametro &&
                parametro.type == AnimatorControllerParameterType.Bool)
            {
                animDino.SetBool(nombreParametro, valor);
                return;
            }
        }
    }

    void SetTriggerSeguro(string nombreParametro)
    {
        if (animDino == null || string.IsNullOrEmpty(nombreParametro))
            return;

        foreach (AnimatorControllerParameter parametro in animDino.parameters)
        {
            if (parametro.name == nombreParametro &&
                parametro.type == AnimatorControllerParameterType.Trigger)
            {
                animDino.SetTrigger(nombreParametro);
                return;
            }
        }
    }

    private void OnDisable()
    {
        RestaurarGravedadJugador();
    }

    private void OnApplicationQuit()
    {
        RestaurarGravedadJugador();
    }
}