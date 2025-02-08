using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{


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
    [SerializeField] private AudioSource musicPlayer;

    [Header("Misc.")]
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private PlayerController playerController;

    private float floatDuration = 5f;
    private float floatStrength = 25f;
    private int floatVibrato = 1;

    private int menuIndex = 0;


    private float inputCooldown = 0.2f; // Cooldown duration in seconds
    private float lastInputTime = 0f;

    private bool isShown = false;


    void Start()
    {
        playText.transform.DOShakePosition(floatDuration, floatStrength, floatVibrato, 90, false, false).SetLoops(-1).SetEase(Ease.InOutQuad);
        optionsText.transform.DOShakePosition(floatDuration, floatStrength, floatVibrato, 90, false, false).SetLoops(-1).SetEase(Ease.InOutQuad);
        quitText.transform.DOShakePosition(floatDuration, floatStrength, floatVibrato, 90, false, false).SetLoops(-1).SetEase(Ease.InOutQuad);

        cameraFollow.cameraSize = 2;
        cameraFollow.offset.y = 1.3f;

        playerController.CanMove = false;
        GetComponent<ScreenFade>().FadeFromBlack(3f, () => {
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
            AudioSource.PlayClipAtPoint(selectClip, playerController.transform.position);
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
        cameraFollow.enabled = false;
        cameraFollow.cameraSize = 4;
        cameraFollow.offset = new Vector2(0, 1.5f);
        isShown = false;

        AudioClip playButtonClip = playButtonClips[Random.Range(0, playButtonClips.Count)];
        AudioSource.PlayClipAtPoint(playButtonClip, playerController.transform.position);

        musicPlayer.Stop();

        canvasGroup.DOFade(0, 4f).SetEase(Ease.InExpo);
        logoGroup.DOFade(0, 4f).SetEase(Ease.InExpo);
        canvasGroup.transform.DOScale(0.8f, 4f).SetEase(Ease.InExpo);
        logoGroup.transform.DOScale(0.8f, 4f).SetEase(Ease.InExpo);

        cameraFollow.GetComponent<Camera>().DOOrthoSize(4, 5f).SetEase(Ease.InExpo)
        .OnComplete(() => {
            playerController.CanMove = true;
            cameraFollow.enabled = true;
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
