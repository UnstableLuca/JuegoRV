using UnityEngine;
using UnityEngine.Rendering; 

public class ControladorBrilloPalanca : MonoBehaviour
{
    public Light luzDirecional; 

    public void ModificarBrillo(float valorPalanca)
    {
        if (luzDirecional == null) return;
        
        luzDirecional.intensity = valorPalanca * 2f; 
    }
}