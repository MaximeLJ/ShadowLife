using UnityEngine;

public class PixelPerfectCameraFollow : MonoBehaviour
{
    public Transform target;
    public float pixelsPerUnit = 16f; // Match this to your PPU

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position;
            float snapValue = 1f / pixelsPerUnit;
            transform.position = new Vector3(
                Mathf.Round(targetPosition.x / snapValue) * snapValue,
                Mathf.Round(targetPosition.y / snapValue) * snapValue,
                transform.position.z
            );
        }
    }
}
