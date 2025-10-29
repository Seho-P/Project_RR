using UnityEngine;

public class SwordWeapon : MeleeWeaponBase
{
    [SerializeField] private float damageMultiplier = 1.0f;
    [SerializeField] private float damageAdd = 0f;
    [SerializeField] private float cooldownMultiplier = 1.0f;
    [SerializeField] private float cooldownAdd = 0f;
    [SerializeField] private float extraKnockRange = 0.5f;

    public override void Attack(Vector2 targetWorldPosition)
    {
        float prevDamage = damage;
        float prevCooldown = cooldown;
        float prevRange = range;

        damage = damage * damageMultiplier + damageAdd;
        cooldown = cooldown * cooldownMultiplier + cooldownAdd;
        range = prevRange + extraKnockRange;

        base.Attack(targetWorldPosition);

        damage = prevDamage;
        cooldown = prevCooldown;
        range = prevRange;
    }
}


