using UnityEngine;

/// <summary>
/// 씬 내 플레이어 스폰 위치 마커.
/// SceneFlowManager가 spawnId를 기준으로 위치를 찾는다.
/// </summary>
public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnId = "Default";
    [SerializeField] private bool isDefault;

    public string SpawnId => spawnId;
    public bool IsDefault => isDefault;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = isDefault ? Color.green : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.35f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.8f);
    }
#endif
}
