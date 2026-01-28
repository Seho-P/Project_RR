using System.Collections.Generic;
using UnityEngine;
using Items.Enums;

public class PlayerStats : MonoBehaviour
{
    private Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>();
    private Dictionary<StatType, float> itemStats = new Dictionary<StatType, float>();
    private Dictionary<StatType, float> setBonusStats = new Dictionary<StatType, float>();
    private Dictionary<StatType, float> itemPercentageBonuses = new Dictionary<StatType, float>();
    private Dictionary<StatType, float> setPercentageBonuses = new Dictionary<StatType, float>();
    
    // 최종 스탯 캐시 (스탯 변경 시 갱신됨)
    private Dictionary<StatType, float> finalStats = new Dictionary<StatType, float>();

    private void Awake()
    {
        baseStats = new Dictionary<StatType, float>
        {
            { StatType.MoveSpeed, 3f },
            { StatType.MaxHealth, 100f },
            { StatType.HealthRegen, 1f },
            { StatType.AttackSpeed, 1f },
            { StatType.AttackDamage, 1f },
            { StatType.CriticalChance, 0f },
            { StatType.CriticalDamage, 0f },
            { StatType.Defense, 0f },
            { StatType.DamageReduction, 0f },
            { StatType.LifeSteal, 0f },
            { StatType.CooldownReduction, 0f },
            { StatType.ExperienceGain, 0f },
            { StatType.GoldGain, 0f },
            { StatType.ProjectileSpeed, 0f },
            { StatType.AttackRange, 0f },
        };
        
        // 초기 최종 스탯 계산
        UpdateAllFinalStats();
    }

    // 기본 스탯 설정
    public void SetBaseStat(StatType type, float value)
    {
        baseStats[type] = value;
        UpdateFinalStat(type);
    }

    // 기본 스탯 가져오기
    public float GetBaseStat(StatType type)
    {
        return baseStats.ContainsKey(type) ? baseStats[type] : 0f;
    }

    // 아이템 스탯 추가
    public void AddItemStat(StatType type, float flatValue, float percentageValue = 0f)
    {
        if (!itemStats.ContainsKey(type))
            itemStats[type] = 0f;
        itemStats[type] += flatValue;

        if (percentageValue != 0f)
        {
            if (!itemPercentageBonuses.ContainsKey(type))
                itemPercentageBonuses[type] = 0f;
            itemPercentageBonuses[type] += percentageValue;
        }
        
        // 최종 스탯 갱신
        UpdateFinalStat(type);
    }

    // 아이템 스탯 제거
    public void RemoveItemStat(StatType type, float flatValue, float percentageValue = 0f)
    {
        if (itemStats.ContainsKey(type))
        {
            itemStats[type] -= flatValue;
            if (itemStats[type] <= 0f)
                itemStats.Remove(type);
        }

        if (percentageValue != 0f && itemPercentageBonuses.ContainsKey(type))
        {
            itemPercentageBonuses[type] -= percentageValue;
            if (itemPercentageBonuses[type] <= 0f)
                itemPercentageBonuses.Remove(type);
        }
        
        // 최종 스탯 갱신
        UpdateFinalStat(type);
    }

    // 아이템 스탯 초기화
    public void ClearItemStats()
    {
        // 영향받는 스탯 타입들 저장
        var affectedStats = new HashSet<StatType>();
        foreach (var kvp in itemStats)
        {
            affectedStats.Add(kvp.Key);
        }
        foreach (var kvp in itemPercentageBonuses)
        {
            affectedStats.Add(kvp.Key);
        }
        
        itemStats.Clear();
        itemPercentageBonuses.Clear();
        
        // 영향받은 스탯들의 최종 값 갱신
        foreach (var statType in affectedStats)
        {
            UpdateFinalStat(statType);
        }
    }

    // 세트 보너스 스탯 추가
    public void AddSetBonusStat(StatType type, float flatValue, float percentageValue = 0f)
    {
        if (!setBonusStats.ContainsKey(type))
            setBonusStats[type] = 0f;
        setBonusStats[type] += flatValue;

        if (percentageValue != 0f)
        {
            if (!setPercentageBonuses.ContainsKey(type))
                setPercentageBonuses[type] = 0f;
            setPercentageBonuses[type] += percentageValue;
        }
        
        // 최종 스탯 갱신
        UpdateFinalStat(type);
    }

    // 세트 보너스 스탯 제거
    public void RemoveSetBonusStat(StatType type, float flatValue, float percentageValue = 0f)
    {
        if (setBonusStats.ContainsKey(type))
        {
            setBonusStats[type] -= flatValue;
            if (setBonusStats[type] <= 0f)
                setBonusStats.Remove(type);
        }

        if (percentageValue != 0f && setPercentageBonuses.ContainsKey(type))
        {
            setPercentageBonuses[type] -= percentageValue;
            if (setPercentageBonuses[type] <= 0f)
                setPercentageBonuses.Remove(type);
        }
        
        // 최종 스탯 갱신
        UpdateFinalStat(type);
    }

    // 세트 보너스 스탯 초기화
    public void ClearSetBonusStats()
    {
        // 영향받는 스탯 타입들 저장
        var affectedStats = new HashSet<StatType>();
        foreach (var kvp in setBonusStats)
        {
            affectedStats.Add(kvp.Key);
        }
        foreach (var kvp in setPercentageBonuses)
        {
            affectedStats.Add(kvp.Key);
        }
        
        setBonusStats.Clear();
        setPercentageBonuses.Clear();
        
        // 영향받은 스탯들의 최종 값 갱신
        foreach (var statType in affectedStats)
        {
            UpdateFinalStat(statType);
        }
    }

    // 특정 스탯의 최종 값 계산 및 갱신
    private void UpdateFinalStat(StatType type)
    {
        float baseValue = GetBaseStat(type);
        float item = itemStats.ContainsKey(type) ? itemStats[type] : 0f;
        float setBonus = setBonusStats.ContainsKey(type) ? setBonusStats[type] : 0f;

        // 기본 스탯에 퍼센트 보너스 적용
        float itemPercentage = itemPercentageBonuses.ContainsKey(type) ? itemPercentageBonuses[type] : 0f;
        float setPercentage = setPercentageBonuses.ContainsKey(type) ? setPercentageBonuses[type] : 0f;
        float totalPercentage = itemPercentage + setPercentage;

        float totalFlat = baseValue + item + setBonus;
        float finalValue = totalFlat * (1f + totalPercentage / 100f);
        
        finalStats[type] = finalValue;
    }
    
    // 모든 최종 스탯 갱신
    private void UpdateAllFinalStats()
    {
        foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
        {
            UpdateFinalStat(statType);
        }
    }

    // 최종 스탯 가져오기 (캐시된 값 반환)
    public float GetStat(StatType type)
    {
        // 캐시에 없으면 계산해서 추가 (안전장치)
        if (!finalStats.ContainsKey(type))
        {
            UpdateFinalStat(type);
        }
        
        return finalStats[type];
    }

    // 모든 스탯 초기화
    public void ResetAllStats()
    {
        ClearItemStats();
        ClearSetBonusStats();
        // ClearItemStats와 ClearSetBonusStats에서 이미 갱신하므로 추가 갱신 불필요
    }
}

