using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

public class Interactable : MonoBehaviour
{
    [SerializeField] private GameMediator mediator;
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string nodeName;
    [SerializeField] private float minDistance = 2;
    private bool isInInteraction = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    // Update is called once per frame
    void Update()
    {
        bool goodDistance = Vector2.Distance(transform.position, mediator.GetPlayerPosition().Value) <= minDistance;

        if(goodDistance)
        {
            spriteRenderer.color = originalColor + new Color(0.2f, 0.2f, -0.1f, 0f);
        } else
        {
            spriteRenderer.color = originalColor;
        }

        if(InputSystem.actions["Interact"].ReadValue<float>() == 1f && !isInInteraction && mediator.CanPlayerMove() && goodDistance)
        {
            isInInteraction = true;
            StartCoroutine(StartInteraction());
        }
    }

    private IEnumerator StartInteraction()
    {
        mediator.SetPlayerCanMove(false);

        dialogueRunner.StartDialogue(nodeName);
        yield return new WaitUntil(() => !dialogueRunner.IsDialogueRunning);

        mediator.SetPlayerCanMove(true);
        isInInteraction = false;
    }
}
