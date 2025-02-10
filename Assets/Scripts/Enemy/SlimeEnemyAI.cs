using System;
using UnityEngine;

public class SlimeEnemyAI : MonoBehaviour
{
    enum EnemyStates {PATROLLING, CHASING}

    [SerializeField] private GameMediator mediator;

    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 10f;
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
        float distance = Vector2.Distance(transform.position, mediator.GetPlayerPosition());
        if(distance <= detectionRange) enemyState = EnemyStates.CHASING;

    }

    private void ChasePlayer()
    {
        
        jumpsIntervals = chasingJumpIntervals;
        if(jumpBuffer <= 0)
        {
            // On peut refaire sauter le slime, on choisit une direction aléatoire.
            int randomDirection = (mediator.GetPlayerPosition().x - transform.position.x <= 0) ? -1 : 1; 
            body.linearVelocity = new Vector2(jumpXSpeed * randomDirection, jumpYSpeed);

            // On relance le buffer
            jumpBuffer = jumpsIntervals;
            return;
        }


        // Condition de transition -> Patrol()
        float distance = Vector2.Distance(transform.position, mediator.GetPlayerPosition());
        if(distance > detectionRange) enemyState = EnemyStates.PATROLLING;
    }
}
