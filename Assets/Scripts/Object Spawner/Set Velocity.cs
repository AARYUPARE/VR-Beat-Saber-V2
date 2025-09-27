using UnityEngine;

public class SetVelocity : MonoBehaviour
{
    [SerializeField] float speed;
    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb.linearVelocity = new Vector3(0.0f, 0.0f, speed);
    }
}
