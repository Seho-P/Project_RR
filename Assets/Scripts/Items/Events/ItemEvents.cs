using System;
using Items.Data;

namespace Items.Events
{
    public static class ItemEvents
{
    public static event Action<ItemInstance> OnItemEquipped;
    public static event Action<ItemInstance> OnItemUnequipped;
    public static event Action<string, int> OnSetCountChanged; // 세트명, 현재 개수
    public static event Action<string, int> OnSetComplete; // 세트명, 완성된 개수
    public static event Action OnStatsChanged; // 스탯 변경 시

    public static void InvokeItemEquipped(ItemInstance item)
    {
        OnItemEquipped?.Invoke(item);
        OnStatsChanged?.Invoke();
    }

    public static void InvokeItemUnequipped(ItemInstance item)
    {
        OnItemUnequipped?.Invoke(item);
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

