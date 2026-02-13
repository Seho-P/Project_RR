using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 던전 그래프에서 하나의 방을 나타내는 데이터 클래스.
/// 그리드 좌표, 방 타입, 인접 연결 정보, 시작점으로부터의 거리를 관리한다.
/// </summary>
public class RoomData
{
    public Vector2Int GridPosition { get; private set; }
    public RoomType RoomType { get; set; }
    public int DistanceFromStart { get; set; }

    /// <summary>
    /// 이 방과 연결된 인접 방들의 그리드 좌표 집합
    /// </summary>
    public HashSet<Vector2Int> ConnectedPositions { get; private set; } = new HashSet<Vector2Int>();

    public RoomData(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
        RoomType = RoomType.Normal;
        DistanceFromStart = 0;
    }

    /// <summary>
    /// 인접 방과의 연결을 추가한다.
    /// </summary>
    public void AddConnection(Vector2Int neighborPosition)
    {
        ConnectedPositions.Add(neighborPosition);
    }

    /// <summary>
    /// 특정 방향에 연결된 방이 있는지 확인한다.
    /// </summary>
    /// <param name="direction">확인할 방향 (예: Vector2Int.up)</param>
    public bool HasConnectionInDirection(Vector2Int direction)
    {
        return ConnectedPositions.Contains(GridPosition + direction);
    }

    /// <summary>
    /// 연결이 1개뿐인 막다른 길인지 여부
    /// </summary>
    public bool IsDeadEnd => ConnectedPositions.Count == 1;
}
