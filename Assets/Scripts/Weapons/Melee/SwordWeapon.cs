using UnityEngine;

public class SwordWeapon : MeleeWeaponBase
{
    [SerializeField] private float extraKnockRange = 0.1f;

    public override void Attack(Vector2 targetWorldPosition)
    {
        base.Attack(targetWorldPosition);
        nextAttackTime = Time.time + cooldown * 0.9f;
        range += extraKnockRange;
        range -= extraKnockRange;
    }
}


