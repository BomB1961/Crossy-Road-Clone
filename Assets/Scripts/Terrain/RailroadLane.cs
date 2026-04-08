using UnityEngine;

/// <summary>
/// 철도 레인 - 기차 통과, 충돌 시死亡
/// </summary>
public class RailroadLane : BaseLane
{
    [Header("레일 설정")]
    [SerializeField] private float trainSpeed = 5f;
    [SerializeField] private float spawnInterval = 5f;

    [Header("기차 Prefabs")]
    [SerializeField] private GameObject[] trainPrefabs;

    private float timeSinceLastSpawn = 0f;
    private int direction = 1;

    public override void Initialize(int row)
    {
        base.Initialize(row);
        direction = Random.Range(0, 2) == 0 ? 1 : -1;
        GenerateObstacles();
    }

    /// <summary>
    /// 초기 기차 생성 (레일 중앙)
    /// </summary>
    public override void GenerateObstacles()
    {
        // 기차 레인에서는 초기 장애물 없음 (기차가 지나감)
    }

    /// <summary>
    /// 주기적 기차 생성
    /// </summary>
    public void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;
        if (timeSinceLastSpawn >= spawnInterval)
        {
            timeSinceLastSpawn = 0f;
            SpawnTrain();
        }
    }

    /// <summary>
    /// 기차 생성
    /// </summary>
    private void SpawnTrain()
    {
        if (trainPrefabs == null || trainPrefabs.Length == 0)
            return;

        GameObject prefab = trainPrefabs[Random.Range(0, trainPrefabs.Length)];
        Vector3 spawnPos = new Vector3(
            direction > 0 ? -10f : 10f, // 화면 밖에서 시작
            0,
            rowIndex
        );

        GameObject train = Instantiate(prefab, spawnPos, Quaternion.Euler(0, direction > 0 ? 90 : -90, 0), transform);
        train.tag = "Obstacle";
    }

    /// <summary>
    /// 풀로 반환
    /// </summary>
    public override void ReleaseToPool()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        gameObject.SetActive(false);
        transform.position = Vector3.one * 1000;
    }
}
