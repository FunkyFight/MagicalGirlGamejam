using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirianController : MonoBehaviour
{
    [SerializeField] public bool CanMove {get; set;} = true;
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpHeight = 14f;
    [SerializeField] private float defaultGravityScale = 4f;
    private Vector2 moveInput;

    private bool isJumping = false;
    private float lastDirection = 1;

    [Header("Collision Settings")]
    [SerializeField] private List<LayerMask> groundLayers;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;

    [Header("Coyote Jump Settings")]
    [SerializeField] private int coyoteTime = 7; // Frames pendant lesquelles le joueur peut sauter après avoir quitté le sol
    private int coyoteBuffer = 0;
    private bool canActivateCoyote = true;

    [Header("Jump Buffering Settings")]
    [SerializeField] private int jumpBufferTime = 7;
    private int jumpBuffer = 0;

    [Header("Half Gravity Jump Peak Settings")]
    [SerializeField] private int maxHalfTime = 5;
    [SerializeField] private float halfGravityCoef = 0.5f;
    private int halfTime = 0;
    private float halfEpsilon = 0.3f;

    [Header("Variable Jump Height Settings")]
    [SerializeField] private float jumpCutMultiplier = 0.5f; // Réduit la vélocité verticale en cas de relâchement précoce du bouton de saut

    [Header("Wall Jump Settings")]
    [SerializeField] private float xStrength = 2f; // Force horizontale appliquée lors d'un wall jump
    private bool isStickedOnWall = false;
    private float lastGravityScale;
    [SerializeField] private int wallJumpGraceFrames = 10; // Frames pendant lesquelles le wall jump reste valide après avoir quitté le mur
    private int wallJumpGraceCounter = 0;
    [SerializeField] private int wallJumpCooldownFrames = 10; // Frames pendant lesquelles le wall stick est désactivé
    private int wallJumpCooldownCounter = 0;

    [Header("Dash Settings")]
    [SerializeField] private float dashStrength = 20f;
    [SerializeField] private float dashDuration = 0.2f; // Durée du dash en secondes
    [SerializeField] private float dashCooldown = 0.5f; // Cooldown du dash en secondes
    [SerializeField] private float dashFriction = 0.95f; // Friction applied during dash
    private float dashTimeRemaining = 0f;
    private float dashCooldownRemaining = 0f;
    private bool isDashing = false;

    // Composant
    private Rigidbody2D rbody;

    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        rbody.gravityScale = defaultGravityScale;
        lastGravityScale = defaultGravityScale;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if(!CanMove) return;

        // Si le dash est actif, on ignore les inputs de déplacement pour conserver la vélocité du dash
        if (isDashing)
            return;

        moveInput = context.ReadValue<Vector2>();

        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            lastDirection = Mathf.Sign(moveInput.x);
        }

        // On conserve la composante verticale de la vélocité actuelle
        rbody.linearVelocity = new Vector2(moveInput.x * moveSpeed, rbody.linearVelocity.y);
        transform.localScale = new Vector3(lastDirection, 1, 1);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if(!CanMove) return;

        // Début du saut
        if (context.phase == InputActionPhase.Started)
        {
            // Le saut est autorisé si le joueur est au sol, dans le délai du coyote jump ou en mode wall-stick
            bool canJump = IsGrounded() || coyoteBuffer > 0 || isStickedOnWall;
            if (!canJump)
            {
                jumpBuffer = jumpBufferTime;
                return;
            }
            _Jump();
        }
        // Réduction de la vélocité verticale pour un saut plus court si le bouton est relâché tôt
        else if (context.phase == InputActionPhase.Canceled)
        {
            if (rbody.linearVelocity.y > 0)
            {
                rbody.linearVelocity = new Vector2(rbody.linearVelocity.x, rbody.linearVelocity.y * jumpCutMultiplier);
            }
        }
    }

    void _Jump()
    {
        
        if (!isStickedOnWall)
        {
            rbody.linearVelocity = new Vector2(rbody.linearVelocity.x, jumpHeight);
        }
        else
        {
            rbody.linearVelocity = Vector2.zero;
            rbody.gravityScale = defaultGravityScale;
            
            // Détermine la direction de base en fonction de la position relative du mur
            float baseDir = (transform.position.x < wallCheck.position.x) ? -1f : 1f;
            
            // Si le joueur donne une input horizontale, on l'utilise pour influencer le saut
            float inputDir = Mathf.Abs(moveInput.x) > 0.1f ? Mathf.Sign(moveInput.x) : baseDir;
            
            rbody.linearVelocity = new Vector2(xStrength * inputDir, jumpHeight);
            isStickedOnWall = false;
            wallJumpGraceCounter = 0;
            wallJumpCooldownCounter = wallJumpCooldownFrames;
        }

        isJumping = true;
        coyoteBuffer = 0;
        canActivateCoyote = false;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if(!CanMove) return;

        if (!context.started)
            return;

        // Si le dash est en cooldown, on ne fait rien
        if (dashCooldownRemaining > 0f)
        {
            return;
        }

        // Début du dash
        isDashing = true;
        dashTimeRemaining = dashDuration;
        dashCooldownRemaining = dashCooldown;

        // On désactive temporairement la gravité pour un dash purement horizontal
        rbody.gravityScale = 0;

        // Applique une vélocité fixe dans la dernière direction connue
        rbody.linearVelocity = new Vector2(lastDirection * dashStrength, 0);
    }

    void FixedUpdate()
    {
        // Gestion du dash
        if (isDashing)
        {
            dashTimeRemaining -= Time.fixedDeltaTime;
            rbody.linearVelocity = new Vector2(rbody.linearVelocity.x * dashFriction, rbody.linearVelocity.y); // Apply dash friction
            if (dashTimeRemaining <= 0f)
            {
                isDashing = false;
                // Restauration de la gravité
                rbody.gravityScale = defaultGravityScale;
                rbody.linearVelocityX = 0;
            }
        }
        if (dashCooldownRemaining > 0f)
        {
            dashCooldownRemaining -= Time.fixedDeltaTime;
        }

        CoyoteJump();
        JumpBuffer();
        HalfJumpGravityPeak();
        WallJump();

        // Gestion de la période de grâce pour le wall jump
        if (isStickedOnWall)
        {
            if (IsWalled())
            {
                wallJumpGraceCounter = wallJumpGraceFrames;
            }
            else
            {
                rbody.gravityScale = defaultGravityScale;
                if (wallJumpGraceCounter > 0)
                {
                    wallJumpGraceCounter--;
                }
                else
                {
                    isStickedOnWall = false;
                }
            }
        }

        // Réinitialisation du saut à l'atterrissage
        if (IsGrounded() && Mathf.Approximately(rbody.linearVelocity.y, 0))
        {
            isJumping = false;
            ResetCoyoteJump();
            rbody.gravityScale = defaultGravityScale;
        }
    }

    /// <summary>
    /// Si un saut a été bufferisé, on le déclenche dès que le joueur est au sol ou en mode wall-stick.
    /// </summary>
    void JumpBuffer()
    {
        if (jumpBuffer > 0)
        {
            if (IsGrounded() || isStickedOnWall)
            {
                jumpBuffer = 0;
                _Jump();
                return;
            }
            jumpBuffer--;
        }
    }

    /// <summary>
    /// Active le mode wall-stick si les conditions sont remplies.
    /// </summary>
    void WallJump()
    {
        if (wallJumpCooldownCounter > 0)
            wallJumpCooldownCounter--;

        if (isStickedOnWall || wallJumpCooldownCounter > 0)
            return;

        bool wallJumpConditions = !IsGrounded() && IsWalled();
        if (wallJumpConditions)
        {
            lastGravityScale = rbody.gravityScale;
            // On réduit la gravité pour simuler un slide le long du mur
            rbody.gravityScale = defaultGravityScale * 0.3f;
            rbody.linearVelocity = Vector2.zero;
            isStickedOnWall = true;
            wallJumpGraceCounter = wallJumpGraceFrames;
        }
    }

    void CoyoteJump()
    {
        if (IsGrounded()) return;
        if (isJumping) return;

        if (coyoteBuffer == 0)
        {
            if (canActivateCoyote)
            {
                coyoteBuffer = coyoteTime;
                canActivateCoyote = false;
            }
        }
        else
        {
            coyoteBuffer--;
        }
    }

    void HalfJumpGravityPeak()
    {
        if (isStickedOnWall)
            return;

        if (!IsGrounded())
        {
            if (halfTime > 0)
            {
                halfTime--;
                if (halfTime == 0)
                {
                    rbody.gravityScale = rbody.gravityScale / halfGravityCoef;
                }
                return;
            }

            if (Mathf.Abs(rbody.linearVelocity.y) < halfEpsilon)
            {
                if (halfTime > 0) return;
                halfTime = maxHalfTime;
                rbody.gravityScale = rbody.gravityScale * halfGravityCoef;
                return;
            }
        }
        else
        {
            if (halfTime <= 0) return;
            halfTime = 0;
            rbody.gravityScale = rbody.gravityScale / halfGravityCoef;
        }
    }

    void ResetCoyoteJump()
    {
        canActivateCoyote = true;
        coyoteBuffer = 0;
    }

    /*
     * Fonctions utilitaires
     */
    private bool CheckCollision(Transform checkTransform)
    {
        Vector2 position = checkTransform.position;
        float radius = 0.2f;

        foreach (LayerMask layer in groundLayers)
        {
            Collider2D collider2D = Physics2D.OverlapCircle(position, radius, layer);
            if (collider2D != null)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsGrounded()
    {
        return CheckCollision(groundCheck);
    }

    private bool IsWalled()
    {
        return CheckCollision(wallCheck);
    }
}
