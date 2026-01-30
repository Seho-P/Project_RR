using System.Collections.Generic;
using UnityEngine;
using Items.Data;
using Items.Events;

/// <summary>
/// 인벤토리 데이터 관리 클래스
/// 아이템 저장 및 관리
/// </summary>
public class Inventory : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxSlots = 30; // 최대 슬롯 수

    private List<ItemInstance> items = new List<ItemInstance>();

    // 이벤트
    public System.Action<ItemInstance, int> OnItemAdded;      // 아이템 추가 (아이템, 인덱스)
    public System.Action<int> OnItemRemoved;                  // 아이템 제거 (인덱스)
    public System.Action<int, ItemInstance> OnItemChanged;    // 아이템 변경 (인덱스, 새 아이템)

    private void Awake()
    {
        // 인벤토리 초기화
        items = new List<ItemInstance>(maxSlots);
        for (int i = 0; i < maxSlots; i++)
        {
            items.Add(null);
        }
    }
        private void Start()
    {
        // ItemEvents 구독
        ItemEvents.OnItemEquipped += HandleItemEquipped;
        ItemEvents.OnItemUnequipped += HandleItemUnequipped;
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        ItemEvents.OnItemEquipped -= HandleItemEquipped;
        ItemEvents.OnItemUnequipped -= HandleItemUnequipped;
    }
        private void HandleItemEquipped(ItemInstance item, System.Collections.Generic.Dictionary<Items.Enums.StatType, float> statsBefore)
    {
        // 장착된 아이템을 인벤토리에 추가
        AddItem(item);
    }

    private void HandleItemUnequipped(ItemInstance item, System.Collections.Generic.Dictionary<Items.Enums.StatType, float> statsBefore)
    {
        // 해제된 아이템을 인벤토리에서 제거
        RemoveItem(item);
    }

    /// <summary>
    /// 아이템 추가 (빈 슬롯에 자동 배치)
    /// </summary>
    public bool AddItem(ItemInstance item)
    {
        if (item == null || item.ItemData == null)
        {
            Debug.LogWarning("Inventory: 추가할 아이템이 유효하지 않습니다.");
            return false;
        }

        // 빈 슬롯 찾기
        int emptyIndex = FindEmptySlot();
        if (emptyIndex == -1)
        {
            Debug.LogWarning("Inventory: 인벤토리가 가득 찼습니다.");
            return false;
        }

        return SetItem(emptyIndex, item);
    }

    /// <summary>
    /// 특정 인덱스에 아이템 설정
    /// </summary>
    public bool SetItem(int index, ItemInstance item)
    {
        if (index < 0 || index >= maxSlots)
        {
            Debug.LogWarning($"Inventory: 유효하지 않은 인덱스입니다: {index}");
            return false;
        }

        items[index] = item;
        OnItemChanged?.Invoke(index, item);
        
        if (item != null)
        {
            OnItemAdded?.Invoke(item, index);
        }

        return true;
    }

    /// <summary>
    /// 아이템 제거
    /// </summary>
    public bool RemoveItem(int index)
    {
        if (index < 0 || index >= maxSlots)
        {
            Debug.LogWarning($"Inventory: 유효하지 않은 인덱스입니다: {index}");
            return false;
        }

        if (items[index] == null)
        {
            return false;
        }

        items[index] = null;
        OnItemRemoved?.Invoke(index);
        OnItemChanged?.Invoke(index, null);

        return true;
    }

    /// <summary>
    /// 아이템 제거 (아이템 인스턴스로)
    /// </summary>
    public bool RemoveItem(ItemInstance item)
    {
        int index = FindItemIndex(item);
        if (index != -1)
        {
            return RemoveItem(index);
        }
        return false;
    }

    /// <summary>
    /// 아이템 가져오기
    /// </summary>
    public ItemInstance GetItem(int index)
    {
        if (index < 0 || index >= maxSlots)
            return null;

        return items[index];
    }

    /// <summary>
    /// 빈 슬롯 찾기
    /// </summary>
    public int FindEmptySlot()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// 아이템 인덱스 찾기
    /// </summary>
    public int FindItemIndex(ItemInstance item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == item)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// 인벤토리가 가득 찼는지 확인
    /// </summary>
    public bool IsFull()
    {
        return FindEmptySlot() == -1;
    }

    /// <summary>
    /// 빈 슬롯 개수 가져오기
    /// </summary>
    public int GetEmptySlotCount()
    {
        int count = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
                count++;
        }
        return count;
    }

    /// <summary>
    /// 모든 아이템 가져오기
    /// </summary>
    public List<ItemInstance> GetAllItems()
    {
        List<ItemInstance> result = new List<ItemInstance>();
        foreach (var item in items)
        {
            if (item != null)
                result.Add(item);
        }
        return result;
    }

    /// <summary>
    /// 인벤토리 초기화
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null)
            {
                RemoveItem(i);
            }
        }
    }

    // 프로퍼티
    public int MaxSlots => maxSlots;
    public int ItemCount => items.Count - GetEmptySlotCount();
}

