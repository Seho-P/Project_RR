using UnityEngine;

public class AxeWeapon : MeleeWeaponBase
{
    [SerializeField] private float heavyMultiplier = 1.4f;

    public override void Attack(Vector2 targetWorldPosition)
    {
        float prevDamage = damage;
        float prevCooldown = cooldown;

        damage *= heavyMultiplier;
        cooldown *= 1.2f;

        base.Attack(targetWorldPosition);

        damage = prevDamage;
        cooldown = prevCooldown;
    }
}


