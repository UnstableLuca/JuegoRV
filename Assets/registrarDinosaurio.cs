using UnityEngine;

public class registrarDinosaurio : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.dinosaurioVivoEnEscena = this.gameObject;
        }
    }
}
