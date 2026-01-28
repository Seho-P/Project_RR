using System.Collections.Generic;
using UnityEngine;
using Items.Enums;
using Items.Data;


    [System.Serializable]
    public class ItemInstance
{
    [SerializeField] private string itemDataId; // ItemData 참조용 ID
    [SerializeField] private int enhancementLevel;
    [SerializeField] private List<RandomOption> randomOptions;

    // 런타임 참조 (저장하지 않음)
    private ItemData itemData;

    public string ItemDataId => itemDataId;
    public ItemData ItemData => itemData;
    public int EnhancementLevel => enhancementLevel;
    public List<RandomOption> RandomOptions => randomOptions;
    public bool IsSetItem => !string.IsNullOrEmpty(itemData?.setName);

    public ItemInstance(ItemData data)
    {
        if (data == null)
        {
            Debug.LogError("ItemInstance: ItemData가 null입니다!");
            return;
        }

        itemDataId = data.itemId;
        itemData = data;
        enhancementLevel = 0;
        randomOptions = new List<RandomOption>();
    }

    // ItemData 로드 (저장/로드 후 사용)
    public void LoadItemData(ItemData data)
    {
        if (data != null && data.itemId == itemDataId)
        {
            itemData = data;
        }
    }

    // 강화 레벨 설정
    public void SetEnhancementLevel(int level)
    {
        enhancementLevel = Mathf.Max(0, level);
    }

    // 강화 레벨 증가
    public void Enhance()
    {
        enhancementLevel++;
    }

    // 랜덤 옵션 추가
    public void AddRandomOption(RandomOption option)
    {
        if (option != null)
        {
            randomOptions.Add(option);
        }
    }

    // 랜덤 옵션 설정
    public void SetRandomOptions(List<RandomOption> options)
    {
        randomOptions = options ?? new List<RandomOption>();
    }

    // 최종 스탯 계산
    public Dictionary<StatType, float> GetTotalStats()
    {
        var stats = new Dictionary<StatType, float>();

        if (itemData == null) return stats;

        // 1. 기본 스탯
        foreach (var baseStat in itemData.baseStats)
        {
            if (!stats.ContainsKey(baseStat.statType))
                stats[baseStat.statType] = 0f;

            stats[baseStat.statType] += baseStat.flatBonus;
        }

        // 2. 강화 보너스 (기본 스탯의 퍼센트 증가)
        float enhancementMultiplier = 1f + (enhancementLevel * 0.1f); // 레벨당 10% 증가
        var keys = new List<StatType>(stats.Keys);
        foreach (var key in keys)
        {
            stats[key] *= enhancementMultiplier;
        }

        // 3. 랜덤 옵션
        foreach (var option in randomOptions)
        {
            if (!stats.ContainsKey(option.statType))
                stats[option.statType] = 0f;

            stats[option.statType] += option.flatValue;
            // 퍼센트 옵션은 별도로 처리 필요 (PlayerStats에서)
        }

        return stats;
    }

    // 퍼센트 보너스 가져오기 (기본 스탯 + 랜덤 옵션)
    public Dictionary<StatType, float> GetPercentageBonuses()
    {
        var bonuses = new Dictionary<StatType, float>();

        if (itemData == null) return bonuses;

        // 1. 기본 스탯의 퍼센트 보너스
        foreach (var baseStat in itemData.baseStats)
        {
            if (baseStat.percentageBonus != 0f)
            {
                if (!bonuses.ContainsKey(baseStat.statType))
                    bonuses[baseStat.statType] = 0f;
                bonuses[baseStat.statType] += baseStat.percentageBonus;
            }
        }

        // 2. 랜덤 옵션의 퍼센트 보너스
        foreach (var option in randomOptions)
        {
            if (option.percentageValue != 0f)
            {
                if (!bonuses.ContainsKey(option.statType))
                    bonuses[option.statType] = 0f;
                bonuses[option.statType] += option.percentageValue;
            }
        }

        return bonuses;
    }
    }

