using UnityEngine;

public class TimeIgnore : MonoBehaviour
{

    [HideInInspector] public float moveSpeed;

    private Vector3 moveDirection;

    void Start()
    {
        moveDirection = transform.forward;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.unscaledDeltaTime;
    }
}