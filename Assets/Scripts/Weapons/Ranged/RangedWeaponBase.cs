using UnityEngine;

public class RangedWeaponBase : MonoBehaviour, IWeapon
{
    [Header("Ranged Settings")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float cooldown = 0.2f;
    [SerializeField] protected float projectileSpeed = 12f;
    [SerializeField] protected GameObject projectilePrefab;

    [Header("Refs")]
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected AnimatorOverrideController animatorOverrideController;
    protected float nextAttackTime;

    public Transform FirePoint => firePoint;
    public AnimatorOverrideController AnimatorOverrideController => animatorOverrideController;
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
        if (projectilePrefab == null) return;

        nextAttackTime = Time.time + cooldown;

        Vector2 origin = (firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position);
        Vector2 dir = (targetWorldPosition - origin).normalized;

        GameObject go = Instantiate(projectilePrefab, origin, Quaternion.identity);
        Projectile p = go.GetComponent<Projectile>();
        if (p != null)
        {
            p.Launch(dir, projectileSpeed, damage);
        }
    }
}


