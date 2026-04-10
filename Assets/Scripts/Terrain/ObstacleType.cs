/// <summary>
/// 레인 장애물 타입
/// </summary>
public enum ObstacleType
{
    FixedOnly,   // 고정 장애물만 (랜덤 개수, 랜덤 위치, 최소 간격 유지)
    MovingOnly   // 이동 장애물만 (좌→우만 이동, 화면 밖 즉시 제거)
}
