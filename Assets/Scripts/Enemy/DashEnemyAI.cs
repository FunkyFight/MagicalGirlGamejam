using System;
using Unity.VisualScripting;
using UnityEngine;

public class DashEnemyAI : EnemyAI
{
    enum EnemyStates {PATROLLING, CHASING}

    [Header("movement Settings")]
    [SerializeField] private float XSpeed = 2f;
    [SerializeField] private float YSpeed = 2f;
    [SerializeField] private float DashIntervals = 4f; // En secondes
    private float DashBuffer = 0;

    [Header("Jump Intervals Settings")]
    [SerializeField] private float chasingDashIntervals = 0.5f;

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
            case EnemyStates.CHASING:
                ChasePlayer();
                break;    
        }
        
        DashBuffer -= Time.deltaTime;
    }

    private void Patrol()
    {
        Vector2? playerPosition = mediator.GetPlayerPosition();
        if(playerPosition == null) return;

        if(DashBuffer <= 0)
        {
            // On peut faire deplacer le sonicWolf, on choisit une direction aléatoire.
            int randomDirection = UnityEngine.Random.Range(0, 2) * 2 - 1;
            body.linearVelocity = new Vector2(XSpeed * randomDirection, 0);

            // On relance le buffer
            DashBuffer = DashIntervals;
            return;
        }


        // Condition de transition -> ChasePlayer()
        float distance = Vector2.Distance(transform.position, playerPosition.Value);
        if(distance <= detectionRange) enemyState = EnemyStates.CHASING;
    }

    private void ChasePlayer()
    {   

        if(DashBuffer > 0) {
            enemyState = EnemyStates.PATROLLING;
        } else {
            Vector2? playerPosition = mediator.GetPlayerPosition();
            if(playerPosition == null) return;

            DashIntervals = chasingDashIntervals;
            if(DashBuffer <= 0)
            {
            
                // On peut refaire sauter le slime, dash dans la direction de FunkyFight se grosd chien.
                int randomDirection = (playerPosition.Value.x - transform.position.x <= 0) ? -1 : 1; 
                body.linearVelocity = new Vector2(XSpeed * randomDirection * 2.5f, YSpeed);

                // On relance le buffer
                DashBuffer = DashIntervals;
                return;
            }


            // Condition de transition -> Patrol()
            float distance = Vector2.Distance(transform.position, playerPosition.Value);
            if(distance > detectionRange) enemyState = EnemyStates.PATROLLING;
            }
        
    }
}
