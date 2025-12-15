using UnityEngine;

public class GameOverCamera : MonoBehaviour
{
    [Header("Settings")]
    public Transform targetToOrbit;

    public float orbitSpeed = 20f;
    public bool lookAtTarget = true;

    [Header("Visuals")]
    public bool showRedPath = true;

    void Update()
    {
        if (targetToOrbit == null) return;


        transform.RotateAround(targetToOrbit.position, Vector3.up, orbitSpeed * Time.unscaledDeltaTime);

        if (lookAtTarget)
        {
            transform.LookAt(targetToOrbit);
        }
    }

    // THIS DRAWS THE RED LINE IN THE EDITOR
    void OnDrawGizmos()
    {
        if (targetToOrbit != null && showRedPath)
        {
            Gizmos.color = Color.red;

            // Calculate distance between camera and target
            float radius = Vector3.Distance(transform.position, targetToOrbit.position);

            // Draw a wire circle to show the path
            // (We draw 50 little lines to make a circle)
            Vector3 prevPos = targetToOrbit.position + new Vector3(radius, 0, 0);
            for (int i = 0; i < 50; i++)
            {
                float angle = (float)i / 50 * 360 * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                Vector3 newPos = targetToOrbit.position + new Vector3(x, 0, z);

                // Adjust height to match the camera
                newPos.y = transform.position.y;
                prevPos.y = transform.position.y;

                Gizmos.DrawLine(prevPos, newPos);
                prevPos = newPos;
            }
            // Connect the last point to the start
            Vector3 startPos = targetToOrbit.position + new Vector3(radius, 0, 0);
            startPos.y = transform.position.y;
            Gizmos.DrawLine(prevPos, startPos);
        }
    }
}