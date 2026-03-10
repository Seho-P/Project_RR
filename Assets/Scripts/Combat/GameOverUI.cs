using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUI;

    /// <summary>
    /// 시작 시 패널을 숨기고 UIManager에 등록한다.
    /// </summary>
    private void Start()
    {
        UIManager.Instance.RegisterDeathPanel(gameOverUI);
        Hide();
    }

    /// <summary>
    /// 게임오버 패널을 표시한다.
    /// </summary>
    public void Show()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
    }

    /// <summary>
    /// 게임오버 패널을 숨긴다.
    /// </summary>
    public void Hide()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    /// <summary>
    /// Town 버튼 클릭 시 시간을 재개하고 마을로 이동한다.
    /// </summary>
    public void OnClickTownButton()
    {
        Time.timeScale = 1f;
        Hide();
        SceneFlowManager.Instance.ReturnToTown();
    }
}