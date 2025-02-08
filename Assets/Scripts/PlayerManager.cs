using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int health = 3;

    [SerializeField] private int maxMana = 100;
    [SerializeField] private int mana = 100;


    [Header("Game Mediator")]
    [SerializeField] private GameMediator mediator;

    private ParticleSystem ps;

    [Header("Character Transformations")]
    [SerializeField] private List<GameObject> transformationPrefabs;
    [SerializeField] private int defaultTransformationIndex = 0;
    [SerializeField] private int currentTransformationIndex = -1;
    [SerializeField] private int leftSlotTransformationIndex = -1;
    [SerializeField] private int rightSlotTransformationIndex = -1;
    public GameObject currentTransformationGameObject {private set; get;}


    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (Keyboard.current.numpad1Key.wasPressedThisFrame || Gamepad.current.leftShoulder.wasPressedThisFrame)
        {
            OnLeftShoulder();
        }

        if (Keyboard.current.numpad2Key.wasPressedThisFrame || Gamepad.current.rightShoulder.wasPressedThisFrame)
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
        if(characterIndex == -1) 
        {
            return false;
        }

        if(mana <= 0) return false; 

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

        // Redéfinition des slots
        if(currentTransformationIndex != 0) 
        {
            leftSlotTransformationIndex = (leftSlotTransformationIndex == characterIndex) ? currentTransformationIndex : leftSlotTransformationIndex;
            rightSlotTransformationIndex = (rightSlotTransformationIndex == characterIndex) ? currentTransformationIndex : rightSlotTransformationIndex;
        }
        currentTransformationIndex = characterIndex;

        return true;
    }

    public void OnLeftShoulder()
    {
        mediator.SwitchPlayerMagicalGirlTransformation(leftSlotTransformationIndex);
    }

    public void OnRightShoulder()
    {
        mediator.SwitchPlayerMagicalGirlTransformation(rightSlotTransformationIndex);
    }

    public void Damage()
    {
        if (currentTransformationIndex == 0) 
            health--;
        else 
            mana = Mathf.FloorToInt(mana * 0.75f);

        if(mana == 0) mediator.SwitchPlayerMagicalGirlTransformation(0);
        if(health == 0) UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private IEnumerator<object> playSwitchParticles(float duration)
    {
        ps.Play();
        yield return new WaitForSeconds(duration);
        ps.Stop();
    }
}
