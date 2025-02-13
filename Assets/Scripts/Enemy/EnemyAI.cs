using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    [SerializeField] protected GameMediator mediator;
    [Header("AI Settings")]
    [SerializeField] protected float detectionRange = 10f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Attack"))
        {
            Destroy(gameObject);
        }
    }
}