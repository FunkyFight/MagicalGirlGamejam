using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 5f;
    private bool isJumping = false;

    [Header("Collision Settings")]
    [SerializeField] private List<LayerMask> groundLayers;
    [SerializeField] private Transform groundCheck;

    [Header("Coyote Jump Settings")]
    [SerializeField] private int coyoteTime = 7; // Frames que le joueur dispose pour faire un saut quand il est tombé
    private int coyoteBuffer = 0;
    private bool canActivateCoyote = true;

    [Header("Jump Buffering Settings")]
    [SerializeField] private int jumpBufferTime = 7;
    private int jumpBuffer = 0;

    [Header("Half Gravity Jump Peak Settings")]
    [SerializeField] private int maxHalfTime = 7;
    [SerializeField] private float halfGravityCoef = 0.5f;
    private int halfTime = 0;
    private float halfEpsilon = 0.1f;

    // Components
    private Rigidbody2D rbody;

    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
    }

    public void Move(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        rbody.linearVelocityX = direction.x * moveSpeed;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Started) return;

        bool canJump = context.ReadValueAsButton() && (IsGrounded() || coyoteBuffer > 0);
        if (!canJump)
        {
            jumpBuffer = jumpBufferTime;
            return;
        }

        _Jump();
    }

    void _Jump()
    {
        rbody.linearVelocityY = jumpHeight;
        isJumping = true;
        
        coyoteBuffer = 0; // Reset coyote buffer after jump
        canActivateCoyote = false;
    }

    void FixedUpdate()
    {
        CoyoteJump();
        JumpBuffer();
        HalfJumpGravityPeak();

        if (IsGrounded() && rbody.linearVelocityY == 0)
        {
            isJumping = false;
            ResetCoyoteJump();
        }
    }

    

    /*
    * Special Movements Functions
    */

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

    void JumpBuffer()
    {
        if(jumpBuffer > 0)
        {
            if(IsGrounded())
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
    
}
