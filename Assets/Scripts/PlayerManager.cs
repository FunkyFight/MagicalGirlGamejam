using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [Header("Game Mediator")]
    [SerializeField] private GameMediator mediator;


    [Header("Character Transformations")]
    [SerializeField] private List<GameObject> transformationPrefabs;
    [SerializeField] private int defaultTransformationIndex = 0;
    [SerializeField] private int currentTransformationIndex = -1;
    [SerializeField] private int leftSlotTransformationIndex = -1;
    [SerializeField] private int rightSlotTransformationIndex = -1;
    public GameObject currentTransformationGameObject {private set; get;}




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

        // Sauvegarde des données de mouvement
        // Bug : le joueur pourra avoir un double jump en spammant très vite le saut pour coyote -> sauvegarder les données de coyote etc
        Vector2 lastPlayerPos = currentTransformationGameObject.transform.position;
        Vector2 lastVelocity = currentTransformationGameObject.GetComponent<Rigidbody2D>().linearVelocity;

        Destroy(currentTransformationGameObject);

        currentTransformationGameObject = Instantiate(transformationPrefabs[characterIndex], transform);
        currentTransformationGameObject.transform.position = lastPlayerPos;
        currentTransformationGameObject.GetComponent<Rigidbody2D>().linearVelocity = lastVelocity;

        // Redéfinition des slots
        leftSlotTransformationIndex = (leftSlotTransformationIndex == characterIndex) ? currentTransformationIndex : leftSlotTransformationIndex;
        rightSlotTransformationIndex = (rightSlotTransformationIndex == characterIndex) ? currentTransformationIndex : rightSlotTransformationIndex;
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
}
