using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 도시 레인 - 차량 이동 장애물 (좌→우) + 가로등/방호벽 고정 장애물
/// </summary>
public class RoadLane : BaseLane
{
    [Header("레이너 설정")]
    [SerializeField] private int laneWidth = 7;
    [SerializeField] private float leftBoundary = -8f;
    [SerializeField] private float rightBoundary = 8f;

    [Header("고정 장애물 (FixedOnly)")]
    [SerializeField] private GameObject[] fixedObstaclePrefabs;  // 가로등, 방호벽
    [SerializeField] private int minFixedCount = 2;
    [SerializeField] private int maxFixedCount = 4;
    [SerializeField] private float minGap = 1.5f;

    [Header("이동 장애물 (MovingOnly)")]
    [SerializeField] private GameObject[] movingObstaclePrefabs;  // 차량 종류별
    [SerializeField] private float[] speeds;                       // 각 종류의 속도 [느림, 보통, 빠름]
    [SerializeField] private float spawnInterval = 2f;

    // 장애물 타입
    private ObstacleType currentType = ObstacleType.FixedOnly;

    // 고정 장애물 관리
    private List<GameObject> fixedObstacles = new List<GameObject>();

    // 이동 장애물 관리 (메모리 최적화: Spawn/Despawn)
    private float timeSinceLastSpawn = 0f;

    public override void Initialize(int row)
    {
        Initialize(row, ObstacleType.FixedOnly);
    }

    /// <summary>
    /// ObstacleType 기반 초기화
    /// </summary>
    public void Initialize(int row, ObstacleType type)
    {
        base.Initialize(row);
        currentType = type;

        // 기존 장애물 제거
        ClearAllObstacles();

        if (type == ObstacleType.FixedOnly)
        {
            GenerateFixedObstacles();
        }
        else // MovingOnly
        {
            timeSinceLastSpawn = 0f;
        }
    }

    /// <summary>
    /// 레인 타입별 장애물 생성 (BaseLane 추상 메서드 구현 - ObstacleType 기반)
    /// </summary>
    public override void GenerateObstacles()
    {
        // ObstacleType 기반 초기화에서 처리되므로 여기서는 빈 구현
        //子类에서 Initialize(row, type)로 처리
    }

    /// <summary>
    /// 고정 장애물 생성 (균형 잡힌 랜덤 배치)
    /// </summary>
    private void GenerateFixedObstacles()
    {
        if (fixedObstaclePrefabs == null || fixedObstaclePrefabs.Length == 0)
            return;

        int count = Random.Range(minFixedCount, maxFixedCount + 1);
        List<float> occupiedPositions = new List<float>();

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = fixedObstaclePrefabs[Random.Range(0, fixedObstaclePrefabs.Length)];

            // 최소 간격 유지ながら 랜덤 위치 생성
            float xPos;
            int attempts = 0;
            do
            {
                xPos = Random.Range(-laneWidth / 2f, laneWidth / 2f);
                attempts++;
            } while (IsPositionOccupied(xPos, occupiedPositions) && attempts < 20);

            occupiedPositions.Add(xPos);

            Vector3 spawnPos = new Vector3(xPos, 0, rowIndex);
            GameObject obstacle = Object.Instantiate(prefab, spawnPos, Quaternion.identity, transform);
            obstacle.tag = "Obstacle";
            fixedObstacles.Add(obstacle);
        }
    }

    /// <summary>
    /// 위치占用 체크 (최소 간격)
    /// </summary>
    private bool IsPositionOccupied(float newPos, List<float> occupied)
    {
        foreach (float pos in occupied)
        {
            if (Mathf.Abs(newPos - pos) < minGap)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 이동 장애물 생성 (화면 왼쪽 밖에서Spawn)
    /// </summary>
    private void SpawnMovingObstacle()
    {
        if (movingObstaclePrefabs == null || movingObstaclePrefabs.Length == 0)
            return;

        // 랜덤 종류 선택 (속도도 해당 종류의 것으로 고정)
        int typeIndex = Random.Range(0, movingObstaclePrefabs.Length);
        GameObject prefab = movingObstaclePrefabs[typeIndex];
        float speed = (speeds != null && typeIndex < speeds.Length) ? speeds[typeIndex] : 4f;

        // 화면 왼쪽 밖에서 Spawn
        Vector3 spawnPos = new Vector3(leftBoundary - 1f, 0, rowIndex);
        Quaternion rotation = Quaternion.Euler(0, 0, 0); // 좌→우이므로 회전 없음

        GameObject obstacle = Object.Instantiate(prefab, spawnPos, rotation, transform);
        obstacle.tag = "Obstacle";

        // 이동 컴포넌트 설정
        var mover = obstacle.AddComponent<MovingObstacleMover>();
        mover.Initialize(speed, rightBoundary + 2f); // 제거 경계
    }

    /// <summary>
    /// 주기적 이동 장애물 생성 (MovingOnly만)
    /// </summary>
    public void Update()
    {
        if (currentType != ObstacleType.MovingOnly)
            return;

        timeSinceLastSpawn += Time.deltaTime;
        if (timeSinceLastSpawn >= spawnInterval)
        {
            timeSinceLastSpawn = 0f;
            SpawnMovingObstacle();
        }
    }

    /// <summary>
    /// 모든 장애물 제거
    /// </summary>
    private void ClearAllObstacles()
    {
        foreach (Transform child in transform)
        {
            Object.Destroy(child.gameObject);
        }
        fixedObstacles.Clear();
    }

    /// <summary>
    /// 풀로 반환
    /// </summary>
    public override void ReleaseToPool()
    {
        ClearAllObstacles();
        gameObject.SetActive(false);
        transform.position = Vector3.one * 1000;
    }
}