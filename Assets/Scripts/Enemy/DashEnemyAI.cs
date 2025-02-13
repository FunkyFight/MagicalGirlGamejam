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

    [Header("Chasing Dash Settings")]
    [SerializeField] private float chasingDashCooldown = 2f; // Cooldown de 2 secondes entre les dashes en mode CHASING
    [SerializeField] private float firstChasingDashCooldown = 0.5f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashSpeedMultiplier = 5f;
    private bool isDashing = false;
    private float dashTimeRemaining = 0f;
    private float firstDashTimeRemaining = 0f;

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
                firstDashTimeRemaining = firstChasingDashCooldown;
                break;
            
            case EnemyStates.CHASING:
                if(firstDashTimeRemaining > 0f)
                {
                    firstDashTimeRemaining -= Time.deltaTime;
                }
                else
                {
                    // Une fois le délai écoulé, on appelle ChasePlayer() pour tenter le dash
                    ChasePlayer();
                }
                break;    
        }
        
        DashBuffer -= Time.deltaTime;

        if (isDashing)
        {
            dashTimeRemaining -= Time.deltaTime;
            if (dashTimeRemaining <= 0f)
            {
                isDashing = false;
                // Fin du dash : on peut par exemple réinitialiser la vélocité horizontale
                body.linearVelocity = new Vector2(0, body.linearVelocity.y);
            }
        }
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
        }


        // Condition de transition -> ChasePlayer()
        float distance = Vector2.Distance(transform.position, playerPosition.Value);
        if(distance <= detectionRange - 3) enemyState = EnemyStates.CHASING;
    }

    private void ChasePlayer()
    {   

        
        Vector2? playerPosition = mediator.GetPlayerPosition();
        if(playerPosition == null) return;
        

        // Condition de transition -> Patrol()
        float distance = Vector2.Distance(transform.position, playerPosition.Value);
        if(distance > detectionRange - 3) 
        {
            enemyState = EnemyStates.PATROLLING;
            return;
        }


        if(DashBuffer <= 0)
        {
            
            // On peut refaire sauter le slime, dash dans la direction de FunkyFight se grosd chien.
            int randomDirection = (playerPosition.Value.x - transform.position.x <= 0) ? -1 : 1;
            isDashing = true;
            dashTimeRemaining = dashDuration;
            body.linearVelocity = new Vector2(XSpeed * randomDirection * dashSpeedMultiplier, YSpeed);

            // On relance le buffer
            DashBuffer = chasingDashCooldown;
        }
    }
}
