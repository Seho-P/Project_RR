using System;
using System.Collections.Generic;
using Items.Data;
using Items.Enums;

namespace Items.Events
{
    public static class ItemEvents
{
    /// <summary>아이템 장착 시 (장착된 아이템, 장착 직전 스탯 스냅샷)</summary>
    public static event Action<ItemInstance, Dictionary<StatType, float>> OnItemEquipped;
    /// <summary>아이템 해제 시 (해제된 아이템, 해제 직전 스탯 스냅샷)</summary>
    public static event Action<ItemInstance, Dictionary<StatType, float>> OnItemUnequipped;
    public static event Action<string, int> OnSetCountChanged; // 세트명, 현재 개수
    public static event Action<string, int> OnSetComplete; // 세트명, 완성된 개수
    public static event Action OnStatsChanged; // 스탯 변경 시

    public static void InvokeItemEquipped(ItemInstance item, Dictionary<StatType, float> statsBefore)
    {
        OnItemEquipped?.Invoke(item, statsBefore);
        OnStatsChanged?.Invoke();
    }

    public static void InvokeItemUnequipped(ItemInstance item, Dictionary<StatType, float> statsBefore)
    {
        OnItemUnequipped?.Invoke(item, statsBefore);
        OnStatsChanged?.Invoke();
    }

    public static void InvokeSetCountChanged(string setName, int count)
    {
        OnSetCountChanged?.Invoke(setName, count);
    }

    public static void InvokeSetComplete(string setName, int pieces)
    {
        OnSetComplete?.Invoke(setName, pieces);
        OnStatsChanged?.Invoke();
    }
    }
}

