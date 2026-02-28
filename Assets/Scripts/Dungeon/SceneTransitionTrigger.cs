using UnityEngine;

/// <summary>
/// 플레이어 상호작용으로 지정된 씬 라우트로 전환하는 범용 트리거.
/// </summary>
public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private SceneRoute targetRoute = SceneRoute.Dungeon;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";
    private bool isPlayerInRange;

    /// <summary>
    /// 플레이어가 범위 안에서 상호작용 키를 누르면 설정된 라우트로 씬을 전환한다.
    /// </summary>
    private void Update()
    {
        if (!isPlayerInRange)
        {
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            Debug.Log($"Player pressed the interact key. route={targetRoute}");
            SceneFlowManager.Instance.LoadByRoute(targetRoute);
        }
    }

    /// <summary>
    /// 플레이어가 트리거 범위에 들어오면 상호작용 가능 상태로 전환한다.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            Debug.Log("Player entered scene transition trigger");
        }
    }

    /// <summary>
    /// 플레이어가 트리거 범위를 벗어나면 상호작용 불가 상태로 전환한다.
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
        }
    }
}
