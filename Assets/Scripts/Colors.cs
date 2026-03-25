using UnityEngine;

public class Colors : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetRed()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }
    public void SetBlue()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }
}
