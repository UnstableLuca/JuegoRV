using UnityEngine;

public class SpawnerDinosaurio : MonoBehaviour
{
    public GameObject prefabTriceratops;
    public GameObject prefabPterodactilo;
    public GameObject prefabAnquilosaurio;

    public Transform puntoSpawn;

    private void Start()
    {
        SpawnDinosaurio();
    }

    private void SpawnDinosaurio()
    {
        DinosauriosTipos elegido = GameManager.Instance.dinosaurioElegido;

        GameObject prefabAInstanciar = null;

        switch (elegido)
        {
            case DinosauriosTipos.Triceratops:
                prefabAInstanciar = prefabTriceratops;
                break;

            case DinosauriosTipos.Pterodactilo:
                prefabAInstanciar = prefabPterodactilo;
                break;

            case DinosauriosTipos.Anquilosaurio:
                prefabAInstanciar = prefabAnquilosaurio;
                break;
        }

        if (prefabAInstanciar != null)
        {
            Instantiate(prefabAInstanciar, puntoSpawn.position, puntoSpawn.rotation);
        }
    }
}
