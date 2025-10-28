using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private LayerMask hitMask;

    private Rigidbody2D rb;
    private float damage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, float speed, float damage)
    {
        this.damage = damage;
        rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;

        IDamageable d = other.GetComponent<IDamageable>();
        if (d != null)
        {
            Vector2 hitDir = rb.linearVelocity.normalized;
            d.TakeDamage(damage, hitDir);
        }
        Destroy(gameObject);
    }
}


