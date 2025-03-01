using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int health = 3;

    [SerializeField] private int maxMana = 100;
    [SerializeField] private int mana = 100;


    [Header("Game Mediator")]
    [SerializeField] private GameMediator mediator;
    
    [SerializeField] public Light2D spotlight {get; private set;}

    private ParticleSystem ps;

    [Header("Character Transformations")]
    [SerializeField] private List<GameObject> transformationPrefabs;
    [SerializeField] private int defaultTransformationIndex = 0;
    [SerializeField] private int currentTransformationIndex = -1;
    [SerializeField] private int leftSlotTransformationIndex = -1;
    [SerializeField] private int rightSlotTransformationIndex = -1;
    public GameObject currentTransformationGameObject {private set; get;}


    private bool isAlive = true;


    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        bool isLeftShoulderPressed = (Gamepad.current == null) ? false : Gamepad.current.leftShoulder.wasPressedThisFrame;
        bool isRightShoulderPressed = (Gamepad.current == null) ? false : Gamepad.current.rightShoulder.wasPressedThisFrame;

        if (Keyboard.current.numpad1Key.wasPressedThisFrame || isLeftShoulderPressed)
        {
            OnLeftShoulder();
        }

        if (Keyboard.current.numpad2Key.wasPressedThisFrame || isRightShoulderPressed)
        {
            OnRightShoulder();
        }
    }


    /// <summary>
    /// Change le personnage en perso par défaut
    /// </summary>
    /// <returns>false si échec, true sinon</returns>
    public bool SwitchToDefaultCharacter()
    {
        int characterIndex = defaultTransformationIndex;

        if(characterIndex == -1) return false;

        if(currentTransformationGameObject != null)
        {
            Destroy(currentTransformationGameObject);
        }

        currentTransformationGameObject = Instantiate(transformationPrefabs[characterIndex], transform);
        currentTransformationGameObject.GetComponent<PlayerDamageController>().playerManager = this;
        currentTransformationGameObject.transform.position = transform.position;

        spotlight = currentTransformationGameObject.GetComponentInChildren<Light2D>();

        // Redéfinition des slots
        leftSlotTransformationIndex = (leftSlotTransformationIndex == characterIndex) ? currentTransformationIndex : leftSlotTransformationIndex;
        rightSlotTransformationIndex = (rightSlotTransformationIndex == characterIndex) ? currentTransformationIndex : rightSlotTransformationIndex;
        currentTransformationIndex = characterIndex;

        return true;
    }
    

    /// <summary>
    /// Change le personnage
    /// </summary>
    /// <param name="characterIndex">Index du préfab de transformationPrefabs</param>
    /// <returns>false si échec, true sinon</returns>
    public bool SwitchToCharacter(int characterIndex)
    {
        if(!isAlive) return false;
        if(characterIndex == -1) 
        {
            return false;
        }

        if(mana <= 0 && characterIndex > 0) return false; 

        // Si on est en forme humaine alors peut importe la transformation on repasse d'abord en magical girl
        if(currentTransformationIndex == 0) characterIndex = 1;

        // Sauvegarde des données de mouvement
        // Bug : le joueur pourra avoir un double jump en spammant très vite le saut pour coyote -> sauvegarder les données de coyote etc
        Vector2 lastPlayerPos = currentTransformationGameObject.transform.position;
        Vector2 lastVelocity = currentTransformationGameObject.GetComponent<Rigidbody2D>().linearVelocity;

        Destroy(currentTransformationGameObject);

        // Play particles
        transform.position = lastPlayerPos; // Teleport this gameobject to lastPlayerPos
        
        StartCoroutine(playSwitchParticles(0.1f));

        

        currentTransformationGameObject = Instantiate(transformationPrefabs[characterIndex], transform);

        currentTransformationGameObject.GetComponent<PlayerDamageController>().playerManager = this;

        // Animation blanc -> couleur pour la transition
        SpriteRenderer spriteRenderer = currentTransformationGameObject.GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.white;

        spriteRenderer.DOColor(originalColor, 1f).SetEase(Ease.OutBack);

        currentTransformationGameObject.transform.position = lastPlayerPos;
        currentTransformationGameObject.GetComponent<Rigidbody2D>().linearVelocity = lastVelocity;
        spotlight = currentTransformationGameObject.GetComponentInChildren<Light2D>();

        // Redéfinition des slots
        if(currentTransformationIndex != 0) 
        {
            leftSlotTransformationIndex = (leftSlotTransformationIndex == characterIndex) ? currentTransformationIndex : leftSlotTransformationIndex;
            rightSlotTransformationIndex = (rightSlotTransformationIndex == characterIndex) ? currentTransformationIndex : rightSlotTransformationIndex;
        }
        currentTransformationIndex = characterIndex;

        return true;
    }

    public void Kill()
    {
        ps.startColor = Color.red;
        ps.transform.position = currentTransformationGameObject.transform.position;
        StartCoroutine(playSwitchParticles(0.1f));
        Destroy(currentTransformationGameObject);
        isAlive = false;
    }

    public void OnLeftShoulder()
    {
        mediator.SwitchPlayerMagicalGirlTransformation(leftSlotTransformationIndex);
    }

    public void OnRightShoulder()
    {
        mediator.SwitchPlayerMagicalGirlTransformation(rightSlotTransformationIndex);
    }

    private bool isInvincible = false;
    private float invincibilityDuration = 1.5f; // Duration of invincibility in seconds

    public void Damage()
    {
        if (isInvincible) return;

        
        StartCoroutine(ActivateInvincibilityFrames());
        if (currentTransformationIndex == 0) 
        {
            health--;
            if (health == 0)
            {
                Kill();
                return;
            }
        }
        else 
        {
            mana -= 25;
            if (mana <= 0)
            {
                mediator.SwitchPlayerMagicalGirlTransformation(0);
                return;
            }
        }

        StartCoroutine(FlickerRedOnDamage(currentTransformationGameObject, 1f, 0.1f));
    }

    private IEnumerator ActivateInvincibilityFrames()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    private IEnumerator<object> playSwitchParticles(float duration)
    {
        ps.Play();
        yield return new WaitForSeconds(duration);
        ps.Stop();
    }

    private IEnumerator FlickerRedOnDamage(GameObject entity, float flickerDuration, float flickerInterval)
    {
        SpriteRenderer spriteRenderer = entity.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer not found on the entity.");
            yield break;
        }

        float elapsedTime = 0f;
        Color originalColor = spriteRenderer.color;
        Color flickerColor = Color.red;

        while (elapsedTime < flickerDuration)
        {
            spriteRenderer.color = flickerColor;
            yield return new WaitForSeconds(flickerInterval);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flickerInterval);

            elapsedTime += flickerInterval * 2; // Account for both flicker and revert intervals
        }

        spriteRenderer.color = originalColor; // Ensure the color is reset to original
    }
}
