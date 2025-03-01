using UnityEngine;

public class MobHitbox : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Attack"))
        {
            Destroy(gameObject);
        }
    }
}
