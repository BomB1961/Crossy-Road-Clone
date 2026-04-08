using UnityEngine;

/// <summary>
/// 도로 레인 - 차량 장애물 발생
/// </summary>
public class RoadLane : BaseLane
{
    [Header("도로 설정")]
    [SerializeField] private int laneWidth = 7;        // 차선 수 (홀수)
    [SerializeField] private float vehicleSpeed = 3f;   // 차량 속도
    [SerializeField] private float spawnInterval = 2f;  // 생성 간격

    [Header("차량Prefabs")]
    [SerializeField] private GameObject[] vehiclePrefabs;

    private float timeSinceLastSpawn = 0f;
    private int direction = 1; // 1 = 오른쪽, -1 = 왼쪽

    public override void Initialize(int row)
    {
        base.Initialize(row);
        direction = Random.Range(0, 2) == 0 ? 1 : -1;
        GenerateObstacles();
    }

    /// <summary>
    /// 초기 장애물 생성
    /// </summary>
    public override void GenerateObstacles()
    {
        // 초기 차량 1~2대 배치
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
        Vector3 spawnPos = new Vector3(
            direction > 0 ? -laneWidth / 2f : laneWidth / 2f,
            0,
            rowIndex
        );

        GameObject vehicle = Instantiate(prefab, spawnPos, Quaternion.Euler(0, direction > 0 ? 90 : -90, 0), transform);
        vehicle.tag = "Obstacle"; // 충돌 감지용 태그
    }

    /// <summary>
    /// 주기적 차량 생성 (게임 중)
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
        // 자식 차량 모두 제거
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        gameObject.SetActive(false);
        transform.position = Vector3.one * 1000;
    }
}
