using UnityEngine;
using Items.Enums;

namespace Items.Data
{
    [System.Serializable]
    public class RandomOption
{
    public StatType statType;
    public float flatValue;
    public float percentageValue;

    public RandomOption(StatType type, float flat, float percentage)
    {
        statType = type;
        flatValue = flat;
        percentageValue = percentage;
    }
    }
}

