using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{

    [SerializeField] private GameMediator gameMediator;
    [Header("Texts")]
    [SerializeField] private TMP_Text playText;
    [SerializeField] private TMP_Text optionsText;
    [SerializeField] private TMP_Text quitText;

    [Header("CanvasGroups")]
    [SerializeField] private CanvasGroup logoGroup;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Sound Effects / Music")]
    [SerializeField] private List<AudioClip> selectClips;
    [SerializeField] private List<AudioClip> playButtonClips;

    private float floatDuration = 5f;
    private float floatStrength = 25f;
    private int floatVibrato = 1;

    private int menuIndex = 0;


    private float inputCooldown = 0.2f; // Cooldown duration in seconds
    private float lastInputTime = 0f;

    private bool isShown = false;


    void Start()
    {
        Vector2? playerPosition = gameMediator.GetPlayerPosition();
        if(playerPosition == null) return;
        
        playText.transform.DOShakePosition(floatDuration, floatStrength, floatVibrato, 90, false, false).SetLoops(-1).SetEase(Ease.InOutQuad);
        optionsText.transform.DOShakePosition(floatDuration, floatStrength, floatVibrato, 90, false, false).SetLoops(-1).SetEase(Ease.InOutQuad);
        quitText.transform.DOShakePosition(floatDuration, floatStrength, floatVibrato, 90, false, false).SetLoops(-1).SetEase(Ease.InOutQuad);

        gameMediator.SetCameraSizeAndOffset(2, new Vector2(0, 1.3f));
        gameMediator.TeleportPlayerAndCameraInstantly(playerPosition.Value);
        gameMediator.SetPlayerCanMove(false);
        gameMediator.SetPlayerSpeed(4);

        GetComponent<ScreenFade>().FadeFromBlack(5f, () => {
            logoGroup.DOFade(1f, 2f).SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                canvasGroup.DOFade(1f, 3f).SetEase(Ease.InOutQuad);
                isShown = true;
            });
        });
    }

    void updateTextColors()
    {
        // Tableau des éléments de texte du menu
        TMP_Text[] menuTexts = { playText, optionsText, quitText };
        // Indices correspondants pour les éléments du menu
        int[] menuIndices = { 0, -1, 1 };

        for (int i = 0; i < menuTexts.Length; i++)
        {
            // Met en surbrillance l'élément de menu sélectionné
            menuTexts[i].color = menuIndex == menuIndices[i] ? Color.yellow : Color.white;

            // Vérifie si l'élément de menu doit être agrandi ou réduit
            Vector3 targetScale = menuIndex == menuIndices[i] ? Vector3.one * 1.2f : Vector3.one;

            // Applique l'animation de mise à l'échelle uniquement si la taille cible est différente de l'échelle actuelle
            if (menuTexts[i].transform.localScale != targetScale)
            {
                menuTexts[i].transform.DOScale(targetScale, 2f).SetEase(Ease.OutBack);
            }
        }
    }
    
    void selection()
    {
        Vector2? playerPosition = gameMediator.GetPlayerPosition();
        if(playerPosition == null) return;

        if (Time.time - lastInputTime < inputCooldown)
            return;

        Vector2 moveInput = InputSystem.actions["Move"].ReadValue<Vector2>();

        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            menuIndex += moveInput.x < 0 ? -1 : 1;
            if (menuIndex < -1) menuIndex = 1;
            else if (menuIndex > 1) menuIndex = -1;
            lastInputTime = Time.time;
            
            AudioClip selectClip = selectClips[Random.Range(0, selectClips.Count)];
            AudioSource.PlayClipAtPoint(selectClip, playerPosition.Value);
        }
    }

    void confirmSelection()
    {
        if(InputSystem.actions["Jump"].ReadValue<float>() != 1f) return;

        switch(menuIndex)
        {
            case 0:
                Play();
                break;
        }
    }

    void Play()
    {
        Vector2? playerPosition = gameMediator.GetPlayerPosition();
        if(playerPosition == null) return;

        gameMediator.SetCameraFollowEnabled(false);
        gameMediator.SetCameraSizeAndOffset(5, new Vector2(0, 1.5f));
        isShown = false;

        AudioClip playButtonClip = playButtonClips[Random.Range(0, playButtonClips.Count)];
        AudioSource.PlayClipAtPoint(playButtonClip, playerPosition.Value);

        gameMediator.FadeMusicPlayer(0f, 4f);

        canvasGroup.DOFade(0, 4f).SetEase(Ease.InExpo);
        logoGroup.DOFade(0, 4f).SetEase(Ease.InExpo);
        canvasGroup.transform.DOScale(0.8f, 4f).SetEase(Ease.InExpo);
        logoGroup.transform.DOScale(0.8f, 4f).SetEase(Ease.InExpo);

        gameMediator.ChangeCameraOrthoSize(4f, 5f, () => {
            gameMediator.SetCameraFollowEnabled(true); 
            gameMediator.SetPlayerCanMove(true);
        });

    }

    // Update is called once per frame
    void Update()
    {
        if(!isShown) return;

        selection();
        confirmSelection();
        updateTextColors();
    }
}
