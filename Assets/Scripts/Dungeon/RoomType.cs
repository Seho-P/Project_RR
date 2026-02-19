/// <summary>
/// 던전 방의 종류를 정의하는 열거형
/// </summary>
public enum RoomType
{
    Start,      // 시작 방 (일반방 프리팹 사용)
    Normal,     // 일반 전투 방
    Treasure,   // 보물 방 (막다른 길에 배치)
    Boss        // 보스 방 (시작점에서 가장 먼 방)
}
