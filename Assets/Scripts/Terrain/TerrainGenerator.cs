using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 레인 생성기 - 랜덤 레인 타입 생성 및 관리
/// </summary>
public class TerrainGenerator : MonoBehaviour, ITerrainGenerator
{
    [Header("레인 설정")]
    [SerializeField] private int visibleLaneCount = 9;
    [SerializeField] private float laneHeight = 1f;
    [SerializeField] private float generateAheadDistance = 5f; //Ahead多远生成

    [Header("레인 타입 가중치")]
    [SerializeField] private int grassWeight = 40;
    [SerializeField] private int roadWeight = 30;
    [SerializeField] private int riverWeight = 15;
    [SerializeField] private int railroadWeight = 15;

    // 레인 관리
    private ObjectPool<BaseLane> lanePool;
    private List<BaseLane> activeLanes = new List<BaseLane>();
    private int currentRow = 0;

    // 레인 프리팹 (추후 할당)
    private BaseLane grassLanePrefab;
    private BaseLane roadLanePrefab;
    private BaseLane riverLanePrefab;
    private BaseLane railroadLanePrefab;

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
    /// 레인 프리팹 로드 (폴백 처리 포함)
    /// </summary>
    private void LoadLanePrefabs()
    {
        //Resources 폴더에서 로드 시도
        grassLanePrefab = Resources.Load<BaseLane>("Prefabs/Lanes/GrassLane");
        roadLanePrefab = Resources.Load<BaseLane>("Prefabs/Lanes/RoadLane");
        riverLanePrefab = Resources.Load<BaseLane>("Prefabs/Lanes/RiverLane");
        railroadLanePrefab = Resources.Load<BaseLane>("Prefabs/Lanes/RailroadLane");

        // 로드 실패 시 로그 출력 (디버깅용)
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
    }

    /// <summary>
    /// 랜덤 레인 타입 결정 (가중치 기반)
    /// </summary>
    private LaneType DecideLaneType()
    {
        int totalWeight = grassWeight + roadWeight + riverWeight + railroadWeight;
        int randomValue = Random.Range(0, totalWeight);

        int cumulative = 0;
        cumulative += grassWeight;
        if (randomValue < cumulative) return LaneType.Grass;

        cumulative += roadWeight;
        if (randomValue < cumulative) return LaneType.Road;

        cumulative += riverWeight;
        if (randomValue < cumulative) return LaneType.River;

        return LaneType.Railroad;
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
            case LaneType.River:
                return laneObj.AddComponent<RiverLane>();
            case LaneType.Railroad:
                return laneObj.AddComponent<RailroadLane>();
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
            case LaneType.River: return riverLanePrefab;
            case LaneType.Railroad: return railroadLanePrefab;
            default: return grassLanePrefab;
        }
    }
}
