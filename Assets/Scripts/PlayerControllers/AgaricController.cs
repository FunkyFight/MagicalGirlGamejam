using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

public class AgaricController : PlayerController
{

    [SerializeField] public bool CanJump {get; set;} = true;

    /*saut Champi variable*/
    [Header("SautChampi Settings")]
    [SerializeField] private float highJumpChampi = 2f;
    [SerializeField] private float JumpChampiCooldown = 1f; // Cooldown du jump en secondes
    private float JumpChampiCooldownRemaining = 0f; 
    private bool isChampiDelayActive = false;
    
    

    [Header("PlaneChampi Settings")]
    [SerializeField] private float planeChampiGravityScale = 0.6f;
    [SerializeField] private float planeChampiHorizontalSpeed = 5f;
    private bool isPlane = false;
    private float graviteNormal;
    private float normalMoveSpeed;
    

    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        normalMoveSpeed = moveSpeed;
        graviteNormal =rbody.gravityScale;
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
        if(!CanJump) return;
        if (isChampiDelayActive) return;

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
        if (JumpChampiCooldownRemaining > 0f && IsGrounded()) 
        {
            JumpChampiCooldownRemaining -= Time.fixedDeltaTime;
        }
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
                rbody.gravityScale = graviteNormal; // sert a regler le bug avec halfjump qui divise par 2 un peu près et le planeChampi; je sais que quand tu vas lire ce message tu vas vouloir me buté
                return true;
            }
        }
        
        return false;
    }

    /* test abality 1*/

    public override void Ability1(InputAction.CallbackContext context) 
    {   
        if (!CanMove) return;
        if (!CanJump) return;
        if (JumpChampiCooldownRemaining > 0f) 
        {
            return;
        }
        JumpChampiCooldownRemaining = JumpChampiCooldown;
        if (context.phase == InputActionPhase.Started)
        {   
            if (IsGrounded()) {
                rbody.linearVelocity = new Vector2(rbody.linearVelocity.x, jumpHeight / 2);
            }
            isChampiDelayActive = true;
            CanMove = false;
            StartCoroutine(DelaySautChampi());
        }
    }
    
    private IEnumerator DelaySautChampi()
    {
        
        while (IsGrounded())
        {
            yield return null;
        }
        while (!IsGrounded())
        {
            yield return null;
        }
        rbody.linearVelocity = new Vector2(rbody.linearVelocity.x, jumpHeight * highJumpChampi);
        isJumping = true;
        CanMove = true;
        isChampiDelayActive = false;
    }

    

    /*test abality 2*/
    public override void Ability2(InputAction.CallbackContext context)
    {   
        if (context.phase == InputActionPhase.Started)
        {
            if (!IsGrounded()) 
            {   
                moveSpeed = planeChampiHorizontalSpeed;
                isPlane = true;
                rbody.linearVelocity = new Vector2(rbody.linearVelocity.x/1.7f, 0);
                rbody.gravityScale = planeChampiGravityScale;
                StartCoroutine(CoroutinePlaneChampi());
            }
        } 
    }
    private IEnumerator CoroutinePlaneChampi()
    {
         while (!IsGrounded())
        {
            yield return null;
        }
        
        moveSpeed = normalMoveSpeed;
        if (isPlane)
        {
            rbody.linearVelocity = new Vector2(rbody.linearVelocity.x * 1.7f, rbody.linearVelocity.y);
            isPlane = false;
            rbody.gravityScale = graviteNormal;

        }
        if (rbody.gravityScale <4) // sert a regler le bug avec halfjump qui divise par 2 un peu près
        {
            rbody.gravityScale = 4f;
        }  
    }
}