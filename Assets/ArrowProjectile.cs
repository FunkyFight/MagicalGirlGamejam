using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Rigidbody2D rigidbody;
    public int Direction;

    [Header("Arrow Settings")]
    [SerializeField] private float dureeDeVie = 2f;
    [SerializeField] private float tempsVecu = 0f;

    [SerializeField] private float startVelocity = 10f;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.linearVelocityX = startVelocity * Direction;
    }

    // Update is called once per frame
    void Update()
    {
        tempsVecu += Time.deltaTime;
        if(tempsVecu >= dureeDeVie) Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.GetComponent<PlayerController>() != null) return;

        Destroy(gameObject);
    }

}
