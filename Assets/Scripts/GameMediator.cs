using UnityEngine;

/*
* Gère absolument TOUTES les relations entre les différents gameobjects systèmes
*/
public class GameMediator : MonoBehaviour
{
    
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private MenuController menuController;

    
    void Start()
    {
        bool successfullyLoaded = playerManager.SwitchToDefaultCharacter();

        if(!successfullyLoaded) return;
        cameraFollow.target = playerManager.currentTransformationGameObject.transform;
    }


    /// <summary>
    /// Switch de personnage de magical girl puis de sujet de caméra
    /// </summary>
    /// <param name="characterIndex"></param>
    public void SwitchPlayerMagicalGirlTransformation(int characterIndex)
    {
        if(playerManager == null || cameraFollow == null) 
        {
            return;
        }

        bool successfullyChanged = playerManager.SwitchToCharacter(characterIndex);

        if(!successfullyChanged) 
        {
            return;
        }
        
        cameraFollow.target = playerManager.currentTransformationGameObject.transform;
    }
}
