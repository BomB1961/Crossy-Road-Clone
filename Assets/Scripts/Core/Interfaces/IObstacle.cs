using UnityEngine;

/// <summary>
/// 장애물 기본 인터페이스
/// </summary>
public interface IObstacle
{
    /// <summary>활성 상태</summary>
    bool IsActive { get; }

    /// <summary>위치 (_ROW)</summary>
    int Row { get; }

    /// <summary>위치 (COL)</summary>
    int Col { get; }

    /// <summary>
    /// 장애물 초기화
    /// </summary>
    void Initialize(int row, int col, float speed);

    /// <summary>
    /// 장애물 활성화
    /// </summary>
    void Activate();

    /// <summary>
    /// 장애물 비활성화
    /// </summary>
    void Deactivate();

    /// <summary>
    /// 이동 로직 (자식 클래스에서 구현)
    /// </summary>
    void Move();
}
