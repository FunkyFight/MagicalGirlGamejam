using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset;
    [SerializeField] private Vector2 scaleOffset;
    [SerializeField] private float drag;


    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = new Vector3(target.position.x + offset.x, target.position.y + offset.y, -10);
        transform.position = Vector3.Lerp(transform.position, newPos, drag);
    }
}
