using System;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
using UnityEngine;

public class FlyingEnemyAI : EnemyAI
{
    enum EnemyStates { PATROLLING, CHASING, DASH }

    [SerializeField] private float flyXSpeed = 10f;
    [SerializeField] private float flyYSpeed = 10f;
    [SerializeField] private float flightIntervals = 1f;
    private float flightBuffer = 0f;

    [Header("Flight Intervals Settings")]
    [SerializeField] private float patrollingFlightIntervals = 1f;
    [SerializeField] private float chasingFlightIntervals = 0.5f;

    [Header("Bat Flight Settings")]
    [SerializeField] private float batOscillationFrequency = 2f;
    [SerializeField] private float batOscillationAmplitude = 0.5f;

    [Header("Chase Behavior")]
    [SerializeField] private float chaseYOffset = 2f;

    private EnemyStates enemyState = EnemyStates.PATROLLING;
    private Rigidbody2D body;
    private Vector2 currentBaseDirection = Vector2.zero;

    float chaseBuffer = 0;
    float timeBeforeReChasing = 0.5f;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        flightBuffer -= Time.fixedDeltaTime;
        switch (enemyState)
        {
            case EnemyStates.PATROLLING:
                Patrol();
                break;
            case EnemyStates.CHASING:
                ChasePlayer();
                break;
        }
        if(chaseBuffer <= 0 && currentBaseDirection != Vector2.zero)
        {
            Vector2 perpendicular = new Vector2(-currentBaseDirection.y, currentBaseDirection.x);
            float oscillation = Mathf.Sin(Time.time * batOscillationFrequency) * batOscillationAmplitude;
            Vector2 finalDirection = (currentBaseDirection + perpendicular * oscillation).normalized;
            body.linearVelocity = new Vector2(flyXSpeed * finalDirection.x, flyYSpeed * finalDirection.y);
        }
    }

    private void Patrol()
    {
        Vector2? playerPosition = mediator.GetPlayerPosition();
        if (playerPosition == null)
            return;
        flightIntervals = patrollingFlightIntervals;
        float distance = Vector2.Distance(transform.position, playerPosition.Value);
        if (distance <= detectionRange)
        {
            enemyState = EnemyStates.CHASING;
            flightBuffer = 0;
            return;
        }
        if (flightBuffer <= 0)
        {
            float randomAngle = UnityEngine.Random.Range(0f, 360f);
            float rad = randomAngle * Mathf.Deg2Rad;
            currentBaseDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
            flightBuffer = flightIntervals;
        }
    }

    private void ChasePlayer()
    {
        if(chaseBuffer > 0)
        {
            chaseBuffer -= Time.fixedDeltaTime;
            if (body != null)
            {
                float frictionCoefficient = 0.95f;
                body.linearVelocity *= frictionCoefficient;
                if (body.linearVelocity.magnitude < 0.1f)
                    body.linearVelocity = Vector2.zero;
            }
            return;
        }
        body.gravityScale = 0;
        Vector2? playerPosition = mediator.GetPlayerPosition();
        if (playerPosition == null)
            return;
        flightIntervals = chasingFlightIntervals;
        float distance = Vector2.Distance(transform.position, playerPosition.Value);
        if (distance > detectionRange)
        {
            enemyState = EnemyStates.PATROLLING;
            flightBuffer = 0;
            return;
        }
        if (flightBuffer <= 0)
        {
            Vector2 targetPosition = playerPosition.Value + new Vector2(0, chaseYOffset);
            currentBaseDirection = (targetPosition - (Vector2)transform.position).normalized;
            flightBuffer = flightIntervals;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerDamageController playerDamage = collision.gameObject.GetComponent<PlayerDamageController>();
        if(playerDamage == null)
        {
            Debug.Log("Collision detected with an object that does not have a PlayerDamageController component.");
            return;
        }
        chaseBuffer = timeBeforeReChasing;
        body.gravityScale = 0;
        Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
        float knockbackForce = 15f;
        body.linearVelocity += knockbackDirection * knockbackForce;
        Debug.Log($"Knockback applied with direction: {knockbackDirection} and force: {knockbackForce}");
    }
}
