using UnityEngine;

public class WeaponPedestal : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private Transform weaponDisplay;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool hideDisplayAfterPickup = false;

    private WeaponHolder cachedHolder;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        if (cachedHolder == null) return;
        if (Input.GetKeyDown(interactKey))
        {
            Debug.Log($"[WeaponPedestal] E키 입력 감지! 무기 획득 시도...");
            TryGiveWeapon();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[WeaponPedestal] OnTriggerEnter2D: {other.name}, Tag: {other.tag}");
        if (!other.CompareTag(playerTag))
        {
            Debug.Log($"[WeaponPedestal] 태그 불일치. 기대: {playerTag}, 실제: {other.tag}");
            return;
        }
        cachedHolder = other.GetComponentInChildren<WeaponHolder>();
        if (cachedHolder == null)
        {
            Debug.LogWarning($"[WeaponPedestal] {other.name}에서 WeaponHolder를 찾을 수 없습니다.");
        }
        else
        {
            Debug.Log($"[WeaponPedestal] WeaponHolder 찾음! E키를 눌러 무기를 획득하세요.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        WeaponHolder holder = other.GetComponentInChildren<WeaponHolder>();
        if (holder == cachedHolder)
        {
            cachedHolder = null;
        }
    }

    private void TryGiveWeapon()
    {
        if (cachedHolder == null)
        {
            Debug.LogWarning($"[WeaponPedestal] cachedHolder가 null입니다.");
            return;
        }

        GameObject template = weaponPrefab != null ? weaponPrefab : weaponDisplay != null ? weaponDisplay.gameObject : null;
        if (template == null)
        {
            Debug.LogWarning($"[WeaponPedestal] 전달할 weapon prefab이 없습니다. weaponPrefab: {weaponPrefab}, weaponDisplay: {weaponDisplay}");
            return;
        }

        Debug.Log($"[WeaponPedestal] 무기 획득: {template.name}");
        cachedHolder.EquipWeaponFromPrefab(template);

        if (hideDisplayAfterPickup && weaponDisplay != null)
        {
            weaponDisplay.gameObject.SetActive(false);
        }
    }
}

