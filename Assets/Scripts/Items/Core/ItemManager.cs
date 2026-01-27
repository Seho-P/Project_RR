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

    [Header("디버그 설정")]
    [SerializeField] private bool enableStatLogging = true; // 스탯 변화 로깅 활성화

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
        if (item == null)
        {
            Debug.LogWarning("ItemManager: 장착할 아이템이 유효하지 않습니다.");
            return false;
        }

        // ItemData가 없으면 실패
        if (item.ItemData == null)
        {
            Debug.LogWarning($"ItemManager: 아이템 데이터가 없습니다. (ItemID: {item.ItemDataId ?? "Unknown"})");
            return false;
        }

        // 이미 장착된 아이템인지 확인
        if (equippedItems.Contains(item))
        {
            Debug.LogWarning($"ItemManager: {item.ItemData.itemName}은(는) 이미 장착되어 있습니다.");
            return false;
        }

        equippedItems.Add(item);

        // 스탯 적용 전 현재 스탯 저장 (로깅용)
        Dictionary<StatType, float> statsBefore = null;
        if (enableStatLogging && playerStats != null)
        {
            statsBefore = GetCurrentStatsSnapshot();
        }

        // 스탯 적용
        ApplyItemStats(item);

        // 세트 효과 업데이트
        setEffectManager?.UpdateSetCounts(equippedItems);

        // 스탯 변화 로깅
        if (enableStatLogging && playerStats != null && statsBefore != null)
        {
            LogStatChanges(item, statsBefore, true);
        }

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

        // 스탯 제거 전 현재 스탯 저장 (로깅용)
        Dictionary<StatType, float> statsBefore = null;
        if (enableStatLogging && playerStats != null)
        {
            statsBefore = GetCurrentStatsSnapshot();
        }

        // 스탯 제거
        RemoveItemStats(item);

        // 세트 효과 업데이트
        setEffectManager?.UpdateSetCounts(equippedItems);

        // 스탯 변화 로깅
        if (enableStatLogging && playerStats != null && statsBefore != null)
        {
            LogStatChanges(item, statsBefore, false);
        }

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

    #region 디버그 및 로깅

    /// <summary>
    /// 현재 스탯의 스냅샷을 가져옵니다 (로깅용)
    /// </summary>
    private Dictionary<StatType, float> GetCurrentStatsSnapshot()
    {
        var snapshot = new Dictionary<StatType, float>();
        foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
        {
            snapshot[statType] = playerStats.GetStat(statType);
        }
        return snapshot;
    }

    /// <summary>
    /// 아이템 장착/해제 시 스탯 변화를 콘솔에 로깅합니다
    /// </summary>
    private void LogStatChanges(ItemInstance item, Dictionary<StatType, float> statsBefore, bool isEquipping)
    {
        if (item?.ItemData == null || playerStats == null) return;

        System.Text.StringBuilder log = new System.Text.StringBuilder();
        log.AppendLine($"\n=== 아이템 {(isEquipping ? "장착" : "해제")}: {item.ItemData.itemName} ===");

        bool hasChanges = false;

        // 모든 스탯 비교
        foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
        {
            float before = statsBefore.ContainsKey(statType) ? statsBefore[statType] : 0f;
            float after = playerStats.GetStat(statType);
            float difference = after - before;

            // 변화가 있는 스탯만 표시
            if (Mathf.Abs(difference) > 0.01f)
            {
                string statName = GetStatName(statType);
                string changeText = difference > 0 ? $"+{difference:F2}" : $"{difference:F2}";
                log.AppendLine($"  {statName}: {before:F2} → {after:F2} ({changeText})");
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            Debug.Log(log.ToString());
        }
        else
        {
            Debug.Log($"[ItemManager] {item.ItemData.itemName} {(isEquipping ? "장착" : "해제")} - 스탯 변화 없음");
        }
    }

    /// <summary>
    /// 스탯 타입의 한글 이름을 반환합니다
    /// </summary>
    private string GetStatName(StatType statType)
    {
        switch (statType)
        {
            case StatType.MaxHealth: return "최대 체력";
            case StatType.HealthRegen: return "체력 재생";
            case StatType.MoveSpeed: return "이동 속도";
            case StatType.AttackSpeed: return "공격 속도";
            case StatType.AttackDamage: return "공격력";
            case StatType.CriticalChance: return "치명타 확률";
            case StatType.CriticalDamage: return "치명타 피해";
            case StatType.Defense: return "방어력";
            case StatType.DamageReduction: return "피해 감소";
            case StatType.LifeSteal: return "생명력 흡수";
            case StatType.CooldownReduction: return "재사용 대기시간 감소";
            case StatType.ExperienceGain: return "경험치 획득";
            case StatType.GoldGain: return "골드 획득";
            case StatType.ProjectileSpeed: return "투사체 속도";
            case StatType.AttackRange: return "공격 범위";
            default: return statType.ToString();
        }
    }

    #endregion
}

