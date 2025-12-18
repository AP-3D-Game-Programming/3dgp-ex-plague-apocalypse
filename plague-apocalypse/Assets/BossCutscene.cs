using UnityEngine;

public class BossCutscene : MonoBehaviour
{
    [Header("Assignments")]
    public Transform bossObject;
    public Transform teleportTarget;
    public GameObject teleportVFX;

    [Header("Rotation Settings")]
    [Tooltip("Drag the Player or Camera here so the boss looks at them")]
    public Transform lookTarget;

    [Tooltip("If true, the boss won't look up/down, only left/right")]
    public bool keepUpright = true;

    public void PerformTeleport()
    {
        if (bossObject == null || teleportTarget == null)
        {
            Debug.LogError("Boss or Target is missing!");
            return;
        }



        // 2. Move the Boss
        bossObject.position = teleportTarget.position;
        if (teleportVFX != null)
        {
            GameObject vfxInstance = Instantiate(teleportVFX, teleportTarget.position, teleportTarget.rotation);
            Destroy(vfxInstance, 3f);
        }
        if (lookTarget != null)
        {
            if (keepUpright)
            {
                // Calculate direction from Boss to Player
                Vector3 direction = lookTarget.position - bossObject.position;

                // FLATTEN the direction (Logic from your Boss script)
                direction.y = 0;

                // Check to prevent errors if they are in the exact same spot
                if (direction != Vector3.zero)
                {
                    // Snap rotation immediately (instead of Slerp, since it's a teleport)
                    bossObject.rotation = Quaternion.LookRotation(direction);
                }
            }
            else
            {
                // Look directly at them (including looking up/down)
                bossObject.LookAt(lookTarget);
            }
        }
        else
        {
            // If no look target, just match the teleport target's rotation
            bossObject.rotation = teleportTarget.rotation;
        }

        Debug.Log("Boss Teleported and rotated!");
    }
}