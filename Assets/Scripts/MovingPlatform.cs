using DG.Tweening;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    [SerializeField] public Vector3 destination;
    [SerializeField] public float duration = 2f;


    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + destination);
    }

    void Start()
    {
        transform.DOMove(transform.position + destination, duration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }
}
