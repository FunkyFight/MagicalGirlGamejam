using UnityEngine;

public class CheckPoint : MonoBehaviour
{

    public CheckPoint cp;
    private bool isActivated = false;
    
    private SpriteRenderer spriteRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        spriteRenderer.color = Color.green;
    }

    public void Activation(Collision2D collision)
    {
        if(!isActivated)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                Destroy(gameObject);
            }
        }
    }
}
