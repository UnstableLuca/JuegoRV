using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CerebroDinoNavMesh : MonoBehaviour
{
    public enum Actividad
    {
        Quieto,
        Caminar,
        Volar,
        Aterrizar
    }

    [Header("Capacidades")]
    public bool puedeCaminar = true;
    public bool puedeVolar = false;

    [Header("Animator")]
    public Animator animator;
    public string parametroCaminar = "EstaCaminando";
    public string parametroVolar = "volando";

    [Header("NavMesh")]
    public NavMeshAgent agent;
    public float radioBusquedaDestino = 10f;
    public float tiempoParaNuevoDestino = 3f;

    [Header("Movimiento aéreo")]
    public float velocidadVolar = 4f;
    public float velocidadAterrizar = 2f;
    public float alturaMinVuelo = 2f;
    public float alturaMaxVuelo = 5f;

    [Header("Tiempos")]
    public float tiempoActividadMin = 2f;
    public float tiempoActividadMax = 5f;
    public float tiempoVueloMin = 4f;
    public float tiempoVueloMax = 8f;

    [Header("Probabilidades")]
    [Range(0f, 1f)] public float probabilidadCaminar = 0.45f;
    [Range(0f, 1f)] public float probabilidadVolar = 0.35f;

    private Actividad actividadActual;
    private float alturaObjetivoVuelo;
    private float temporizadorDestino;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.updateRotation = true;
            agent.updatePosition = true;
        }

        StartCoroutine(CicloDeCerebro());
    }

    void Update()
    {
        EjecutarActividadActual();
    }

    IEnumerator CicloDeCerebro()
    {
        while (true)
        {
            Actividad nuevaActividad = ElegirActividad();
            IniciarActividad(nuevaActividad);

            if (nuevaActividad == Actividad.Volar)
            {
                yield return new WaitForSeconds(Random.Range(tiempoVueloMin, tiempoVueloMax));
            }
            else if (nuevaActividad == Actividad.Aterrizar)
            {
                while (actividadActual == Actividad.Aterrizar)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(Random.Range(tiempoActividadMin, tiempoActividadMax));
            }
            else
            {
                yield return new WaitForSeconds(Random.Range(tiempoActividadMin, tiempoActividadMax));
            }
        }
    }

    Actividad ElegirActividad()
    {
        bool estaEnElAire = !EstaCercaDelSuelo();

        if (puedeVolar && estaEnElAire)
        {
            return Random.value > 0.4f ? Actividad.Volar : Actividad.Aterrizar;
        }

        float decision = Random.value;

        if (puedeVolar && decision < probabilidadVolar)
        {
            return Actividad.Volar;
        }

        if (puedeCaminar && decision < probabilidadVolar + probabilidadCaminar)
        {
            return Actividad.Caminar;
        }

        return Actividad.Quieto;
    }

    void IniciarActividad(Actividad nuevaActividad)
    {
        actividadActual = nuevaActividad;
        ReiniciarAnimaciones();

        switch (actividadActual)
        {
            case Actividad.Quieto:
                DetenerAgente();
                break;

            case Actividad.Caminar:
                PrepararAgenteParaCaminar();
                SetBoolSeguro(parametroCaminar, true);
                ElegirNuevoDestinoNavMesh();
                break;

            case Actividad.Volar:
                DesactivarAgenteParaVuelo();
                alturaObjetivoVuelo = Random.Range(alturaMinVuelo, alturaMaxVuelo);
                RotarAleatoriamenteEnY();
                SetBoolSeguro(parametroVolar, true);
                break;

            case Actividad.Aterrizar:
                DesactivarAgenteParaVuelo();
                SetBoolSeguro(parametroVolar, true);
                break;
        }
    }

    void EjecutarActividadActual()
    {
        switch (actividadActual)
        {
            case Actividad.Caminar:
                EjecutarCaminarConNavMesh();
                break;

            case Actividad.Volar:
                EjecutarVueloSimple();
                break;

            case Actividad.Aterrizar:
                EjecutarAterrizaje();
                break;
        }
    }

    void EjecutarCaminarConNavMesh()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        temporizadorDestino -= Time.deltaTime;

        bool llegoAlDestino =
            !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance;

        if (llegoAlDestino || temporizadorDestino <= 0f)
        {
            ElegirNuevoDestinoNavMesh();
        }
    }

    void ElegirNuevoDestinoNavMesh()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        for (int i = 0; i < 10; i++)
        {
            Vector3 puntoAleatorio = transform.position + new Vector3(
                Random.Range(-radioBusquedaDestino, radioBusquedaDestino),
                0f,
                Random.Range(-radioBusquedaDestino, radioBusquedaDestino)
            );

            NavMeshHit hit;

            if (NavMesh.SamplePosition(puntoAleatorio, out hit, radioBusquedaDestino, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                temporizadorDestino = tiempoParaNuevoDestino;
                return;
            }
        }
    }

    void EjecutarVueloSimple()
    {
        transform.Translate(Vector3.forward * velocidadVolar * Time.deltaTime);

        Vector3 pos = transform.position;
        pos.y = Mathf.MoveTowards(
            pos.y,
            alturaObjetivoVuelo,
            velocidadAterrizar * Time.deltaTime
        );

        transform.position = pos;
    }

    void EjecutarAterrizaje()
    {
        NavMeshHit hit;

        bool encontroSueloNavMesh = NavMesh.SamplePosition(
            transform.position,
            out hit,
            20f,
            NavMesh.AllAreas
        );

        float alturaDestino = encontroSueloNavMesh ? hit.position.y : 0f;

        Vector3 pos = transform.position;
        pos.y = Mathf.MoveTowards(
            pos.y,
            alturaDestino,
            velocidadAterrizar * Time.deltaTime
        );

        transform.position = pos;

        Quaternion rotacionHorizontal = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            rotacionHorizontal,
            Time.deltaTime * 2f
        );

        if (Mathf.Abs(transform.position.y - alturaDestino) < 0.05f)
        {
            if (encontroSueloNavMesh && agent != null)
            {
                agent.enabled = true;
                agent.Warp(hit.position);
            }

            actividadActual = Actividad.Quieto;
            ReiniciarAnimaciones();
        }
    }

    void PrepararAgenteParaCaminar()
    {
        if (agent == null)
            return;

        if (!agent.enabled)
            agent.enabled = true;

        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }

    void DetenerAgente()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    void DesactivarAgenteParaVuelo()
    {
        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
            }

            agent.enabled = false;
        }
    }

    bool EstaCercaDelSuelo()
    {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
        {
            return Mathf.Abs(transform.position.y - hit.position.y) < 0.2f;
        }

        return transform.position.y <= 0.2f;
    }

    void RotarAleatoriamenteEnY()
    {
        float anguloY = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(0f, anguloY, 0f);
    }

    void ReiniciarAnimaciones()
    {
        SetBoolSeguro(parametroCaminar, false);
        SetBoolSeguro(parametroVolar, false);
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