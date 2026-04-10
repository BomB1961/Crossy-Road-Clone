using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 강 레인 - 물 + 배(보트) 이동 장애물 + 고정 장애물 없음
/// </summary>
public class RiverLane : BaseLane
{
    [Header("강 레인 설정")]
    [SerializeField] private int laneWidth = 7;        //レーン幅
    [SerializeField] private float spawnInterval = 2.5f; // 배 생성 간격

    [Header("배 Prefabs")]
    [SerializeField] private GameObject[] boatPrefabs;

    // 이동 장애물 관리
    private float timeSinceLastSpawn = 0f;
    private int direction = 1;
    private List<GameObject> activeBoats = new List<GameObject>();

    public override void Initialize(int row)
    {
        base.Initialize(row);

        // 방향 랜덤 결정
        direction = Random.Range(0, 2) == 0 ? 1 : -1;

        // 고정 장애물 없음 (River는 이동 장애물만)

        // 초기 배 생성
        GenerateObstacles();
    }

    /// <summary>
    /// 초기 배 생성
    /// </summary>
    public override void GenerateObstacles()
    {
        if (boatPrefabs == null || boatPrefabs.Length == 0)
            return;

        int initialCount = Random.Range(1, 2);
        for (int i = 0; i < initialCount; i++)
        {
            SpawnBoat();
        }
    }

    /// <summary>
    /// 배 생성
    /// </summary>
    private void SpawnBoat()
    {
        if (boatPrefabs == null || boatPrefabs.Length == 0)
            return;

        GameObject prefab = boatPrefabs[Random.Range(0, boatPrefabs.Length)];
        float startX = direction > 0 ? -laneWidth / 2f - 1f : laneWidth / 2f + 1f;

        Vector3 spawnPos = new Vector3(startX, 0, rowIndex);
        Quaternion rotation = Quaternion.Euler(0, direction > 0 ? 90 : -90, 0);

        GameObject boat = Instantiate(prefab, spawnPos, rotation, transform);
        boat.tag = "Obstacle";
        activeBoats.Add(boat);
    }

    /// <summary>
    /// 주기적 배 생성
    /// </summary>
    public void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;
        if (timeSinceLastSpawn >= spawnInterval)
        {
            timeSinceLastSpawn = 0f;
            SpawnBoat();
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

        activeBoats.Clear();
        gameObject.SetActive(false);
        transform.position = Vector3.one * 1000;
    }
}
