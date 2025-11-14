using UnityEngine;

public interface IWeapon
{
    bool CanAttack();
    void Attack(Vector2 targetWorldPosition);
    Transform FirePoint { get; }
    AnimatorOverrideController AnimatorOverrideController { get; }
}


