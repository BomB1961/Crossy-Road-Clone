using UnityEngine;

/// <summary>
/// 플레이어 조종 인터페이스
/// </summary>
public interface IPlayerController
{
    /// <summary>현재 위치 (_ROW)</summary>
    int CurrentRow { get; }

    /// <summary>현재 위치 (COL)</summary>
    int CurrentCol { get; }

    /// <summary>게임 오버 상태</summary>
    bool IsDead { get; }

    /// <summary>
    /// 지정한 행으로 이동 (앞/뒤)
    /// </summary>
    void MoveRow(int delta);

    /// <summary>
    /// 지정한 열로 이동 (좌/우)
    /// </summary>
    void MoveCol(int delta);

    /// <summary>
    /// 점프 애니메이션 트리거
    /// </summary>
    void Jump();

    /// <summary>
    /// 플레이어 위치 초기화
    /// </summary>
    void ResetPosition();

    /// <summary>
    /// 게임 오버 처리
    /// </summary>
    void Die();
}
