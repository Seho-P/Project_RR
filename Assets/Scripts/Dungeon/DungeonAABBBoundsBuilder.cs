using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 생성된 방 그리드 집합을 기반으로 AABB 카메라 경계를 만든다.
/// 결과는 BoxCollider2D 하나로 표현되며 Cinemachine Confiner 2D의 Bounding Shape로 사용할 수 있다.
/// </summary>
public class DungeonAABBBoundsBuilder : MonoBehaviour
{
    [Header("=== 경계 오브젝트 ===")]
    [Tooltip("자동 생성/갱신할 경계 오브젝트 이름")]
    [SerializeField] private string boundaryObjectName = "CameraBoundary";

    [Tooltip("방 전체 외곽에 추가할 여백 값 (월드 단위)")]
    [SerializeField] private float padding = 0f;
    [SerializeField] private GameObject cameraBoundary;

    /// <summary>
    /// 방 데이터와 방 크기를 기준으로 AABB 경계를 재생성한다.
    /// </summary>
    public void RebuildBounds(IReadOnlyDictionary<Vector2Int, RoomData> roomMap, Vector2 roomSize)
    {
        if (roomMap == null || roomMap.Count == 0)
        {
            cameraBoundary.SetActive(false);
            return;
        }

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (var kvp in roomMap)
        {
            Vector2 roomCenter = new Vector2(kvp.Key.x * roomSize.x, kvp.Key.y * roomSize.y);
            Vector2 roomMin = roomCenter - roomSize * 0.5f;
            Vector2 roomMax = roomCenter + roomSize * 0.5f;

            min = Vector2.Min(min, roomMin);
            max = Vector2.Max(max, roomMax);
        }

        min -= Vector2.one * padding;
        max += Vector2.one * padding;

        Vector2 center = (min + max) * 0.5f;
        Vector2 size = max - min;

        cameraBoundary.transform.position = new Vector3(center.x, center.y, 0f);
        cameraBoundary.transform.localScale = new Vector3(size.x, size.y, 1f);
    }

}
