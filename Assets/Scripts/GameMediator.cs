using DG.Tweening;
using UnityEngine;

/*
* Gère absolument TOUTES les relations entre les différents gameobjects systèmes
*/
public class GameMediator : MonoBehaviour
{
    
    [Header("Components")]
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private MenuController menuController;
    [SerializeField] private AudioSource musicPlayer;

    [Header("Settings")]
    [SerializeField] private bool startMenu = true;
    
    void Awake()
    {
        bool successfullyLoaded = playerManager.SwitchToDefaultCharacter();

        if(!successfullyLoaded) return;
        cameraFollow.target = playerManager.currentTransformationGameObject.transform;

    }



    #region Player
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

    /// <summary>
    /// Sets the player's ability to move.
    /// </summary>
    /// <param name="canMove">True if the player can move, false otherwise.</param>
    public void SetPlayerCanMove(bool canMove)
    {
        if (playerManager == null)
        {
            Debug.LogWarning("PlayerManager is not assigned.");
            return;
        }

        playerManager.currentTransformationGameObject.GetComponent<PlayerController>().CanMove = canMove;
        
        playerManager.currentTransformationGameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Sets the player's movement speed.
    /// </summary>
    /// <param name="speed">The new speed for the player.</param>
    public void SetPlayerSpeed(float speed)
    {
        if (playerManager == null)
        {
            Debug.LogWarning("PlayerManager is not assigned.");
            return;
        }

        PlayerController playerController = playerManager.currentTransformationGameObject.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("PlayerController component is not found on the current transformation.");
            return;
        }

        playerController.moveSpeed = speed;
    }

    public Vector2 GetPlayerPosition()
    {
        return playerManager.currentTransformationGameObject.transform.position;
    }

    /// <summary>
    /// Instantly teleports the player and camera to a specified position.
    /// </summary>
    /// <param name="position">The target position to teleport to.</param>
    public void TeleportPlayerAndCameraInstantly(Vector2 position)
    {
        if (playerManager == null)
        {
            Debug.LogWarning("PlayerManager is not assigned.");
            return;
        }

        if (cameraFollow == null)
        {
            Debug.LogWarning("CameraFollow is not assigned.");
            return;
        }

        // Teleport the player
        playerManager.currentTransformationGameObject.transform.position = position;

        // Teleport the camera
        cameraFollow.transform.position = new Vector3(position.x, position.y, cameraFollow.transform.position.z);
    }
    #endregion






    
    #region Camera
    /// <summary>
    /// Sets the camera size and offset.
    /// </summary>
    /// <param name="size">The new size of the camera.</param>
    /// <param name="offset">The new offset of the camera.</param>
    public void SetCameraSizeAndOffset(float size, Vector2 offset)
    {
        if (cameraFollow == null)
        {
            Debug.LogWarning("CameraFollow is not assigned.");
            return;
        }

        cameraFollow.cameraSize = size;
        cameraFollow.offset = offset;
    }

    /// <summary>
    /// Enables or disables the camera follow.
    /// </summary>
    /// <param name="enabled">True to enable, false to disable.</param>
    public void SetCameraFollowEnabled(bool enabled)
    {
        if (cameraFollow == null)
        {
            Debug.LogWarning("CameraFollow is not assigned.");
            return;
        }

        cameraFollow.enabled = enabled;
    }

    /// <summary>
    /// Smoothly changes the orthographic size of the camera using DOOrthoSize.
    /// </summary>
    /// <param name="newSize">The new orthographic size to transition to.</param>
    /// <param name="duration">The duration over which the transition occurs.</param>
    public void ChangeCameraOrthoSize(float newSize, float duration, System.Action callback)
    {
        if (cameraFollow == null || cameraFollow.GetComponent<Camera>() == null)
        {
            Debug.LogWarning("Camera or CameraFollow is not assigned.");
            return;
        }

        cameraFollow.GetComponent<Camera>().DOOrthoSize(newSize, duration).SetEase(Ease.InOutQuad).OnComplete(() => callback?.Invoke());
    }
    #endregion

    #region Audio
    /// <summary>
    /// Fades the music player's volume to a specified level over a given duration.
    /// </summary>
    /// <param name="targetVolume">The target volume to fade to.</param>
    /// <param name="duration">The duration over which to fade the volume.</param>
    public void FadeMusicPlayer(float targetVolume, float duration)
    {
        if (musicPlayer == null)
        {
            Debug.LogWarning("MusicPlayer is not assigned.");
            return;
        }

        musicPlayer.DOFade(targetVolume, duration);
    }

    /// <summary>
    /// Fades the screen to black using the ScreenFade component on the MenuController.
    /// </summary>
    /// <param name="duration">The duration over which the fade occurs.</param>
    /// <param name="onComplete">Action to perform when the fade is complete.</param>
    public void FadeToBlack(float duration, System.Action onComplete = null)
    {
        if (menuController == null)
        {
            Debug.LogWarning("MenuController is not assigned.");
            return;
        }

        ScreenFade screenFade = menuController.GetComponent<ScreenFade>();
        if (screenFade == null)
        {
            Debug.LogWarning("ScreenFade component is not found on MenuController.");
            return;
        }

        screenFade.FadeToBlack(duration, onComplete);
    }

    /// <summary>
    /// Fades the screen from black using the ScreenFade component on the MenuController.
    /// </summary>
    /// <param name="duration">The duration over which the fade occurs.</param>
    /// <param name="onComplete">Action to perform when the fade is complete.</param>
    public void FadeFromBlack(float duration, System.Action onComplete = null)
    {
        if (menuController == null)
        {
            Debug.LogWarning("MenuController is not assigned.");
            return;
        }

        ScreenFade screenFade = menuController.GetComponent<ScreenFade>();
        if (screenFade == null)
        {
            Debug.LogWarning("ScreenFade component is not found on MenuController.");
            return;
        }

        screenFade.FadeFromBlack(duration, onComplete);
    }
    #endregion
}
