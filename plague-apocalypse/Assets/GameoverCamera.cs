using UnityEngine;

public class GamveOverCamera : MonoBehaviour
{
    [Header("Movement ")]
    public float moveSpeed = 5f;

    [Tooltip("rotate")]
    public float rotateSpeed = 2f;
    public Vector3 driftDirection = new Vector3(0, 0, 1);

    void Update()
    {

        transform.Translate(driftDirection * moveSpeed * Time.unscaledDeltaTime, Space.World);

        transform.Rotate(Vector3.up * rotateSpeed * Time.unscaledDeltaTime, Space.World);
    }
}