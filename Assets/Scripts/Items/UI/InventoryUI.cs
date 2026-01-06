using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Items.Data;

/// <summary>
/// 인벤토리 UI 관리 클래스
/// 슬롯들을 생성하고 관리
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemSlot slotPrefab; // 슬롯 프리팹
    [SerializeField] private Transform slotContainer; // 슬롯들이 배치될 부모 오브젝트
    [SerializeField] private GridLayoutGroup gridLayout; // 그리드 레이아웃 (선택사항)

    [Header("Settings")]
    [SerializeField] private bool createSlotsOnStart = true;
    [SerializeField] private int slotsPerRow = 6; // 한 줄에 표시할 슬롯 수

    private List<ItemSlot> slots = new List<ItemSlot>();

    private void Awake()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();

        if (slotContainer == null)
            slotContainer = transform;

        // 그리드 레이아웃 설정
        if (gridLayout == null)
            gridLayout = slotContainer.GetComponent<GridLayoutGroup>();

        if (gridLayout != null)
        {
            // 자동으로 그리드 설정 (필요시 수정)
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = slotsPerRow;
        }
    }

    private void Start()
    {
        if (createSlotsOnStart)
        {
            CreateSlots();
        }

        if (inventory != null)
        {
            // 인벤토리 이벤트 구독
            inventory.OnItemAdded += OnInventoryItemAdded;
            inventory.OnItemRemoved += OnInventoryItemRemoved;
            inventory.OnItemChanged += OnInventoryItemChanged;
        }
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnItemAdded -= OnInventoryItemAdded;
            inventory.OnItemRemoved -= OnInventoryItemRemoved;
            inventory.OnItemChanged -= OnInventoryItemChanged;
        }
    }

    /// <summary>
    /// 슬롯들 생성
    /// </summary>
    public void CreateSlots()
    {
        if (slotPrefab == null)
        {
            Debug.LogError("InventoryUI: 슬롯 프리팹이 설정되지 않았습니다!");
            return;
        }

        if (inventory == null)
        {
            Debug.LogError("InventoryUI: 인벤토리가 설정되지 않았습니다!");
            return;
        }

        // 기존 슬롯 제거
        ClearSlots();

        // 새 슬롯 생성
        int slotCount = inventory.MaxSlots;
        for (int i = 0; i < slotCount; i++)
        {
            ItemSlot slot = Instantiate(slotPrefab, slotContainer);
            slot.name = $"Slot_{i}";
            slot.SetItem(null, i);

            // 슬롯 클릭 이벤트 구독
            slot.OnSlotClicked += OnSlotClicked;
            slot.OnSlotRightClicked += OnSlotRightClicked;

            slots.Add(slot);
        }

        // 인벤토리 아이템들 업데이트
        RefreshAllSlots();
    }

    /// <summary>
    /// 모든 슬롯 새로고침
    /// </summary>
    public void RefreshAllSlots()
    {
        if (inventory == null) return;

        for (int i = 0; i < slots.Count && i < inventory.MaxSlots; i++)
        {
            ItemInstance item = inventory.GetItem(i);
            slots[i].SetItem(item, i);
        }
    }

    /// <summary>
    /// 특정 슬롯 새로고침
    /// </summary>
    public void RefreshSlot(int index)
    {
        if (index < 0 || index >= slots.Count || inventory == null)
            return;

        ItemInstance item = inventory.GetItem(index);
        slots[index].SetItem(item, index);
    }

    /// <summary>
    /// 슬롯 클릭 처리
    /// </summary>
    private void OnSlotClicked(ItemSlot slot, ItemInstance item)
    {
        if (item == null)
        {
            // 빈 슬롯 클릭 처리 (필요시 확장)
            Debug.Log($"빈 슬롯 클릭: {slot.SlotIndex}");
            return;
        }

        // 아이템 클릭 처리
        Debug.Log($"아이템 클릭: {item.ItemData.itemName} (슬롯 {slot.SlotIndex})");
        
        // 여기에 아이템 상세 정보 표시, 장착 등의 로직 추가 가능
        // 예: ShowItemDetail(item);
        // 예: EquipItem(item);
    }

    /// <summary>
    /// 슬롯 우클릭 처리
    /// </summary>
    private void OnSlotRightClicked(ItemSlot slot, ItemInstance item)
    {
        if (item == null)
            return;

        Debug.Log($"아이템 우클릭: {item.ItemData.itemName} (슬롯 {slot.SlotIndex})");
        
        // 여기에 빠른 사용, 버리기 등의 로직 추가 가능
        // 예: UseItem(item);
        // 예: DropItem(item);
    }

    /// <summary>
    /// 인벤토리 아이템 추가 이벤트 처리
    /// </summary>
    private void OnInventoryItemAdded(ItemInstance item, int index)
    {
        RefreshSlot(index);
    }

    /// <summary>
    /// 인벤토리 아이템 제거 이벤트 처리
    /// </summary>
    private void OnInventoryItemRemoved(int index)
    {
        RefreshSlot(index);
    }

    /// <summary>
    /// 인벤토리 아이템 변경 이벤트 처리
    /// </summary>
    private void OnInventoryItemChanged(int index, ItemInstance newItem)
    {
        RefreshSlot(index);
    }

    /// <summary>
    /// 모든 슬롯 제거
    /// </summary>
    private void ClearSlots()
    {
        foreach (var slot in slots)
        {
            if (slot != null)
            {
                slot.OnSlotClicked -= OnSlotClicked;
                slot.OnSlotRightClicked -= OnSlotRightClicked;
                Destroy(slot.gameObject);
            }
        }
        slots.Clear();
    }

    /// <summary>
    /// 인벤토리 UI 표시/숨기기
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// 인벤토리 UI 토글
    /// </summary>
    public void Toggle()
    {
        SetVisible(!gameObject.activeSelf);
    }

    // 프로퍼티
    public List<ItemSlot> Slots => slots;
    public Inventory Inventory => inventory;
}

