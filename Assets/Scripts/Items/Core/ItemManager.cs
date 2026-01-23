using System.Collections.Generic;
using UnityEngine;
using Items.Data;
using Items.Enums;
using Items.Events;

/// <summary>
/// 아이템 매니저 - 장착/해제 및 랜덤 아이템 뽑기 담당
/// 싱글톤 패턴으로 전역 접근 가능
/// </summary>
public class ItemManager : MonoBehaviour
{
    private static ItemManager instance;
    public static ItemManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ItemManager");
                instance = go.AddComponent<ItemManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private SetEffectManager setEffectManager;

    private List<ItemInstance> equippedItems = new List<ItemInstance>();
    private Dictionary<string, ItemData> itemDataCache = new Dictionary<string, ItemData>();
    private ItemPoolManager itemPoolManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();
        
        if (setEffectManager == null)
            setEffectManager = GetComponent<SetEffectManager>();

        // ItemPoolManager 초기화
        itemPoolManager = ItemPoolManager.Instance;
        if (itemPoolManager == null)
        {
            Debug.LogWarning("ItemManager: ItemPoolManager를 찾을 수 없습니다. 랜덤 아이템 기능이 작동하지 않을 수 있습니다.");
        }
    }

    // 아이템 장착
    public bool EquipItem(ItemInstance item)
    {
        if (item == null || item.ItemData == null)
        {
            Debug.LogWarning("ItemManager: 장착할 아이템이 유효하지 않습니다.");
            return false;
        }

        // 이미 장착된 아이템인지 확인
        if (equippedItems.Contains(item))
        {
            Debug.LogWarning($"ItemManager: {item.ItemData.itemName}은(는) 이미 장착되어 있습니다.");
            return false;
        }

        // 아이템 데이터 로드
        LoadItemData(item);

        equippedItems.Add(item);

        // 스탯 적용
        ApplyItemStats(item);

        // 세트 효과 업데이트
        setEffectManager?.UpdateSetCounts(equippedItems);

        // 이벤트 발생
        ItemEvents.InvokeItemEquipped(item);

        return true;
    }

    // 아이템 해제
    public bool UnequipItem(ItemInstance item)
    {
        if (item == null || !equippedItems.Contains(item))
        {
            Debug.LogWarning("ItemManager: 해제할 아이템이 장착되어 있지 않습니다.");
            return false;
        }

        equippedItems.Remove(item);

        // 스탯 제거
        RemoveItemStats(item);

        // 세트 효과 업데이트
        setEffectManager?.UpdateSetCounts(equippedItems);

        // 이벤트 발생
        ItemEvents.InvokeItemUnequipped(item);

        return true;
    }

    // 아이템 스탯 적용
    private void ApplyItemStats(ItemInstance item)
    {
        if (playerStats == null || item.ItemData == null) return;

        var stats = item.GetTotalStats();
        var percentageBonuses = item.GetPercentageBonuses();

        foreach (var kvp in stats)
        {
            float percentage = percentageBonuses.ContainsKey(kvp.Key) ? percentageBonuses[kvp.Key] : 0f;
            playerStats.AddItemStat(kvp.Key, kvp.Value, percentage);
        }

        // 퍼센트만 있는 옵션도 처리
        foreach (var kvp in percentageBonuses)
        {
            if (!stats.ContainsKey(kvp.Key))
            {
                playerStats.AddItemStat(kvp.Key, 0f, kvp.Value);
            }
        }
    }

    // 아이템 스탯 제거
    private void RemoveItemStats(ItemInstance item)
    {
        if (playerStats == null || item.ItemData == null) return;

        var stats = item.GetTotalStats();
        var percentageBonuses = item.GetPercentageBonuses();

        foreach (var kvp in stats)
        {
            float percentage = percentageBonuses.ContainsKey(kvp.Key) ? percentageBonuses[kvp.Key] : 0f;
            playerStats.RemoveItemStat(kvp.Key, kvp.Value, percentage);
        }

        // 퍼센트만 있는 옵션도 처리
        foreach (var kvp in percentageBonuses)
        {
            if (!stats.ContainsKey(kvp.Key))
            {
                playerStats.RemoveItemStat(kvp.Key, 0f, kvp.Value);
            }
        }
    }

    // 아이템 데이터 로드
    private void LoadItemData(ItemInstance item)
    {
        if (item.ItemData != null) return; // 이미 로드됨

        string itemId = item.ItemDataId;
        if (string.IsNullOrEmpty(itemId)) return;

        // 캐시에서 찾기
        if (itemDataCache.ContainsKey(itemId))
        {
            item.LoadItemData(itemDataCache[itemId]);
            return;
        }

        // Resources 폴더에서 로드 (또는 다른 방식)
        ItemData data = Resources.Load<ItemData>($"Items/{itemId}");
        if (data != null)
        {
            itemDataCache[itemId] = data;
            item.LoadItemData(data);
        }
        else
        {
            Debug.LogWarning($"ItemManager: ItemData를 찾을 수 없습니다: {itemId}");
        }
    }

    // 장착된 아이템 목록 가져오기
    public List<ItemInstance> GetEquippedItems()
    {
        return new List<ItemInstance>(equippedItems);
    }

    // 특정 세트의 장착된 아이템 가져오기
    public List<ItemInstance> GetEquippedSetItems(string setName)
    {
        List<ItemInstance> setItems = new List<ItemInstance>();
        foreach (var item in equippedItems)
        {
            if (item.IsSetItem && item.ItemData != null && item.ItemData.setName == setName)
            {
                setItems.Add(item);
            }
        }
        return setItems;
    }

    // 모든 아이템 해제
    public void UnequipAll()
    {
        var itemsToRemove = new List<ItemInstance>(equippedItems);
        foreach (var item in itemsToRemove)
        {
            UnequipItem(item);
        }
    }

    // 아이템 데이터 캐시에 추가 (외부에서 사용)
    public void RegisterItemData(ItemData data)
    {
        if (data != null && !string.IsNullOrEmpty(data.itemId))
        {
            itemDataCache[data.itemId] = data;
        }
    }

    #region Random Item Methods (ItemPoolManager 위임)

    /// <summary>
    /// 랜덤 아이템 뽑기 (기본 - 레벨업 아이템만)
    /// </summary>
    /// <param name="count">뽑을 개수</param>
    /// <returns>랜덤으로 선택된 ItemData 리스트</returns>
    public List<ItemData> GetRandomItems(int count)
    {
        if (itemPoolManager == null)
        {
            Debug.LogError("ItemManager: ItemPoolManager가 초기화되지 않았습니다!");
            return new List<ItemData>();
        }
        return itemPoolManager.GetRandomItems(count);
    }

    /// <summary>
    /// 랜덤 아이템 뽑기 (고급 - 필터링 옵션)
    /// </summary>
    public List<ItemData> GetRandomItems(
        int count,
        bool useLevelUpItems = true,
        bool useNormalItems = false,
        bool useSetItems = false,
        bool allowDuplicates = false,
        List<ItemRarity> rarityFilter = null,
        List<ItemType> typeFilter = null)
    {
        if (itemPoolManager == null)
        {
            Debug.LogError("ItemManager: ItemPoolManager가 초기화되지 않았습니다!");
            return new List<ItemData>();
        }
        return itemPoolManager.GetRandomItems(count, useLevelUpItems, useNormalItems, useSetItems, allowDuplicates, rarityFilter, typeFilter);
    }

    /// <summary>
    /// 특정 레어도의 아이템만 뽑기
    /// </summary>
    public List<ItemData> GetRandomItemsByRarity(int count, ItemRarity rarity, bool allowDuplicates = false)
    {
        if (itemPoolManager == null)
        {
            Debug.LogError("ItemManager: ItemPoolManager가 초기화되지 않았습니다!");
            return new List<ItemData>();
        }
        return itemPoolManager.GetRandomItemsByRarity(count, rarity, allowDuplicates);
    }

    /// <summary>
    /// 특정 타입의 아이템만 뽑기
    /// </summary>
    public List<ItemData> GetRandomItemsByType(int count, ItemType type, bool allowDuplicates = false)
    {
        if (itemPoolManager == null)
        {
            Debug.LogError("ItemManager: ItemPoolManager가 초기화되지 않았습니다!");
            return new List<ItemData>();
        }
        return itemPoolManager.GetRandomItemsByType(count, type, allowDuplicates);
    }

    /// <summary>
    /// ItemData 리스트를 ItemInstance 리스트로 변환
    /// </summary>
    public List<ItemInstance> ConvertToItemInstances(List<ItemData> itemDataList)
    {
        List<ItemInstance> instances = new List<ItemInstance>();
        foreach (var data in itemDataList)
        {
            if (data != null)
            {
                ItemInstance instance = new ItemInstance(data);
                instances.Add(instance);
            }
        }
        return instances;
    }

    #endregion
}

