using System;
using Unity.VisualScripting;
using UnityEngine;

public class FlyingEnemyAI : EnemyAI
{
    enum EnemyStates {PATROLLING, CHASING, DASH}

    
    
    [SerializeField] private float flyXSpeed = 10f;
    [SerializeField] private float flyYSpeed = 10f;
    [SerializeField] private float flightIntervals = 1f; // In seconds
    private float flightBuffer = 0;

    [Header("Flight Intervals Settings")]
    [SerializeField] private float patrollingFlightIntervals = 1f;
    [SerializeField] private float chasingFlightIntervals = 0.5f;

    private EnemyStates enemyState = EnemyStates.PATROLLING;
    private Rigidbody2D body;
    

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }


    void FixedUpdate()
    {
        switch(enemyState)
        {
            case EnemyStates.PATROLLING:
                Patrol();
                break;
        }

        flightBuffer--;
    }
    

    private void Patrol()
    {
        Vector2? playerPosition = mediator.GetPlayerPosition();
        if(playerPosition == null) return;

        flightIntervals = patrollingFlightIntervals;
        if(flightBuffer <= 0)
        {
            // The flying enemy can fly again, choose a random direction.
            float randomAngle = UnityEngine.Random.Range(0f, 360f);
            Vector2 randomDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)).normalized;
            body.linearVelocity = new Vector2(flyXSpeed * randomDirection.x, flyYSpeed * randomDirection.y);

            // Reset the buffer
            flightBuffer = flightIntervals;
            return;
        }

        /*
        // Transition condition -> ChasePlayer()
        float distance = Vector2.Distance(transform.position, playerPosition.Value);
        if(distance <= detectionRange) enemyState = EnemyStates.CHASING;
        */
    }
}
