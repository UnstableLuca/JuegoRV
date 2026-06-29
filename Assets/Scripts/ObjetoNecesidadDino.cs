using System.Collections.Generic;
using UnityEngine;

public class ObjetoNecesidadDino : MonoBehaviour
{
    public enum TipoObjeto
    {
        Jabon,
        Bola,
        Comida,
        Medicina
    }

    [Header("Tipo")]
    public TipoObjeto tipoObjeto;

    [Header("Atracción")]
    public bool atraeDinos = true;
    public bool soloAtraeSiEstaAgarrado = true;

    [Header("Debug")]
    [SerializeField] private bool estaAgarrado;

    private static readonly List<ObjetoNecesidadDino> objetosActivos =
        new List<ObjetoNecesidadDino>();

    public bool EstaAgarrado => estaAgarrado;

    private void OnEnable()
    {
        if (!objetosActivos.Contains(this))
            objetosActivos.Add(this);
    }

    private void OnDisable()
    {
        objetosActivos.Remove(this);
    }

    public void MarcarAgarrado()
    {
        estaAgarrado = true;
    }

    public void MarcarSoltado()
    {
        estaAgarrado = false;
    }

    public bool PuedeCubrirNecesidad(CerebroDino.Necesidad necesidad)
    {
        switch (necesidad)
        {
            case CerebroDino.Necesidad.Limpieza:
                return tipoObjeto == TipoObjeto.Jabon;

            case CerebroDino.Necesidad.Cuidado:
                return tipoObjeto == TipoObjeto.Bola;

            case CerebroDino.Necesidad.Hambre:
                return tipoObjeto == TipoObjeto.Comida;

            case CerebroDino.Necesidad.Salud:
                return tipoObjeto == TipoObjeto.Medicina;

            default:
                return false;
        }
    }

    public static ObjetoNecesidadDino BuscarObjetoParaNecesidad(
        CerebroDino.Necesidad necesidad,
        Vector3 posicionDino,
        float distanciaMaxima
    )
    {
        ObjetoNecesidadDino mejorObjeto = null;
        float mejorDistancia = distanciaMaxima;

        foreach (ObjetoNecesidadDino objeto in objetosActivos)
        {
            if (objeto == null)
                continue;

            if (!objeto.atraeDinos)
                continue;

            if (objeto.soloAtraeSiEstaAgarrado && !objeto.estaAgarrado)
                continue;

            if (!objeto.PuedeCubrirNecesidad(necesidad))
                continue;

            float distancia = Vector3.Distance(posicionDino, objeto.transform.position);

            if (distancia < mejorDistancia)
            {
                mejorDistancia = distancia;
                mejorObjeto = objeto;
            }
        }

        return mejorObjeto;
    }
}