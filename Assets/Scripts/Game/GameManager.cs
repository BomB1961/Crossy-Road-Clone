using UnityEngine;

/// <summary>
/// 게임 매니저 - 게임 상태 및 점수 관리
/// </summary>
public class GameManager : MonoBehaviour, IGameManager
{
    [Header("점수 설정")]
    [SerializeField] private int maxScore = 9999;
    [SerializeField] private int startScore = 0;

    private GameState currentState = GameState.Ready;
    private int score = 0;
    private int highScore = 0;

    public GameState CurrentState => currentState;
    public int Score => score;
    public int HighScore => highScore;

    public event System.Action<int> OnScoreChanged;
    public event System.Action OnGameStarted;
    public event System.Action OnGameOver;

    private void Start()
    {
        // 최고 점수 로드 (PlayerPrefs)
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        Reset();
    }

    /// <summary>
    /// 게임 시작
    /// </summary>
    public void StartGame()
    {
        currentState = GameState.Playing;
        OnGameStarted?.Invoke();
    }

    /// <summary>
    /// 게임 오버 처리
    /// </summary>
    public void GameOver()
    {
        if (currentState != GameState.Playing)
            return;

        currentState = GameState.GameOver;

        // 최고 점수 업데이트
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        OnGameOver?.Invoke();
    }

    /// <summary>
    /// 점수 추가
    /// </summary>
    public void AddScore(int amount)
    {
        if (currentState != GameState.Playing)
            return;

        score = Mathf.Min(score + amount, maxScore);
        OnScoreChanged?.Invoke(score);
    }

    /// <summary>
    /// 게임 리셋
    /// </summary>
    public void Reset()
    {
        score = startScore;
        currentState = GameState.Ready;
    }
}
