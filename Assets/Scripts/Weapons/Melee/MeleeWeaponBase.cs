using UnityEngine;

public abstract class MeleeWeaponBase : MonoBehaviour, IWeapon
{
    [Header("Melee Settings")]
    [SerializeField] protected float damage = 20f;
    [SerializeField] protected float cooldown = 0.4f;
    [SerializeField] protected float range = 1.2f;
    [SerializeField] protected float hitRadius = 0.7f;
    [SerializeField] protected LayerMask hitMask;

	[Header("Attack Area")]
	[SerializeField] private AttackAreaBase attackArea;

    [Header("Refs")]
    [SerializeField] protected Transform firePoint;

    protected float nextAttackTime;

    public Transform FirePoint => firePoint;

    protected virtual void Awake()
    {
        if (firePoint == null) firePoint = transform;
    }

    public bool CanAttack()
    {
        return Time.time >= nextAttackTime;
    }

    public virtual void Attack(Vector2 targetWorldPosition)
    {
        nextAttackTime = Time.time + cooldown;

        Vector2 origin = (firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position);
        Vector2 direction = (targetWorldPosition - origin).normalized;

		Collider2D[] hits;
		if (attackArea != null)
		{
			hits = attackArea.FindTargets(origin, direction, hitMask, range, hitRadius);
		}
		else
		{
			hits = Physics2D.OverlapCircleAll(origin, hitRadius, hitMask);
		}

		for (int i = 0; i < hits.Length; i++)
		{
			IDamageable d = hits[i].GetComponent<IDamageable>();
			if (d != null)
			{
				d.TakeDamage(damage, direction);
			}
		}
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = (firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position);
        Vector2 mouse = Camera.main != null ? (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) : origin + Vector2.right;
        Vector2 dir = (mouse - origin).normalized;
		if (attackArea != null)
		{
			attackArea.DrawGizmos(origin, dir, range, hitRadius, Color.red);
		}
		else
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(origin, hitRadius);
		}
    }
}


