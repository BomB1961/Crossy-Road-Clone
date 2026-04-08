/// <summary>
/// 레인 타입 열거형
/// </summary>
public enum LaneType
{
    Grass,      // 풀밭 (안전)
    Road,       // 도로 (차량)
    River,      // 강 (통나무)
    Railroad    // 레일 (기차)
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
