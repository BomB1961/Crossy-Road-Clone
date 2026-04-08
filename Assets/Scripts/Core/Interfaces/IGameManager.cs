/// <summary>
/// 게임 상태 열거형
/// </summary>
public enum GameState
{
    Ready,      // 준비 상태
    Playing,    // 플레이 중
    GameOver    // 게임 오버
}

/// <summary>
/// 게임 매니저 인터페이스
/// </summary>
public interface IGameManager
{
    /// <summary>현재 게임 상태</summary>
    GameState CurrentState { get; }

    /// <summary>현재 점수</summary>
    int Score { get; }

    /// <summary>최고 점수</summary>
    int HighScore { get; }

    /// <summary>
    /// 게임 시작
    /// </summary>
    void StartGame();

    /// <summary>
    /// 게임 오버 처리
    /// </summary>
    void GameOver();

    /// <summary>
    /// 점수 추가 (전진 시 호출)
    /// </summary>
    void AddScore(int amount);

    /// <summary>
    /// 게임 리셋
    /// </summary>
    void Reset();
}
