using UnityEngine;

public class Spinner : MonoBehaviour
{
    [Header("Spin Settings")]
    public float rotationSpeed = 50f; // Degrees per second

    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}