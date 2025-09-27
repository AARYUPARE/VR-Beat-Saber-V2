using UnityEngine;

public class DestroyableObject : MonoBehaviour, IDestroyable
{
    [SerializeField] GameObject destroyParticle;

    private void OnDestroy()
    {
        if (destroyParticle)
        { 
            Instantiate(destroyParticle, transform.position, Quaternion.identity);
        }
    }
}
