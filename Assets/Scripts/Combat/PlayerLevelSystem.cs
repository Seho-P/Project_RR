using UnityEngine;
using Items.Events;

public class PlayerLevelSystem : MonoBehaviour
{
    [Header("레벨 설정")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int baseXPRequired = 100; // 레벨 1->2에 필요한 기본 경험치
    [SerializeField] private float xpMultiplier = 1.5f; // 레벨당 경험치 증가 배율

    public int CurrentLevel => currentLevel;
    public int CurrentXP => currentXP;
    public int XPRequiredForNextLevel => GetXPRequiredForLevel(currentLevel + 1);

    private void Awake()
    {
        // 싱글톤 패턴 (필요시)
        // 다른 곳에서도 접근 가능하도록
    }

    // 경험치 추가
    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        // ExperienceGain 스탯 보너스 적용 (PlayerStats에서 가져오기)
        float experienceGainBonus = GetExperienceGainBonus();
        int finalAmount = Mathf.RoundToInt(amount * (1f + experienceGainBonus / 100f));

        currentXP += finalAmount;
        
        // 경험치 변경 이벤트 발생
        PlayerLevelEvents.InvokeXPChanged(currentXP, XPRequiredForNextLevel);

        // 레벨업 체크
        CheckLevelUp();
    }

    // 레벨업 체크 및 처리
    private void CheckLevelUp()
    {
        int xpRequired = XPRequiredForNextLevel;
        
        while (currentXP >= xpRequired && xpRequired > 0)
        {
            int oldLevel = currentLevel;
            currentXP -= xpRequired;
            currentLevel++;
            
            // 레벨업 이벤트 발생
            PlayerLevelEvents.InvokeLevelUp(currentLevel);
            PlayerLevelEvents.InvokeLevelChanged(oldLevel, currentLevel);
            
            // 다음 레벨 경험치 요구량 업데이트
            xpRequired = XPRequiredForNextLevel;
            
            // 경험치 변경 이벤트 발생 (레벨업 후 남은 경험치 반영)
            PlayerLevelEvents.InvokeXPChanged(currentXP, xpRequired);
        }
    }

    // 특정 레벨에 필요한 경험치 계산
    private int GetXPRequiredForLevel(int level)
    {
        if (level <= 1) return 0;
        
        // 레벨 2부터 시작 (레벨 1->2: baseXPRequired)
        // 레벨 2->3: baseXPRequired * xpMultiplier
        // 레벨 3->4: baseXPRequired * xpMultiplier^2
        // ...
        float multiplier = Mathf.Pow(xpMultiplier, level - 2);
        return Mathf.RoundToInt(baseXPRequired * multiplier);
    }

    // ExperienceGain 보너스 가져오기 (PlayerStats에서)
    private float GetExperienceGainBonus()
    {
        PlayerStats playerStats = GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            return playerStats.GetStat(Items.Enums.StatType.ExperienceGain);
        }
        return 0f;
    }

    // 레벨 설정 (초기화용)
    public void SetLevel(int level, int xp = 0)
    {
        currentLevel = Mathf.Max(1, level);
        currentXP = Mathf.Max(0, xp);
        PlayerLevelEvents.InvokeXPChanged(currentXP, XPRequiredForNextLevel);
    }

    // 경험치 직접 설정 (디버그/테스트용)
    public void SetXP(int xp)
    {
        currentXP = Mathf.Max(0, xp);
        PlayerLevelEvents.InvokeXPChanged(currentXP, XPRequiredForNextLevel);
        CheckLevelUp();
    }
}

