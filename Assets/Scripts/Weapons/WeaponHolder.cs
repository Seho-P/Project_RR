using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private MonoBehaviour initialWeapon;
    [SerializeField] private Animator animator;

    private IWeapon currentWeapon;
    private MonoBehaviour currentWeaponBehaviour;

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
        currentWeaponBehaviour = weapon as MonoBehaviour;
        
        // 무기의 firePoint가 없거나 무기 자신의 transform이면 WeaponHolder의 firePoint를 할당
        if (weapon != null && firePoint != null)
        {
            Transform weaponFirePoint = weapon.FirePoint;
            MonoBehaviour weaponMono = weapon as MonoBehaviour;
            
            // firePoint가 null이거나 무기 자신의 transform이면 WeaponHolder의 firePoint로 설정
            if (weaponFirePoint == null || (weaponMono != null && weaponFirePoint == weaponMono.transform))
            {
                if (weapon is MeleeWeaponBase meleeWeapon)
                {
                    var field = typeof(MeleeWeaponBase).GetField("firePoint", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(meleeWeapon, firePoint);
                }
                else if (weapon is RangedWeaponBase rangedWeapon)
                {
                    var field = typeof(RangedWeaponBase).GetField("firePoint", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(rangedWeapon, firePoint);
                }
            }
        }

        if (animator != null && weapon != null)
        {
            var controller = weapon.AnimatorOverrideController;
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }
        }
    }

    public void UnequipCurrentWeapon()
    {
        if (currentWeaponBehaviour != null)
        {
            Destroy(currentWeaponBehaviour.gameObject);
        }

        currentWeapon = null;
        currentWeaponBehaviour = null;
    }

    public void EquipWeaponFromPrefab(GameObject weaponPrefab)
    {
        if (weaponPrefab == null)
        {
            Debug.LogWarning("WeaponHolder: weapon prefab이 지정되지 않았습니다.");
            return;
        }

        UnequipCurrentWeapon();

        Transform weaponPoint = firePoint != null ? firePoint : transform;
        
        // WeaponPoint 아래에 Pivot 찾기
        Transform pivot = weaponPoint.Find("pivot");
        
        // 프리팹의 원래 로컬 회전을 유지하기 위해 부모를 먼저 설정하지 않고 인스턴스화
        GameObject weaponInstance = Instantiate(weaponPrefab, pivot.position, Quaternion.identity);
        
        // Pivot 아래에 무기를 배치하고 프리팹의 원래 로컬 변환값 유지
        weaponInstance.transform.SetParent(pivot);
        // 프리팹의 원래 로컬 포지션 유지
        weaponInstance.transform.localPosition = weaponPrefab.transform.localPosition;
        // 프리팹의 원래 로컬 회전 유지 (프리팹이 90도면 그대로 유지)
        weaponInstance.transform.localRotation = weaponPrefab.transform.localRotation;
        
        IWeapon weapon = weaponInstance.GetComponent<IWeapon>();
        if (weapon == null)
        {
            Debug.LogWarning($"WeaponHolder: {weaponPrefab.name}에 IWeapon 구현이 없습니다.");
            Destroy(weaponInstance);
            return;
        }

        EquipWeapon(weapon);
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


