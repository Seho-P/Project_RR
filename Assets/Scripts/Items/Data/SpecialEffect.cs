using UnityEngine;
using Items.Enums;

namespace Items.Data
{
    [System.Serializable]
    public class SpecialEffect
{
    public EffectType effectType;
    public float value;
    public float duration; // 지속 시간 (0이면 영구)

    public SpecialEffect(EffectType type, float val, float dur = 0f)
    {
        effectType = type;
        value = val;
        duration = dur;
    }
    }
}

