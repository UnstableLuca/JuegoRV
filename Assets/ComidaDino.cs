using UnityEngine;

public class ComidaDino : MonoBehaviour
{
    
    private void OnTriggerEnter(Collider other)
    {
        Comida dinosaur = other.GetComponent<Comida>();

        if (dinosaur != null)
        {
            StatsDinosaurios stats = GetComponent<StatsDinosaurios>();
            stats.Feed(100f);
            gameObject.SetActive(false);
        }
    }
}
