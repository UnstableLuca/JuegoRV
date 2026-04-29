using UnityEngine;

public class ComidaDino : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Comida dinosaur = other.GetComponent<Comida>();

        if (dinosaur != null)
        {
            dinosaur.Eat();

            gameObject.SetActive(false);
        }
    }
}
