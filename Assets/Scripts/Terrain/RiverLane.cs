using UnityEngine;

/// <summary>
/// 강 레인 - 통나무/platform 필요, 물에 빠지면死亡
/// </summary>
public class RiverLane : BaseLane
{
    [Header("강 설정")]
    [SerializeField] private int laneWidth = 7;
    [SerializeField] private float flowSpeed = 2f;
    [SerializeField] private float spawnInterval = 3f;

    [Header("통나무 Prefabs")]
    [SerializeField] private GameObject[] logPrefabs;

    private float timeSinceLastSpawn = 0f;
    private int direction = 1;

    public override void Initialize(int row)
    {
        base.Initialize(row);
        direction = Random.Range(0, 2) == 0 ? 1 : -1;
        GenerateObstacles();
    }

    /// <summary>
    /// 초기 통나무 생성
    /// </summary>
    public override void GenerateObstacles()
    {
        int initialCount = Random.Range(1, 3);
        for (int i = 0; i < initialCount; i++)
        {
            SpawnLog();
        }
    }

    /// <summary>
    /// 통나무 생성
    /// </summary>
    private void SpawnLog()
    {
        if (logPrefabs == null || logPrefabs.Length == 0)
            return;

        GameObject prefab = logPrefabs[Random.Range(0, logPrefabs.Length)];
        Vector3 spawnPos = new Vector3(
            direction > 0 ? -laneWidth / 2f : laneWidth / 2f,
            0,
            rowIndex
        );

        GameObject log = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        log.tag = "Obstacle";
    }

    /// <summary>
    /// 주기적 통나무 생성
    /// </summary>
    public void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;
        if (timeSinceLastSpawn >= spawnInterval)
        {
            timeSinceLastSpawn = 0f;
            SpawnLog();
        }
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
