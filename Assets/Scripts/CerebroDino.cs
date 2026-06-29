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
        EsperandoIdle,
        Vagando,
        YendoANecesidad,
        SatisfaciendoNecesidad
    }

    [Header("Capacidades")]
    public bool puedeCaminar = true;
    public bool puedeAtenderNecesidades = true;

    [Header("Referencias")]
    public StatsDinosaurios stats;
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Lugares fijos de necesidades")]
    public Transform foodStation;
    public Transform careStation;
    public Transform cleaningStation;
    public Transform healingStation;

    [Header("Objetos en mano / herramientas")]
    public bool buscarObjetosEnMano = true;
    public float distanciaMaximaObjetoEnMano = 15f;
    public float intervaloActualizarDestinoMovil = 0.25f;
    public float distanciaBusquedaNavMeshDestino = 4f;
    public float distanciaLlegadaObjetoEnMano = 1.5f;

    [Header("Vagabundeo")]
    public Transform wanderCenter;
    public float wanderRadius = 10f;
    public float wanderChangeTime = 4f;

    [Header("Idle / Espera")]
    public float idleTimeMin = 2f;
    public float idleTimeMax = 5f;
    public bool esperarIdleAlTerminarNecesidad = true;
    public bool esperarIdleAlLlegarDestino = true;
    public bool idlePuedeSerInterrumpidoPorNecesidadCritica = true;

    [Header("Umbrales de necesidad")]
    public float needThreshold = 45f;
    public float criticalThreshold = 20f;
    public float satisfiedThreshold = 85f;

    [Header("Recuperación por segundo")]
    public float feedAmountPerSecond = 12f;
    public float careAmountPerSecond = 10f;
    public float cleanAmountPerSecond = 10f;
    public float healAmountPerSecond = 8f;

    [Header("Animaciones")]
    public string parametroCaminar = "EstaCaminando";
    public string parametroIdle = "EstaIdle";
    public string parametroComer = "EstaComiendo";
    public string parametroJugar = "EstaJugando";
    public string parametroLimpiar = "EstaLimpiando";
    public string parametroCurar = "EstaCurando";

    private Estado estadoActual = Estado.EsperandoIdle;
    private Necesidad necesidadActual = Necesidad.Ninguna;

    private float wanderTimer;
    private float idleTimer;
    private float actualizarDestinoTimer;

    private Transform destinoNecesidadActual;
    private bool destinoActualEsObjetoEnMano;

    void Start()
    {
        if (stats == null)
            stats = GetComponent<StatsDinosaurios>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();

        PrepararAgenteParaSuelo();
        IniciarIdle(false);
    }

    void Update()
    {
        switch (estadoActual)
        {
            case Estado.EsperandoIdle:
                EjecutarIdle();
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
        ActualizarAnimacionIdle();
    }

    void EjecutarIdle()
    {
        DetenerAgente();

        if (idlePuedeSerInterrumpidoPorNecesidadCritica && puedeAtenderNecesidades)
        {
            Necesidad necesidadCritica = ObtenerNecesidadCritica();

            if (necesidadCritica != Necesidad.Ninguna)
            {
                IrANecesidad(necesidadCritica);
                return;
            }
        }

        idleTimer -= Time.deltaTime;

        if (idleTimer > 0f)
            return;

        Necesidad necesidad = ObtenerNecesidadMasUrgente();

        if (puedeAtenderNecesidades && necesidad != Necesidad.Ninguna)
        {
            IrANecesidad(necesidad);
            return;
        }

        if (puedeCaminar)
        {
            estadoActual = Estado.Vagando;
            ElegirNuevoDestinoVagabundeo();
        }
        else
        {
            IniciarIdle(false);
        }
    }

    void EjecutarVagabundeo()
    {
        if (!puedeCaminar)
        {
            IniciarIdle(false);
            return;
        }

        Necesidad necesidadMasUrgente = ObtenerNecesidadMasUrgente();

        if (puedeAtenderNecesidades && necesidadMasUrgente != Necesidad.Ninguna)
        {
            IrANecesidad(necesidadMasUrgente);
            return;
        }

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        wanderTimer -= Time.deltaTime;

        bool llegoAlDestino =
            !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance + 0.3f;

        if (llegoAlDestino)
        {
            if (esperarIdleAlLlegarDestino)
            {
                IniciarIdle(false);
            }
            else
            {
                ElegirNuevoDestinoVagabundeo();
            }

            return;
        }

        if (wanderTimer <= 0f)
        {
            ElegirNuevoDestinoVagabundeo();
        }
    }

    void EjecutarIrANecesidad()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (destinoNecesidadActual == null)
        {
            IniciarIdle(false);
            return;
        }

        if (destinoActualEsObjetoEnMano && ObjetoEnManoYaNoEsValido(destinoNecesidadActual))
        {
            Transform nuevoDestino = ObtenerDestinoParaNecesidad(necesidadActual);

            if (nuevoDestino == null)
            {
                IniciarIdle(false);
                return;
            }

            destinoNecesidadActual = nuevoDestino;
            destinoActualEsObjetoEnMano = EsObjetoNecesidadEnMano(nuevoDestino);
            actualizarDestinoTimer = 0f;
        }

        actualizarDestinoTimer -= Time.deltaTime;

        if (actualizarDestinoTimer <= 0f)
        {
            actualizarDestinoTimer = intervaloActualizarDestinoMovil;
            MoverAgenteHaciaDestino(destinoNecesidadActual);
        }

        if (agent.pathPending)
            return;

        bool llegoAlDestino;

        if (destinoActualEsObjetoEnMano)
        {
            float distanciaReal = Vector3.Distance(
                transform.position,
                destinoNecesidadActual.position
            );

            llegoAlDestino = distanciaReal <= distanciaLlegadaObjetoEnMano;
        }
        else
        {
            llegoAlDestino =
                agent.remainingDistance <= agent.stoppingDistance + 0.3f;
        }

        if (llegoAlDestino)
        {
            DetenerAgente();

            estadoActual = Estado.SatisfaciendoNecesidad;
            ActivarAnimacionDeNecesidad(necesidadActual);
        }
    }

    void EjecutarSatisfacerNecesidad()
    {
        if (stats == null)
            return;

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

        if (stats.health < needThreshold)
            return Necesidad.Salud;

        return Necesidad.Ninguna;
    }

    Necesidad ObtenerNecesidadCritica()
    {
        if (stats == null)
            return Necesidad.Ninguna;

        Necesidad necesidad = Necesidad.Ninguna;
        float valorMasBajo = criticalThreshold;

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

        if (stats.health < criticalThreshold)
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

        destinoNecesidadActual = destino;
        destinoActualEsObjetoEnMano = EsObjetoNecesidadEnMano(destino);
        actualizarDestinoTimer = 0f;

        ReiniciarAnimacionesDeAccion();
        SetBoolSeguro(parametroIdle, false);

        agent.isStopped = false;
        MoverAgenteHaciaDestino(destinoNecesidadActual);
    }

    Transform ObtenerDestinoParaNecesidad(Necesidad necesidad)
    {
        if (buscarObjetosEnMano)
        {
            ObjetoNecesidadDino objeto = ObjetoNecesidadDino.BuscarObjetoParaNecesidad(
                necesidad,
                transform.position,
                distanciaMaximaObjetoEnMano
            );

            if (objeto != null)
            {
                return objeto.transform;
            }
        }

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

    bool EsObjetoNecesidadEnMano(Transform posibleObjeto)
    {
        if (posibleObjeto == null)
            return false;

        ObjetoNecesidadDino objeto = posibleObjeto.GetComponent<ObjetoNecesidadDino>();

        if (objeto == null)
            return false;

        return objeto.EstaAgarrado;
    }

    bool ObjetoEnManoYaNoEsValido(Transform posibleObjeto)
    {
        if (posibleObjeto == null)
            return true;

        ObjetoNecesidadDino objeto = posibleObjeto.GetComponent<ObjetoNecesidadDino>();

        if (objeto == null)
            return true;

        if (!objeto.atraeDinos)
            return true;

        if (objeto.soloAtraeSiEstaAgarrado && !objeto.EstaAgarrado)
            return true;

        if (!objeto.PuedeCubrirNecesidad(necesidadActual))
            return true;

        return false;
    }

    void MoverAgenteHaciaDestino(Transform destino)
    {
        if (destino == null)
            return;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        NavMeshHit hit;

        bool encontroPuntoNavMesh = NavMesh.SamplePosition(
            destino.position,
            out hit,
            distanciaBusquedaNavMeshDestino,
            NavMesh.AllAreas
        );

        if (encontroPuntoNavMesh)
        {
            agent.SetDestination(hit.position);
        }
    }

    void TerminarNecesidad()
    {
        necesidadActual = Necesidad.Ninguna;
        destinoNecesidadActual = null;
        destinoActualEsObjetoEnMano = false;

        ReiniciarAnimacionesDeAccion();

        if (stats != null)
            stats.SaveStats();

        if (esperarIdleAlTerminarNecesidad)
        {
            IniciarIdle(false);
        }
        else
        {
            estadoActual = Estado.Vagando;
            ElegirNuevoDestinoVagabundeo();
        }
    }

    void IniciarIdle(bool guardarStats)
    {
        estadoActual = Estado.EsperandoIdle;
        necesidadActual = Necesidad.Ninguna;
        destinoNecesidadActual = null;
        destinoActualEsObjetoEnMano = false;

        DetenerAgente();
        ReiniciarAnimacionesDeAccion();

        float min = Mathf.Min(idleTimeMin, idleTimeMax);
        float max = Mathf.Max(idleTimeMin, idleTimeMax);

        idleTimer = Random.Range(min, max);

        if (guardarStats && stats != null)
            stats.SaveStats();
    }

    void ElegirNuevoDestinoVagabundeo()
    {
        if (!puedeCaminar)
        {
            IniciarIdle(false);
            return;
        }

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

        IniciarIdle(false);
    }

    bool PrepararAgenteParaSuelo()
    {
        if (agent == null)
            return false;

        if (!agent.enabled)
            agent.enabled = true;

        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            else
            {
                return false;
            }
        }

        return agent.isOnNavMesh;
    }

    void DetenerAgente()
    {
        if (agent == null)
            return;

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    void ActivarAnimacionDeNecesidad(Necesidad necesidad)
    {
        ReiniciarAnimacionesDeAccion();
        SetBoolSeguro(parametroIdle, false);

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
                (estadoActual == Estado.Vagando || estadoActual == Estado.YendoANecesidad);
        }

        SetBoolSeguro(parametroCaminar, caminando);
    }

    void ActualizarAnimacionIdle()
    {
        bool estaIdle = estadoActual == Estado.EsperandoIdle;
        SetBoolSeguro(parametroIdle, estaIdle);
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