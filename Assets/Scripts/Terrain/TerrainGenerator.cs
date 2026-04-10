using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 테마 구간 열거형
/// </summary>
public enum ZoneType
{
    Forest,  // 잔디 (GrassLane)
    City     // 도로 (RoadLane)
}

/// <summary>
/// 레인 생성기 - 구간별 테마 기반 레인 생성
/// </summary>
public class TerrainGenerator : MonoBehaviour, ITerrainGenerator
{
    [Header("레인 설정")]
    [SerializeField] private int visibleLaneCount = 9;
    [SerializeField] private float laneHeight = 1f;

    [Header("구간 설정")]
    [SerializeField] private int lanesPerZone = 9;  // 구간당 레인 수

    // 구간 관리
    private ZoneType currentZone = ZoneType.Forest;
    private int lanesInCurrentZone = 0;

    // 레인 관리
    private ObjectPool<BaseLane> lanePool;
    private List<BaseLane> activeLanes = new List<BaseLane>();
    private int currentRow = 0;

    // 레인 프리팹
    private BaseLane grassLanePrefab;
    private BaseLane roadLanePrefab;

    public int LaneCount => activeLanes.Count;

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize(int count, float height, ObjectPool<BaseLane> pool)
    {
        visibleLaneCount = count;
        laneHeight = height;
        lanePool = pool;

        // 프리팹 로드 ( Hierarchy에서 찾기 - 추후Resources.Load로 변경 가능)
        LoadLanePrefabs();
    }

    /// <summary>
    /// 레인 프리팹 로드
    /// </summary>
    private void LoadLanePrefabs()
    {
        //Resources 폴더에서 로드
        grassLanePrefab = Resources.Load<BaseLane>("Prefabs/Lanes/GrassLane");
        roadLanePrefab = Resources.Load<BaseLane>("Prefabs/Lanes/RoadLane");

        #if UNITY_EDITOR
        if (grassLanePrefab == null)
            Debug.LogWarning("[TerrainGenerator] GrassLane 프리팹을 찾을 수 없습니다. Resources/Prefabs/Lanes/에 배치하세요.");
        if (roadLanePrefab == null)
            Debug.LogWarning("[TerrainGenerator] RoadLane 프리팹을 찾을 수 없습니다. Resources/Prefabs/Lanes/에 배치하세요.");
        #endif
    }

    /// <summary>
    /// 특정 행의 레인 타입 반환
    /// </summary>
    public LaneType GetLaneTypeAtRow(int row)
    {
        var lane = activeLanes.Find(l => l.RowIndex == row);
        return lane != null ? lane.LaneType : LaneType.Grass;
    }

    /// <summary>
    /// 지정한 행에 레인 생성
    /// </summary>
    public void GenerateLaneAtRow(int row)
    {
        LaneType type = DecideLaneType();
        BaseLane lane = RentLane(type);
        lane.Initialize(row);
        activeLanes.Add(lane);

        // 구간 내 레인 카운트 증가
        IncrementLaneCount();

        currentRow = Mathf.Max(currentRow, row);
    }

    /// <summary>
    /// 특정 행의 레인 제거
    /// </summary>
    public void RemoveLaneAtRow(int row)
    {
        var lane = activeLanes.Find(l => l.RowIndex == row);
        if (lane != null)
        {
            activeLanes.Remove(lane);
            lane.ReleaseToPool();
        }
    }

    /// <summary>
    /// 레인 장애물 활성화/비활성화
    /// </summary>
    public void SetObstaclesActive(int row, bool active)
    {
        var lane = activeLanes.Find(l => l.RowIndex == row);
        lane?.SetObstaclesActive(active);
    }

    /// <summary>
    /// 모든 레인 초기화
    /// </summary>
    public void ResetAll()
    {
        foreach (var lane in activeLanes)
        {
            lane.ReleaseToPool();
        }
        activeLanes.Clear();
        currentRow = 0;
        currentZone = ZoneType.Forest;
        lanesInCurrentZone = 0;
    }

    /// <summary>
    /// 구간 기반 레인 타입 결정
    /// Forest → GrassLane, City → RoadLane
    /// </summary>
    private LaneType DecideLaneType()
    {
        // 구간당 레인 수 도달 시 구간 전환
        if (lanesInCurrentZone >= lanesPerZone)
        {
            SwitchZone();
        }

        // 현재 구간에 맞는 레인 타입 반환
        return currentZone == ZoneType.Forest ? LaneType.Grass : LaneType.Road;
    }

    /// <summary>
    /// 구간 전환 (Forest ↔ City)
    /// </summary>
    private void SwitchZone()
    {
        currentZone = currentZone == ZoneType.Forest ? ZoneType.City : ZoneType.Forest;
        lanesInCurrentZone = 0;

        Debug.Log($"[TerrainGenerator] 구간 전환: {currentZone}");
    }

    /// <summary>
    /// 레인 생성 후 카운트 증가
    /// </summary>
    private void IncrementLaneCount()
    {
        lanesInCurrentZone++;
    }

    /// <summary>
    /// 풀에서 레인租借
    /// </summary>
    private BaseLane RentLane(LaneType type)
    {
        BaseLane prefab = GetPrefabForType(type);

        // 프리팹이 있으면 풀에서租借, 없으면 동적 생성
        if (prefab != null)
        {
            return lanePool.Rent();
        }

        // 프리팹 없음: 구체적 타입 GameObject 생성 (폴백)
        #if UNITY_EDITOR
        Debug.LogWarning($"[TerrainGenerator] {type} 프리팹 없음 - 동적 생성");
        #endif

        var laneObj = new GameObject(type.ToString() + "Lane");
        laneObj.transform.parent = transform;

        switch (type)
        {
            case LaneType.Grass:
                return laneObj.AddComponent<GrassLane>();
            case LaneType.Road:
                return laneObj.AddComponent<RoadLane>();
            default:
                return laneObj.AddComponent<GrassLane>();
        }
    }

    /// <summary>
    /// 타입별 프리팹 반환
    /// </summary>
    private BaseLane GetPrefabForType(LaneType type)
    {
        switch (type)
        {
            case LaneType.Grass: return grassLanePrefab;
            case LaneType.Road: return roadLanePrefab;
            default: return grassLanePrefab;
        }
    }
}
