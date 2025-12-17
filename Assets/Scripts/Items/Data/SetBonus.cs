using UnityEngine;
using System.Collections.Generic;

namespace Items.Data
{
    [System.Serializable]
    public class SetBonus
{
    public int requiredPieces;
    public List<StatModifier> statModifiers;
    public List<SpecialEffect> specialEffects;

    public SetBonus(int pieces)
    {
        requiredPieces = pieces;
        statModifiers = new List<StatModifier>();
        specialEffects = new List<SpecialEffect>();
    }
    }
}

