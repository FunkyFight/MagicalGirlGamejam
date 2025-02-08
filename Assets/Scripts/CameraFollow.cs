using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform target;
    [SerializeField] public Vector2 offset;
    [SerializeField] public float cameraSize;
    [SerializeField] public float drag;

    private Camera camera;

    // Update is called once per frame
    void Start()
    {
        camera = GetComponent<Camera>();
    }    
    void Update()
    {
        Vector3 newPos = new Vector3(target.position.x + offset.x, target.position.y + offset.y, -10);
        transform.position = Vector3.Lerp(transform.position, newPos, drag);
        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, cameraSize, drag);
    }
}
