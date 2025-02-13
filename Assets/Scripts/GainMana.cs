using System;
using UnityEngine;

public class GainMana : MonoBehaviour
{   
    [Header("Temps Respawn Settings")]
    [SerializeField] private float reparitionTemps = 5f;
    private float bufferReparitionTemps = 0f;


    [SerializeField] private GameMediator gameMediator;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        if(bufferReparitionTemps > 0)
        {
            
            SetOpacity(1f);
        }
        else
        {
            bufferReparitionTemps -= Time.fixedDeltaTime;
        }
    }

    void FixedUpdate()
    {
        if(bufferReparitionTemps>0) bufferReparitionTemps -= Time.fixedDeltaTime ;
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {   
        PlayerController playerController = collision.GetComponent<PlayerController>();

        if(playerController == null) return;
        if(bufferReparitionTemps > 0) 
        {   
            SetOpacity(0.5f);
            Debug.Log("bizarre");
        } 
        else 
        {
            
            Debug.Log("touché");
            int maxMana  = gameMediator.GetPlayerMaxMana();
            bufferReparitionTemps = reparitionTemps;
            gameMediator.SetPlayerMana(maxMana);
        }
    }


    private void SetOpacity(float alpha)
    {
        if (spriteRenderer != null)
            {
                Color newColor = spriteRenderer.color;
                newColor.a = alpha;
                spriteRenderer.color = newColor;
            }
    }
 
}
