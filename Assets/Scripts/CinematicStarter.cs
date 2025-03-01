using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Yarn.Unity;

public abstract class CinematicStarter : MonoBehaviour {
    
    [SerializeField] protected GameMediator gameMediator;
    [SerializeField] protected DialogueRunner dialogueRunner;

    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.GetComponent<PlayerController>() == null) return;

        GetComponent<BoxCollider2D>().enabled = false;
        gameMediator.SetPlayerCanMove(false);
        StartCoroutine(WaitForCinematicToEnd());

        IEnumerator WaitForCinematicToEnd()
        {
            yield return StartCoroutine(StartCinematic());
            gameMediator.SetPlayerCanMove(true);
            Destroy(this);
        }
    }

    protected abstract IEnumerator StartCinematic();
}