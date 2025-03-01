using UnityEngine;

[ExecuteInEditMode]
public class CameraPointEvent : MonoBehaviour
{
    [Header("Camera Override Settings")]
    [SerializeField] private bool followTarget = false;

    [Header("Camera Preview Settings")]
    [Tooltip("This represents the center position of the camera view.")]
    public Vector3 cameraPosition;

    [Tooltip("For an orthographic camera, this is the half-height of the view.")]
    public float cameraSize = 5f;

    [Tooltip("The aspect ratio to use when drawing the camera preview (width/height).")]
    public float aspectRatio = 1.77778f; // 16:9 aspect by default

    [Header("Game Mediator")]
    [SerializeField] private GameMediator mediator;

    private void OnDrawGizmos()
    {
        if(followTarget) return;

        // Calculate the view dimensions:
        float halfHeight = cameraSize;
        float halfWidth = halfHeight * aspectRatio;
        Vector3 viewSize = new Vector3(halfWidth * 2, halfHeight * 2, 0);

        // Draw the camera rectangle (green outline)
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(cameraPosition, viewSize);

        // Draw the camera position (red sphere)
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(cameraPosition, 0.1f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<PlayerController>() == null) return;

        if(followTarget)
        {
            mediator.FollowTarget();
            cameraSize = 5f;
        } else {
            mediator.PointCameraTo(cameraPosition);
        }

        mediator.SetCameraSizeAndOffset(cameraSize, Vector2.zero);
    }
}
