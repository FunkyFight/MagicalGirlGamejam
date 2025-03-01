using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] public Transform target;
    [SerializeField] public Vector2 offset;
    [SerializeField] public float cameraSize;
    [SerializeField] public float drag;

    private Camera camera;
    private Vector3? lookAtPoint = null;

    void Start()
    {
        camera = GetComponent<Camera>();
    }    

    void Update()
    {
        Vector3 newPos;

        if (lookAtPoint.HasValue)
        {
            newPos = new Vector3(lookAtPoint.Value.x + offset.x, lookAtPoint.Value.y + offset.y, -10);
        }
        else if (target != null)
        {
            newPos = new Vector3(target.position.x + offset.x, target.position.y + offset.y, -10);
        }
        else
        {
            return;
        }

        transform.position = Vector3.Lerp(transform.position, newPos, drag);
        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, cameraSize, drag);
    }

    public void LookAtPoint(Vector3 point)
    {
        lookAtPoint = point;
    }

    public void FollowTarget()
    {
        lookAtPoint = null;
    }
}
