using UnityEngine;
using Items.Enums;

namespace Items.Data
{
    [System.Serializable]
    public class StatModifier
{
    public StatType statType;
    public float flatBonus;
    public float percentageBonus;

    public StatModifier(StatType type, float flat = 0f, float percentage = 0f)
    {
        statType = type;
        flatBonus = flat;
        percentageBonus = percentage;
    }
    }
}

