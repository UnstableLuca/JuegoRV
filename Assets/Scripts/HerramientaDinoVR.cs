using UnityEngine;

public class HerramientaDinoVR : MonoBehaviour
{
    public enum TipoHerramienta
    {
        Jabon,
        Bola
    }

    public enum ModoUso
    {
        MientrasToca,
        SoloAlGolpear
    }

    [Header("Tipo")]
    public TipoHerramienta tipoHerramienta;

    [Header("Modo")]
    public ModoUso modoUso = ModoUso.MientrasToca;

    [Header("Efecto")]
    public float cantidadPorSegundo = 20f;
    public float cantidadAlGolpear = 15f;

    [Header("VR")]
    public bool soloFuncionaSiEstaAgarrado = true;

    [Header("Golpe")]
    public float velocidadMinimaGolpe = 1.5f;

    private bool estaAgarrado;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerStay(Collider other)
    {
        if (modoUso != ModoUso.MientrasToca)
            return;

        if (soloFuncionaSiEstaAgarrado && !estaAgarrado)
            return;

        StatsDinosaurios stats = other.GetComponentInParent<StatsDinosaurios>();

        if (stats == null)
            return;

        AplicarEfectoContinuo(stats);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (modoUso != ModoUso.SoloAlGolpear)
            return;

        if (soloFuncionaSiEstaAgarrado && !estaAgarrado)
            return;

        if (rb != null && rb.linearVelocity.magnitude < velocidadMinimaGolpe)
            return;

        StatsDinosaurios stats = collision.collider.GetComponentInParent<StatsDinosaurios>();

        if (stats == null)
            return;

        AplicarEfectoInstantaneo(stats);
    }

    void AplicarEfectoContinuo(StatsDinosaurios stats)
    {
        switch (tipoHerramienta)
        {
            case TipoHerramienta.Jabon:
                stats.Clean(cantidadPorSegundo * Time.deltaTime, false);
                break;

            case TipoHerramienta.Bola:
                stats.Care(cantidadPorSegundo * Time.deltaTime, false);
                break;
        }
    }

    void AplicarEfectoInstantaneo(StatsDinosaurios stats)
    {
        switch (tipoHerramienta)
        {
            case TipoHerramienta.Jabon:
                stats.Clean(cantidadAlGolpear);
                break;

            case TipoHerramienta.Bola:
                stats.Care(cantidadAlGolpear);
                break;
        }
    }

    public void MarcarAgarrado()
    {
        estaAgarrado = true;
    }

    public void MarcarSoltado()
    {
        estaAgarrado = false;
    }
}