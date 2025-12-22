using System;

namespace Items.Events
{
    public static class PlayerLevelEvents
    {
        // 경험치 변경 이벤트 (현재 경험치, 최대 경험치)
        public static event Action<int, int> OnXPChanged;
        
        // 레벨업 이벤트 (새 레벨)
        public static event Action<int> OnLevelUp;
        
        // 레벨 변경 이벤트 (이전 레벨, 새 레벨)
        public static event Action<int, int> OnLevelChanged;

        public static void InvokeXPChanged(int currentXP, int maxXP)
        {
            OnXPChanged?.Invoke(currentXP, maxXP);
        }

        public static void InvokeLevelUp(int newLevel)
        {
            OnLevelUp?.Invoke(newLevel);
        }

        public static void InvokeLevelChanged(int oldLevel, int newLevel)
        {
            OnLevelChanged?.Invoke(oldLevel, newLevel);
        }
    }
}

