using UnityEngine;

public class ObjectDestroyer : MonoBehaviour
{
    [SerializeField] ScoreManager scoreManager;

    private void OnTriggerEnter(Collider other)
    {
        IDestroyable destroyable = other.gameObject.GetComponent<IDestroyable>();

        if (destroyable != null )
        {
            Destroy(other.gameObject);
        }
     }
}
