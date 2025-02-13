using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MagicalGirlController : PlayerController
{
    [Header("Attack Speed")]
    [SerializeField] private float attackSpeed = 16f;

    [Header("Attack Damage")]
    [SerializeField] private int attackDmg = 20;

    [Header("Arrow")]
    [SerializeField] private GameObject arrow;

    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
    }

    public override void Move(InputAction.CallbackContext context)
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

    public override void Jump(InputAction.CallbackContext context)
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

    public void Attack(InputAction.CallbackContext context)
    {
        if(context.phase != InputActionPhase.Started) return;
        
        GameObject arrowObj = Instantiate(arrow);
        arrowObj.transform.position = new Vector2(transform.position.x + 1f * transform.localScale.x, transform.position.y);
        ArrowProjectile arrowComp = arrowObj.GetComponent<ArrowProjectile>();
        arrowComp.Direction = Mathf.FloorToInt(transform.localScale.x);
    }
}