using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    [SerializeField] protected GameMediator mediator;
    [Header("AI Settings")]
    [SerializeField] protected float detectionRange = 10f;
}