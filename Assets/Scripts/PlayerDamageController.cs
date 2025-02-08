using Unity.VisualScripting;
using UnityEngine;

public class PlayerDamageController : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Damage"))
        {
            playerManager.Damage();
        }
    }
}
