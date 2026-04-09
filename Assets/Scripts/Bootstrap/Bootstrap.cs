using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// DI 컨테이너 역할 - 의존성 주입 및 게임 초기화
/// </summary>
public class Bootstrap : MonoBehaviour
{
    [Header("게임 설정")]
    [SerializeField] private int startRow = 0;
    [SerializeField] private int visibleLaneCount = 9;
    [SerializeField] private float laneHeight = 1f;

    [Header("레퍼런스")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private GameObject playerPrefab;

    // DI 컨테이너 (인터페이스 → 구현체 매핑)
    private Dictionary<System.Type, object> container = new Dictionary<System.Type, object>();

    // 게임 매니저
    private IGameManager gameManager;
    private IPlayerController playerController;
    private ITerrainGenerator terrainGenerator;

    // 오브젝트 풀
    private ObjectPool<BaseLane> lanePool;
    private ObjectPool<BaseObstacle> obstaclePool;

    public static Bootstrap Instance { get; private set; }

    private void Awake()
    {
        // 싱글톤 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeContainer();
    }

    private void Start()
    {
        InitializePools();
        SetupGame();
    }

    /// <summary>
    /// DI 컨테이너 초기화 - 인터페이스 매핑
    /// </summary>
    private void InitializeContainer()
    {
        // 실제 구현체는 Start에서 초기화되지만,
        // 참조는 인터페이스를 통해 이루어짐
    }

    /// <summary>
    /// 오브젝트 풀 초기화
    /// </summary>
    private void InitializePools()
    {
        // 레인 풀 초기화
        lanePool = new ObjectPool<BaseLane>(
            () => CreateNewLane(),
            (lane) => lane.gameObject.SetActive(true),
            (lane) => lane.gameObject.SetActive(false)
        );

        // 장애물 풀 초기화
        obstaclePool = new ObjectPool<BaseObstacle>(
            () => CreateNewObstacle(),
            (obs) => obs.gameObject.SetActive(true),
            (obs) => obs.gameObject.SetActive(false)
        );
    }

    /// <summary>
    /// 게임 설정 및 초기화
    /// </summary>
    private void SetupGame()
    {
        // 게임 매니저 설정 (GameManager 컴포넌트 필요)
        var gameManagerObj = new GameObject("GameManager");
        gameManagerObj.transform.parent = transform;
        var gameManagerComp = gameManagerObj.AddComponent<GameManager>();
        gameManager = gameManagerComp;

        // 플레이어 컨트롤러 설정
        // 먼저 scene에서 기존 PlayerController 찾기
        var existingPlayer = FindObjectOfType<PlayerController>();
        if (existingPlayer != null)
        {
            // 에디터 작업 중: 기존 Player 사용
            playerController = existingPlayer;
        }
        else if (playerPrefab != null && playerSpawnPoint != null)
        {
            // 게임 플레이 시: PlayerPrefab에서 생성
            var playerObj = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
            playerController = playerObj.GetComponent<IPlayerController>();
        }

        // 지형 생성기 설정
        var terrainObj = new GameObject("TerrainGenerator");
        terrainObj.transform.parent = transform;
        var terrainComp = terrainObj.AddComponent<TerrainGenerator>();
        terrainComp.Initialize(visibleLaneCount, laneHeight, lanePool);
        terrainGenerator = terrainComp;

        // DI 주입
        RegisterDependency<IGameManager>(gameManager);
        RegisterDependency<IPlayerController>(playerController);
        RegisterDependency<ITerrainGenerator>(terrainGenerator);
        RegisterDependency<ObjectPool<BaseLane>>(lanePool);
        RegisterDependency<ObjectPool<BaseObstacle>>(obstaclePool);

        // 초기 레인 생성
        for (int i = 0; i < visibleLaneCount; i++)
        {
            terrainGenerator.GenerateLaneAtRow(startRow + i);
        }
    }

    /// <summary>
    /// DI 컨테이너에 의존성 등록
    /// </summary>
    public void RegisterDependency<T>(T implementation) where T : class
    {
        container[typeof(T)] = implementation;
    }

    /// <summary>
    /// DI 컨테이너에서 의존성 해결
    /// </summary>
    public T Resolve<T>() where T : class
    {
        if (container.TryGetValue(typeof(T), out var value))
        {
            return (T)value;
        }
        return null;
    }

    /// <summary>
    /// 새 레인 생성 (풀링용) - 구체적인 레인 타입 사용
    /// </summary>
    private BaseLane CreateNewLane()
    {
        var laneObj = new GameObject("Lane");
        BaseLane lane = laneObj.AddComponent<GrassLane>(); // 기본값
        return lane;
    }

    /// <summary>
    /// 새 장애물 생성 (풀링용)
    /// </summary>
    private BaseObstacle CreateNewObstacle()
    {
        var obsObj = new GameObject("Obstacle");
        BaseObstacle obs = obsObj.AddComponent<BaseObstacle>(); // BaseObstacle은 abstract 불가
        return obs;
    }

    /// <summary>
    /// 게임 리셋
    /// </summary>
    public void ResetGame()
    {
        terrainGenerator.ResetAll();
        playerController.ResetPosition();
        gameManager.Reset();

        for (int i = 0; i < visibleLaneCount; i++)
        {
            terrainGenerator.GenerateLaneAtRow(startRow + i);
        }
    }
}
