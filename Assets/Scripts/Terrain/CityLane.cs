using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 도시 레인 - 도로 + 차량 이동 장애물 + 가로등/방호벽 고정 장애물
/// </summary>
public class CityLane : BaseLane
{
    [Header("도시 레인 설정")]
    [SerializeField] private int laneWidth = 7;        // 차선 폭
    [SerializeField] private float spawnInterval = 2f;  // 차량 생성 간격

    [Header("고정 장애물 설정")]
    [SerializeField] private int minStaticObstacles = 1;   // 최소 고정 장애물 개수
    [SerializeField] private int maxStaticObstacles = 3;   // 최대 고정 장애물 개수
    [SerializeField] private float minObstacleGap = 1.5f;  // 고정 장애물 최소 간격

    [Header("차량 Prefabs")]
    [SerializeField] private GameObject[] vehiclePrefabs;

    [Header("고정 장애물 Prefabs (가로등, 방호벽)")]
    [SerializeField] private GameObject[] staticObstaclePrefabs;

    // 이동 장애물 관리
    private float timeSinceLastSpawn = 0f;
    private int direction = 1;
    private List<GameObject> activeVehicles = new List<GameObject>();

    public override void Initialize(int row)
    {
        base.Initialize(row);

        // 방향 랜덤 결정
        direction = Random.Range(0, 2) == 0 ? 1 : -1;

        // 고정 장애물 생성
        GenerateStaticObstacles();

        // 초기 차량 생성
        GenerateObstacles();
    }

    /// <summary>
    /// 고정 장애물 생성 (가로등, 방호벽)
    /// </summary>
    private void GenerateStaticObstacles()
    {
        if (staticObstaclePrefabs == null || staticObstaclePrefabs.Length == 0)
            return;

        int count = Random.Range(minStaticObstacles, maxStaticObstacles + 1);
        List<float> occupiedPositions = new List<float>();

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = staticObstaclePrefabs[Random.Range(0, staticObstaclePrefabs.Length)];

            // 랜덤 위치 생성 (간격 체크)
            float xPos;
            int attempts = 0;
            do
            {
                xPos = Random.Range(-laneWidth / 2f, laneWidth / 2f);
                attempts++;
            } while (IsPositionOccupied(xPos, occupiedPositions) && attempts < 10);

            occupiedPositions.Add(xPos);

            Vector3 spawnPos = new Vector3(xPos, 0, rowIndex);
            GameObject obstacle = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
            obstacle.tag = "Obstacle";
        }
    }

    /// <summary>
    /// 위치占用 체크
    /// </summary>
    private bool IsPositionOccupied(float newPos, List<float> occupied)
    {
        foreach (float pos in occupied)
        {
            if (Mathf.Abs(newPos - pos) < minObstacleGap)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 초기 차량 생성
    /// </summary>
    public override void GenerateObstacles()
    {
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0)
            return;

        int initialCount = Random.Range(1, 3);
        for (int i = 0; i < initialCount; i++)
        {
            SpawnVehicle();
        }
    }

    /// <summary>
    /// 차량 생성
    /// </summary>
    private void SpawnVehicle()
    {
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0)
            return;

        GameObject prefab = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];
        float startX = direction > 0 ? -laneWidth / 2f - 1f : laneWidth / 2f + 1f;

        Vector3 spawnPos = new Vector3(startX, 0, rowIndex);
        Quaternion rotation = Quaternion.Euler(0, direction > 0 ? 90 : -90, 0);

        GameObject vehicle = Instantiate(prefab, spawnPos, rotation, transform);
        vehicle.tag = "Obstacle";
        activeVehicles.Add(vehicle);
    }

    /// <summary>
    /// 주기적 차량 생성
    /// </summary>
    public void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;
        if (timeSinceLastSpawn >= spawnInterval)
        {
            timeSinceLastSpawn = 0f;
            SpawnVehicle();
        }
    }

    /// <summary>
    /// 풀로 반환
    /// </summary>
    public override void ReleaseToPool()
    {
        // 모든 자식 제거
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        activeVehicles.Clear();
        gameObject.SetActive(false);
        transform.position = Vector3.one * 1000;
    }
}
