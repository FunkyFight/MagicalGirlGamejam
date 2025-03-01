using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] public bool CanMove = true;
    [Header("Movement Settings")]
    [SerializeField] public float moveSpeed = 10f;
    [SerializeField] protected float jumpHeight = 14f;
    protected bool isJumping = false;
    protected float lastDirection = 1;

    [Header("Collision Settings")]
    [SerializeField] protected List<LayerMask> groundLayers;
    [SerializeField] protected Transform groundCheck;

    [Header("Coyote Jump Settings")]
    [SerializeField] protected int coyoteTime = 7; // Frames que le joueur dispose pour faire un saut quand il est tombé
    protected int coyoteBuffer = 0;
    protected bool canActivateCoyote = true;

    [Header("Jump Buffering Settings")]
    [SerializeField] protected int jumpBufferTime = 7;
    protected int jumpBuffer = 0;

    [Header("Half Gravity Jump Peak Settings")]
    [SerializeField] protected int maxHalfTime = 5;
    [SerializeField] protected float halfGravityCoef = 0.5f;
    protected int halfTime = 0;
    protected float halfEpsilon = 0.3f;

    [Header("Variable Jump Height Settings")]
    [SerializeField] protected float jumpCutMultiplier = 0.5f; // Multiplie la vélocité verticale lors d'un relâchement précoce

    private float lastXVelocity = 0;
    private float lastYVelocity = 0;

    // Components
    protected Rigidbody2D rbody;

    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
    }

    public virtual void Move(InputAction.CallbackContext context)
    {
        if(!CanMove) return;

        Vector2 direction = context.ReadValue<Vector2>();

        if (Mathf.Abs(direction.x) > 0.1f)
        {
            lastDirection = Mathf.Sign(direction.x);
        }

        rbody.linearVelocityX = direction.x * moveSpeed;

        transform.localScale = new Vector3(lastDirection, 1, 1);
    }

    public virtual void Jump(InputAction.CallbackContext context)
    {
        if(!CanMove) return;

        // Début du saut
        if (context.phase == InputActionPhase.Started)
        {
            bool canJump = IsGrounded() || coyoteBuffer > 0;
            if (!canJump)
            {
                jumpBuffer = jumpBufferTime;
                return;
            }

            _Jump();
        }
        // Relâchement du bouton de saut pour un variable jump height
        else if (context.phase == InputActionPhase.Canceled)
        {
            if (rbody.linearVelocityY > 0)
            {
                rbody.linearVelocityY *= jumpCutMultiplier;
            }
        }
    }

    void _Jump()
    {
        rbody.linearVelocityY = jumpHeight;
        isJumping = true;
        coyoteBuffer = 0; // Réinitialise le coyote buffer après un saut
        canActivateCoyote = false;
    }

    public virtual void FixedUpdate()
    {
        CoyoteJump();
        JumpBuffer();
        HalfJumpGravityPeak();

        if (IsGrounded() && rbody.linearVelocityY == 0)
        {
            isJumping = false;
            ResetCoyoteJump();
        }

        if(!IsGrounded())
        {
            lastXVelocity = rbody.linearVelocityX;
            lastYVelocity = rbody.linearVelocityY;
        }
    }

    void OnCollisionEnter2D(Collision2D collision2D)
    {
        if(collision2D.gameObject.CompareTag("Bouncepad"))
        {
            rbody.linearVelocity = new Vector2(lastXVelocity, -lastYVelocity);
        }
    }

    void JumpBuffer()
    {
        if (jumpBuffer > 0)
        {
            if (IsGrounded())
            {
                jumpBuffer = 0;
                _Jump();
                return;
            }
            
            jumpBuffer--;
        }
    }

    void CoyoteJump()
    {
        if (IsGrounded()) return;
        if (isJumping)
        {
            return;
        }

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
        if(!IsGrounded())
        {
            if(halfTime > 0)
            {
                halfTime--;
                if(halfTime == 0)
                {
                    rbody.gravityScale = rbody.gravityScale / halfGravityCoef;
                }
                return;
            }

            if(Mathf.Abs(rbody.linearVelocityY) < halfEpsilon)
            {
                if(halfTime > 0) return;
                // Starting half jump gravity peak
                halfTime = maxHalfTime;
                rbody.gravityScale = rbody.gravityScale * halfGravityCoef;
                return;
            }
        } else {
            if(halfTime <= 0) return;
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
    * Fonction utilitaires
    */
    private bool IsGrounded()
    {
        Vector2 position = groundCheck.position;
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

    public virtual void Ability1(InputAction.CallbackContext context){}
    public virtual void Ability2(InputAction.CallbackContext context){}
}