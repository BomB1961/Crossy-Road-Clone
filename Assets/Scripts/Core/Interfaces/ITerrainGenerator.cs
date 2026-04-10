/// <summary>
/// 레인 타입 열거형
/// </summary>
public enum LaneType
{
    Grass,      // 숲 (안전) - 동물 이동 장애물
    Road,       // 도시 (차량) - 가로등/방호벽 고정 장애물
    River,      // 강 (배) - 고정 장애물 없음
    Space       // 우주 (우주선) - 암석/안테나 고정 장애물
}

/// <summary>
/// 레인 생성 인터페이스
/// </summary>
public interface ITerrainGenerator
{
    /// <summary>현재 생성된 레인 수</summary>
    int LaneCount { get; }

    /// <summary>
    /// 지정한 행 위치에 해당하는 레인 타입 반환
    /// </summary>
    LaneType GetLaneTypeAtRow(int row);

    /// <summary>
    /// 새로운 레인 생성 (화면 위로)
    /// </summary>
    void GenerateLaneAtRow(int row);

    /// <summary>
    /// 오래된 레인 제거 (화면 아래로)
    /// </summary>
    void RemoveLaneAtRow(int row);

    /// <summary>
    /// 특정 레인의 장애물 활성화/비활성화
    /// </summary>
    void SetObstaclesActive(int row, bool active);

    /// <summary>
    /// 모든 레인 초기화
    /// </summary>
    void ResetAll();
}
