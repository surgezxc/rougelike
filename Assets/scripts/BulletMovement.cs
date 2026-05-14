using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletMovement : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private Rigidbody2D rb;
    private float lifeTimer;
    private bool isLaunched;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    public void Launch(Vector2 direction, float speed, float lifetime)
    {
        rb.linearVelocity = direction.normalized * speed;
        lifeTimer = lifetime;
        isLaunched = true;
    }

    private void Update()
    {
        if (!isLaunched || lifeTimer <= 0f)
        {
            return;
        }

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Despawn();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLaunched)
        {
            return;
        }

        EnemyHealth enemy = collision.collider.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        Despawn();
    }

    private void Despawn()
    {
        isLaunched = false;
        lifeTimer = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        Destroy(gameObject);
    }
}
