using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private MonoBehaviour initialWeapon;

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
        currentWeapon.Attack(targetWorld);
    }

    private bool IsWeaponAlive(IWeapon weapon)
    {
        if (weapon == null) return false;
        var unityObj = weapon as Object; // ensure Unity null semantics for destroyed objects
        return unityObj != null;
    }
}


