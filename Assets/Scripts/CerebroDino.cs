using UnityEngine;
using UnityEngine.AI;

public class CerebroDino : MonoBehaviour
{
    public enum Necesidad
    {
        Ninguna,
        Hambre,
        Cuidado,
        Limpieza,
        Salud
    }

    public enum Estado
    {
        Vagando,
        YendoANecesidad,
        SatisfaciendoNecesidad,
        Volando,
        Aterrizando
    }

    [Header("Capacidades")]
    public bool puedeCaminar = true;
    public bool puedeVolar = false;
    public bool puedeAtenderNecesidades = true;

    [Header("Referencias")]
    public StatsDinosaurios stats;
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Lugares de necesidades")]
    public Transform foodStation;
    public Transform careStation;
    public Transform cleaningStation;
    public Transform healingStation;

    [Header("Vagabundeo")]
    public Transform wanderCenter;
    public float wanderRadius = 10f;
    public float wanderChangeTime = 4f;

    [Header("Umbrales de necesidad")]
    public float needThreshold = 45f;
    public float criticalThreshold = 20f;
    public float satisfiedThreshold = 85f;

    [Header("Recuperación por segundo")]
    public float feedAmountPerSecond = 12f;
    public float careAmountPerSecond = 10f;
    public float cleanAmountPerSecond = 10f;
    public float healAmountPerSecond = 8f;

    [Header("Vuelo")]
    public float velocidadVolar = 4f;
    public float velocidadAterrizar = 2f;
    public float alturaMinVuelo = 2f;
    public float alturaMaxVuelo = 5f;
    public float tiempoVueloMin = 4f;
    public float tiempoVueloMax = 8f;
    public float probabilidadVolar = 0.15f;
    public float tiempoCambioDireccionVuelo = 3f;
    public float distanciaBusquedaSuelo = 30f;

    [Header("Evitar obstáculos en vuelo simple")]
    public bool evitarObstaculosVolando = true;
    public float distanciaDeteccionVuelo = 3f;
    public LayerMask capasObstaculosVuelo = ~0;

    [Header("Animaciones")]
    public string parametroCaminar = "EstaCaminando";
    public string parametroVolar = "volando";
    public string parametroComer = "EstaComiendo";
    public string parametroJugar = "EstaJugando";
    public string parametroLimpiar = "EstaLimpiando";
    public string parametroCurar = "EstaCurando";

    private Estado estadoActual = Estado.Vagando;
    private Necesidad necesidadActual = Necesidad.Ninguna;

    private float wanderTimer;
    private float vueloTimer;
    private float cambioDireccionVueloTimer;
    private float alturaObjetivoVuelo;

    private Vector3 direccionVuelo;

    void Start()
    {
        if (stats == null)
            stats = GetComponent<StatsDinosaurios>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();

        estadoActual = Estado.Vagando;
        necesidadActual = Necesidad.Ninguna;

        PrepararAgenteParaSuelo();
        ElegirNuevoDestinoVagabundeo();
    }

    void Update()
    {
        switch (estadoActual)
        {
            case Estado.Volando:
                EjecutarVuelo();
                break;

            case Estado.Aterrizando:
                EjecutarAterrizaje();
                break;

            case Estado.Vagando:
                EjecutarVagabundeo();
                break;

            case Estado.YendoANecesidad:
                EjecutarIrANecesidad();
                break;

            case Estado.SatisfaciendoNecesidad:
                EjecutarSatisfacerNecesidad();
                break;
        }

        ActualizarAnimacionCaminar();
    }

    void EjecutarVagabundeo()
    {
        if (!puedeCaminar)
            return;

        Necesidad necesidadMasUrgente = ObtenerNecesidadMasUrgente();

        if (puedeAtenderNecesidades && necesidadMasUrgente != Necesidad.Ninguna)
        {
            IrANecesidad(necesidadMasUrgente);
            return;
        }

        if (puedeVolar && Random.value < probabilidadVolar * Time.deltaTime)
        {
            IniciarVuelo();
            return;
        }

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        wanderTimer -= Time.deltaTime;

        bool llegoAlDestino =
            !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance + 0.3f;

        if (wanderTimer <= 0f || llegoAlDestino)
        {
            ElegirNuevoDestinoVagabundeo();
        }
    }

    void EjecutarIrANecesidad()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (agent.pathPending)
            return;

        bool llegoAlDestino = agent.remainingDistance <= agent.stoppingDistance + 0.3f;

        if (llegoAlDestino)
        {
            agent.isStopped = true;
            estadoActual = Estado.SatisfaciendoNecesidad;
            ActivarAnimacionDeNecesidad(necesidadActual);
        }
    }

    void EjecutarSatisfacerNecesidad()
    {
        switch (necesidadActual)
        {
            case Necesidad.Hambre:
                stats.Feed(feedAmountPerSecond * Time.deltaTime, false);

                if (stats.hunger >= satisfiedThreshold)
                    TerminarNecesidad();

                break;

            case Necesidad.Cuidado:
                stats.Care(careAmountPerSecond * Time.deltaTime, false);

                if (stats.care >= satisfiedThreshold)
                    TerminarNecesidad();

                break;

            case Necesidad.Limpieza:
                stats.Clean(cleanAmountPerSecond * Time.deltaTime, false);

                if (stats.cleanliness >= satisfiedThreshold)
                    TerminarNecesidad();

                break;

            case Necesidad.Salud:
                stats.Heal(healAmountPerSecond * Time.deltaTime, false);

                if (stats.health >= satisfiedThreshold)
                    TerminarNecesidad();

                break;
        }
    }

    Necesidad ObtenerNecesidadMasUrgente()
    {
        if (stats == null)
            return Necesidad.Ninguna;

        Necesidad necesidad = Necesidad.Ninguna;
        float valorMasBajo = needThreshold;

        // Primero se priorizan hambre, cuidado y limpieza.
        // Si estas están en cero, siguen dañando la vida.
        if (stats.hunger < valorMasBajo)
        {
            valorMasBajo = stats.hunger;
            necesidad = Necesidad.Hambre;
        }

        if (stats.care < valorMasBajo)
        {
            valorMasBajo = stats.care;
            necesidad = Necesidad.Cuidado;
        }

        if (stats.cleanliness < valorMasBajo)
        {
            valorMasBajo = stats.cleanliness;
            necesidad = Necesidad.Limpieza;
        }

        if (necesidad != Necesidad.Ninguna)
            return necesidad;

        // La salud se atiende cuando no hay otra necesidad básica urgente.
        if (stats.health < needThreshold)
            return Necesidad.Salud;

        return Necesidad.Ninguna;
    }

    void IrANecesidad(Necesidad necesidad)
    {
        Transform destino = ObtenerDestinoParaNecesidad(necesidad);

        if (destino == null)
            return;

        if (!PrepararAgenteParaSuelo())
            return;

        necesidadActual = necesidad;
        estadoActual = Estado.YendoANecesidad;

        ReiniciarAnimacionesDeAccion();

        agent.isStopped = false;
        agent.SetDestination(destino.position);
    }

    Transform ObtenerDestinoParaNecesidad(Necesidad necesidad)
    {
        switch (necesidad)
        {
            case Necesidad.Hambre:
                return foodStation;

            case Necesidad.Cuidado:
                return careStation;

            case Necesidad.Limpieza:
                return cleaningStation;

            case Necesidad.Salud:
                return healingStation;

            default:
                return null;
        }
    }

    void TerminarNecesidad()
    {
        necesidadActual = Necesidad.Ninguna;
        estadoActual = Estado.Vagando;

        ReiniciarAnimacionesDeAccion();

        if (stats != null)
            stats.SaveStats();

        PrepararAgenteParaSuelo();
        ElegirNuevoDestinoVagabundeo();
    }

    void ElegirNuevoDestinoVagabundeo()
    {
        if (!PrepararAgenteParaSuelo())
            return;

        Vector3 centro = wanderCenter != null ? wanderCenter.position : transform.position;

        for (int i = 0; i < 12; i++)
        {
            Vector3 puntoAleatorio = centro + new Vector3(
                Random.Range(-wanderRadius, wanderRadius),
                0f,
                Random.Range(-wanderRadius, wanderRadius)
            );

            NavMeshHit hit;

            if (NavMesh.SamplePosition(puntoAleatorio, out hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
                wanderTimer = wanderChangeTime;
                return;
            }
        }
    }

    void IniciarVuelo()
    {
        if (!puedeVolar)
            return;

        DesactivarAgenteParaVuelo();

        estadoActual = Estado.Volando;
        necesidadActual = Necesidad.Ninguna;

        vueloTimer = Random.Range(tiempoVueloMin, tiempoVueloMax);
        alturaObjetivoVuelo = Random.Range(alturaMinVuelo, alturaMaxVuelo);

        ElegirNuevaDireccionVuelo();

        ReiniciarAnimacionesDeAccion();
        SetBoolSeguro(parametroVolar, true);
    }

    void EjecutarVuelo()
    {
        vueloTimer -= Time.deltaTime;
        cambioDireccionVueloTimer -= Time.deltaTime;

        Necesidad necesidadUrgente = ObtenerNecesidadMasUrgente();

        if (puedeAtenderNecesidades && necesidadUrgente != Necesidad.Ninguna)
        {
            IniciarAterrizaje();
            return;
        }

        if (vueloTimer <= 0f)
        {
            IniciarAterrizaje();
            return;
        }

        if (cambioDireccionVueloTimer <= 0f)
        {
            ElegirNuevaDireccionVuelo();
        }

        if (evitarObstaculosVolando)
        {
            if (Physics.Raycast(transform.position, direccionVuelo, distanciaDeteccionVuelo, capasObstaculosVuelo))
            {
                ElegirNuevaDireccionVuelo();
            }
        }

        Vector3 movimientoHorizontal = direccionVuelo * velocidadVolar * Time.deltaTime;

        Vector3 nuevaPosicion = transform.position + movimientoHorizontal;
        nuevaPosicion.y = Mathf.MoveTowards(
            nuevaPosicion.y,
            alturaObjetivoVuelo,
            velocidadAterrizar * Time.deltaTime
        );

        transform.position = nuevaPosicion;

        if (direccionVuelo != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionVuelo);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotacionObjetivo, Time.deltaTime * 3f);
        }
    }

    void ElegirNuevaDireccionVuelo()
    {
        float anguloY = Random.Range(0f, 360f);
        direccionVuelo = Quaternion.Euler(0f, anguloY, 0f) * Vector3.forward;
        direccionVuelo.Normalize();

        cambioDireccionVueloTimer = tiempoCambioDireccionVuelo;
    }

    void IniciarAterrizaje()
    {
        estadoActual = Estado.Aterrizando;
        SetBoolSeguro(parametroVolar, true);
    }

    void EjecutarAterrizaje()
    {
        NavMeshHit hit;

        bool encontroSuelo = NavMesh.SamplePosition(
            transform.position,
            out hit,
            distanciaBusquedaSuelo,
            NavMesh.AllAreas
        );

        float alturaDestino = encontroSuelo ? hit.position.y : 0f;

        Vector3 pos = transform.position;
        pos.y = Mathf.MoveTowards(pos.y, alturaDestino, velocidadAterrizar * Time.deltaTime);
        transform.position = pos;

        Quaternion rotacionHorizontal = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            rotacionHorizontal,
            Time.deltaTime * 2f
        );

        if (Mathf.Abs(transform.position.y - alturaDestino) <= 0.05f)
        {
            SetBoolSeguro(parametroVolar, false);

            if (encontroSuelo)
            {
                transform.position = hit.position;
            }

            PrepararAgenteParaSuelo();

            Necesidad necesidad = ObtenerNecesidadMasUrgente();

            if (puedeAtenderNecesidades && necesidad != Necesidad.Ninguna)
            {
                IrANecesidad(necesidad);
            }
            else
            {
                estadoActual = Estado.Vagando;
                ElegirNuevoDestinoVagabundeo();
            }
        }
    }

    bool PrepararAgenteParaSuelo()
    {
        if (agent == null)
            return false;

        if (!agent.enabled)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(transform.position, out hit, distanciaBusquedaSuelo, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.enabled = true;
            }
            else
            {
                return false;
            }
        }

        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(transform.position, out hit, distanciaBusquedaSuelo, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            else
            {
                return false;
            }
        }

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            return true;
        }

        return false;
    }

    void DesactivarAgenteParaVuelo()
    {
        if (agent == null)
            return;

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }

        agent.enabled = false;
    }

    void ActivarAnimacionDeNecesidad(Necesidad necesidad)
    {
        ReiniciarAnimacionesDeAccion();

        switch (necesidad)
        {
            case Necesidad.Hambre:
                SetBoolSeguro(parametroComer, true);
                break;

            case Necesidad.Cuidado:
                SetBoolSeguro(parametroJugar, true);
                break;

            case Necesidad.Limpieza:
                SetBoolSeguro(parametroLimpiar, true);
                break;

            case Necesidad.Salud:
                SetBoolSeguro(parametroCurar, true);
                break;
        }
    }

    void ReiniciarAnimacionesDeAccion()
    {
        SetBoolSeguro(parametroComer, false);
        SetBoolSeguro(parametroJugar, false);
        SetBoolSeguro(parametroLimpiar, false);
        SetBoolSeguro(parametroCurar, false);
    }

    void ActualizarAnimacionCaminar()
    {
        bool caminando = false;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            caminando =
                !agent.isStopped &&
                agent.velocity.magnitude > 0.1f &&
                estadoActual != Estado.Volando &&
                estadoActual != Estado.Aterrizando;
        }

        SetBoolSeguro(parametroCaminar, caminando);
    }

    void SetBoolSeguro(string nombreParametro, bool valor)
    {
        if (animator == null || string.IsNullOrEmpty(nombreParametro))
            return;

        foreach (AnimatorControllerParameter parametro in animator.parameters)
        {
            if (parametro.name == nombreParametro &&
                parametro.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(nombreParametro, valor);
                return;
            }
        }
    }
}