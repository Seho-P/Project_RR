using UnityEngine;
using UnityEngine.UI;
using Items.Events;

public class PlayerLevelUI : MonoBehaviour
{
    [SerializeField] private Slider xpBar;
    [SerializeField] private TMPro.TextMeshProUGUI levelText; // 레벨 텍스트 (선택사항)
    
    private void Start()
    {
        // 초기값 설정
        PlayerLevelSystem playerLevelSystem = FindFirstObjectByType<PlayerLevelSystem>();
        if (playerLevelSystem != null)
        {
            xpBar.maxValue = playerLevelSystem.XPRequiredForNextLevel;
            xpBar.value = playerLevelSystem.CurrentXP;
            
            if (levelText != null)
            {
                levelText.text = $"Lv.{playerLevelSystem.CurrentLevel}";
            }
        }
        
        // 이벤트 구독
        PlayerLevelEvents.OnXPChanged += UpdateXPBar;
        PlayerLevelEvents.OnLevelChanged += UpdateLevelText;
    }
    
    private void OnDestroy()
    {
        PlayerLevelEvents.OnXPChanged -= UpdateXPBar;
        PlayerLevelEvents.OnLevelChanged -= UpdateLevelText;
    }
    
    private void UpdateXPBar(int currentXP, int maxXP)
    {
        xpBar.maxValue = maxXP;
        xpBar.value = currentXP;
    }
    
    private void UpdateLevelText(int oldLevel, int newLevel)
    {
        if (levelText != null)
        {
            levelText.text = $"Lv.{newLevel}";
        }
    }
}