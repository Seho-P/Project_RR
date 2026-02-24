using UnityEngine;

/// <summary>
/// UI 버튼에서 씬 흐름 API를 쉽게 호출하기 위한 헬퍼.
/// </summary>
public class SceneFlowButtonActions : MonoBehaviour
{
    public void StartGame()
    {
        SceneFlowManager.Instance.StartGame();
    }

    public void GoToStartScreen()
    {
        SceneFlowManager.Instance.GoToStartScreen();
    }

    public void GoToDungeon()
    {
        SceneFlowManager.Instance.GoToDungeon();
    }

    public void ReturnToTown()
    {
        SceneFlowManager.Instance.ReturnToTown();
    }
}
