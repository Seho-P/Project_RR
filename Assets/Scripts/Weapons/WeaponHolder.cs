using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private MonoBehaviour initialWeapon;
    [SerializeField] private Animator animator;

    private IWeapon currentWeapon;

    public Transform FirePoint => firePoint;

    private void Awake()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }
        if (initialWeapon != null) EquipWeapon(initialWeapon as IWeapon);
    }

    public void EquipWeapon(IWeapon weapon)
    {
        currentWeapon = weapon;
    }

    public void Attack(Vector2 targetWorld)
    {
        if (!IsWeaponAlive(currentWeapon)) return;
        if (!currentWeapon.CanAttack()) return;
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        currentWeapon.Attack(targetWorld);
    }

    // Animation Event에서 호출할 수 있는 공통 훅
    public void OnAnimEvent(string evt)
    {
        if (!IsWeaponAlive(currentWeapon)) return;
        if (currentWeapon is IAnimEventWeapon animEventWeapon)
        {
            animEventWeapon.OnAnimEvent(evt);
        }
    }

    private bool IsWeaponAlive(IWeapon weapon)
    {
        if (weapon == null) return false;
        var unityObj = weapon as Object; // ensure Unity null semantics for destroyed objects
        return unityObj != null;
    }
}


