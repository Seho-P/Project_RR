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

    // 기본 스탯 설정
    public void SetBaseStat(StatType type, float value)
    {
        baseStats[type] = value;
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
    }

    // 아이템 스탯 초기화
    public void ClearItemStats()
    {
        itemStats.Clear();
        itemPercentageBonuses.Clear();
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
    }

    // 세트 보너스 스탯 초기화
    public void ClearSetBonusStats()
    {
        setBonusStats.Clear();
        setPercentageBonuses.Clear();
    }

    // 최종 스탯 가져오기
    public float GetStat(StatType type)
    {
        float baseValue = GetBaseStat(type);
        float item = itemStats.ContainsKey(type) ? itemStats[type] : 0f;
        float setBonus = setBonusStats.ContainsKey(type) ? setBonusStats[type] : 0f;

        // 기본 스탯에 퍼센트 보너스 적용
        float itemPercentage = itemPercentageBonuses.ContainsKey(type) ? itemPercentageBonuses[type] : 0f;
        float setPercentage = setPercentageBonuses.ContainsKey(type) ? setPercentageBonuses[type] : 0f;
        float totalPercentage = itemPercentage + setPercentage;

        float totalFlat = baseValue + item + setBonus;
        return totalFlat * (1f + totalPercentage / 100f);
    }

    // 모든 스탯 초기화
    public void ResetAllStats()
    {
        ClearItemStats();
        ClearSetBonusStats();
    }
}

