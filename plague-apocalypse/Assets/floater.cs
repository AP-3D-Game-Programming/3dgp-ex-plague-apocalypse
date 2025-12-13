using UnityEngine;

public class Floater : MonoBehaviour
{

    public float frequency = 1f;

    public float amplitude = 0.5f;


    public float offset = 0f;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {

        float yDisplacement = Mathf.Sin(Time.time * frequency + offset) * amplitude;
        transform.position = startPosition + new Vector3(0, yDisplacement, 0);
    }
}