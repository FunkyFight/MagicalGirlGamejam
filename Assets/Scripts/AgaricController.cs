using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

public class AgaricController : MonoBehaviour
{

    [SerializeField] public bool CanMove {get; set;} = true;
    [SerializeField] public bool CanJump {get; set;} = true;
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpHeight = 14f;
    private bool isJumping = false;
    private float lastDirection = 1;

    [Header("Collision Settings")]
    [SerializeField] private List<LayerMask> groundLayers;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;

    [Header("Coyote Jump Settings")]
    [SerializeField] private int coyoteTime = 7; // Frames que le joueur dispose pour faire un saut quand il est tombé
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
    [SerializeField] private float jumpCutMultiplier = 0.5f; // Multiplie la vélocité verticale lors d'un relâchement précoce

     /*saut Champi variable*/
    [Header("SautChampi Settings")]
    [SerializeField] private float highJumpChampi = 2f;
    //[SerializeField] private float delayBeforeJumpChamp = 0.2f;  
    private bool isChampiDelayActive = false;
    //test pour changer la couleur
    private SpriteRenderer spriteRenderer;
    private Color initialColor;

    [Header("PlaneChampi Settings")]
    [SerializeField] private float planeChampiGravityScale = 0.6f;
    [SerializeField] private float planeChampiHorizontalSpeed = 5f;
    private bool isPlane = false;
     private float graviteNormal = 4f;

    // Components
    private Rigidbody2D rbody;

    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialColor = spriteRenderer.color;
    }

    public void Move(InputAction.CallbackContext context)
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

    public void Jump(InputAction.CallbackContext context)
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
                rbody.gravityScale = 4f; // sert a regler le bug avec halfjump qui divise par 2 un peu près et le planeChampi
                return true;
            }
        }
        
        return false;
    }

    /* test abality 1*/

    public void SautChampi(InputAction.CallbackContext context) 
    {   
        if (!CanMove) return;
        if (!CanJump) return;
        if (context.phase == InputActionPhase.Started)
        {   
            if (IsGrounded()) {
                rbody.linearVelocity = new Vector2(rbody.linearVelocity.x, jumpHeight / 2);
            }
            isChampiDelayActive = true;
            spriteRenderer.color = Color.red;
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
        spriteRenderer.color = initialColor;
        CanMove = true;
        isChampiDelayActive = false;
    }

    

    /*test abality 2*/
    public void PlaneChampi(InputAction.CallbackContext context)
    {   
        if (context.phase == InputActionPhase.Started)
        {
            if (!IsGrounded()) 
            {   
                isPlane = true;
                spriteRenderer.color = Color.blue;
                rbody.linearVelocity = new Vector2(rbody.linearVelocity.x/1.2f, 0);
                rbody.gravityScale = planeChampiGravityScale;
                moveSpeed = 2f;
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
        spriteRenderer.color = initialColor;
           
        if (isPlane)
        {
            rbody.linearVelocity = new Vector2(rbody.linearVelocity.x * moveSpeed, rbody.linearVelocity.y);
            isPlane = false;
            rbody.gravityScale = graviteNormal;
            moveSpeed = 10f;
            
        }
        if (rbody.gravityScale <4) // sert a regler le bug avec halfjump qui divise par 2 un peu près
        {
            rbody.gravityScale = 4f;
        }
            
    }
}