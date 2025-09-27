using UnityEngine;

public class HitDestroy : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        IDestroyable destroyable = collision.gameObject.GetComponent<IDestroyable>();

        if (destroyable == null) { return; }

        Destroy(collision.gameObject);
    }
}
