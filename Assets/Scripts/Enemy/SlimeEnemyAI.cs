using System;
using Unity.VisualScripting;
using UnityEngine;

public class SlimeEnemyAI : EnemyAI
{
    enum EnemyStates {PATROLLING, CHASING}

    [SerializeField] private GameMediator mediator;
    
    [SerializeField] private float jumpXSpeed = 10f;
    [SerializeField] private float jumpYSpeed = 10f;
    [SerializeField] private float jumpsIntervals = 1f; // En secondes
    private float jumpBuffer = 0;

    [Header("Jump Intervals Settings")]
    [SerializeField] private float patrollingJumpIntervals = 1f;
    [SerializeField] private float chasingJumpIntervals = 0.5f;

    private EnemyStates enemyState = EnemyStates.PATROLLING;
    private Rigidbody2D body;
    

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
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

        
        jumpBuffer -= Time.deltaTime;
    }
    

    private void Patrol()
    {
        Vector2? playerPosition = mediator.GetPlayerPosition();
        if(playerPosition == null) return;

        jumpsIntervals = patrollingJumpIntervals;
        if(jumpBuffer <= 0)
        {
            // On peut refaire sauter le slime, on choisit une direction aléatoire.
            int randomDirection = UnityEngine.Random.Range(0, 2) * 2 - 1;
            body.linearVelocity = new Vector2(jumpXSpeed * randomDirection, jumpYSpeed);

            // On relance le buffer
            jumpBuffer = jumpsIntervals;
            return;
        }


        // Condition de transition -> ChasePlayer()
        float distance = Vector2.Distance(transform.position, playerPosition.Value);
        if(distance <= detectionRange) enemyState = EnemyStates.CHASING;

    }

    private void ChasePlayer()
    {
        Vector2? playerPosition = mediator.GetPlayerPosition();
        if(playerPosition == null) return;

        jumpsIntervals = chasingJumpIntervals;
        if(jumpBuffer <= 0)
        {
            
            // On peut refaire sauter le slime, on choisit une direction aléatoire.
            int randomDirection = (playerPosition.Value.x - transform.position.x <= 0) ? -1 : 1; 
            body.linearVelocity = new Vector2(jumpXSpeed * randomDirection, jumpYSpeed);

            // On relance le buffer
            jumpBuffer = jumpsIntervals;
            return;
        }


        // Condition de transition -> Patrol()
        float distance = Vector2.Distance(transform.position, playerPosition.Value);
        if(distance > detectionRange) enemyState = EnemyStates.PATROLLING;
    }
}
