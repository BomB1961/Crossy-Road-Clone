using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 테마 구간 열거형 (4개 테마)
/// </summary>
public enum ZoneType
{
    Forest,  // 숲 - GrassLane
    City,    // 도시 - RoadLane
    River,   // 강 - RiverLane
    Space    // 우주 - SpaceLane
}

/// <summary>
/// 레인 프리팹과 장애물 타입 pairing
/// </summary>
[System.Serializable]
public class LanePrefabType
{
    public BaseLane prefab;         // 레인 프리팹
    public ObstacleType obstacleType; // FixedOnly 또는 MovingOnly
}

/// <summary>
/// 레인 생성기 - 4개 테마 기반 레인 생성 (ObstacleType: FixedOnly/MovingOnly)
/// </summary>
public class TerrainGenerator : MonoBehaviour, ITerrainGenerator
{
    [Header("레인 설정")]
    [SerializeField] private int visibleLaneCount = 9;
    [SerializeField] private float laneHeight = 1f;

    [Header("구간 설정")]
    [SerializeField] private int lanesPerZone = 10;  // 구간당 레인 수

    [Header("각 테마별 레인 프리팹 + 장애물 타입 배열")]
    [SerializeField] private LanePrefabType[] forestLaneOptions;   // Forest용
    [SerializeField] private LanePrefabType[] cityLaneOptions;    // City용
    [SerializeField] private LanePrefabType[] riverLaneOptions;    // River용
    [SerializeField] private LanePrefabType[] spaceLaneOptions;   // Space용

    // 구간 관리
    private ZoneType currentZone = ZoneType.Forest;
    private int lanesInCurrentZone = 0;

    // 레인 관리
    private ObjectPool<BaseLane> lanePool;
    private List<BaseLane> activeLanes = new List<BaseLane>();
    private int currentRow = 0;

    public int LaneCount => activeLanes.Count;

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize(int count, float height, ObjectPool<BaseLane> pool)
    {
        visibleLaneCount = count;
        laneHeight = height;
        lanePool = pool;
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

        // 바닥 프리팹 + 장애물 타입 랜덤 선택
        LanePrefabType selected = GetRandomPrefabTypeForZone(currentZone);
        ObstacleType obstacleType = selected != null ? selected.obstacleType : ObstacleType.FixedOnly;
        BaseLane lanePrefab = selected != null ? selected.prefab : null;

        BaseLane lane;

        if (lanePrefab != null)
        {
            lane = lanePool.Rent();
        }
        else
        {
            // 폴백: 동적 생성
            lane = CreateDynamicLane(type);
        }

        // 장애물 타입 파라미터 전달
        var method = lane.GetType().GetMethod("Initialize", new[] { typeof(int), typeof(ObstacleType) });
        if (method != null)
        {
            method.Invoke(lane, new object[] { row, obstacleType });
        }
        else
        {
            lane.Initialize(row);
        }

        activeLanes.Add(lane);

        // 구간 내 레인 카운트 증가
        IncrementLaneCount();

        currentRow = Mathf.Max(currentRow, row);
    }

    /// <summary>
    /// 구간에 맞는 랜덤 바닥+타입 반환 (null 자동 필터링)
    /// </summary>
    private LanePrefabType GetRandomPrefabTypeForZone(ZoneType zone)
    {
        LanePrefabType[] options = GetOptionsForZone(zone);

        // null 및 무효 프리팹 필터링
        List<LanePrefabType> validOptions = new List<LanePrefabType>();
        foreach (var option in options)
        {
            if (option != null && option.prefab != null)
                validOptions.Add(option);
        }

        if (validOptions.Count == 0)
        {
            Debug.LogWarning($"[TerrainGenerator] {zone} 테마의 유효한 레인이 없습니다.");
            return null;
        }

        return validOptions[Random.Range(0, validOptions.Count)];
    }

    /// <summary>
    /// Zone별 LanePrefabType 배열 반환
    /// </summary>
    private LanePrefabType[] GetOptionsForZone(ZoneType zone)
    {
        switch (zone)
        {
            case ZoneType.Forest: return forestLaneOptions;
            case ZoneType.City:   return cityLaneOptions;
            case ZoneType.River:  return riverLaneOptions;
            case ZoneType.Space:  return spaceLaneOptions;
            default:             return forestLaneOptions;
        }
    }

    /// <summary>
    /// 동적 레인 생성 (폴백)
    /// </summary>
    private BaseLane CreateDynamicLane(LaneType type)
    {
        var laneObj = new GameObject(type.ToString() + "Lane");
        laneObj.transform.parent = transform;

        switch (type)
        {
            case LaneType.Grass:  return laneObj.AddComponent<GrassLane>();
            case LaneType.Road:   return laneObj.AddComponent<RoadLane>();
            case LaneType.River:  return laneObj.AddComponent<RiverLane>();
            case LaneType.Space:  return laneObj.AddComponent<SpaceLane>();
            default:             return laneObj.AddComponent<GrassLane>();
        }
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
    /// 구간 기반 레인 타입 결정 (4개 테마 순환)
    /// </summary>
    private LaneType DecideLaneType()
    {
        // 구간당 레인 수 도달 시 구간 전환
        if (lanesInCurrentZone >= lanesPerZone)
        {
            SwitchZone();
        }

        // 현재 구간에 맞는 레인 타입 반환
        return ZoneToLaneType(currentZone);
    }

    /// <summary>
    /// ZoneType → LaneType 변환
    /// </summary>
    private LaneType ZoneToLaneType(ZoneType zone)
    {
        switch (zone)
        {
            case ZoneType.Forest: return LaneType.Grass;
            case ZoneType.City:   return LaneType.Road;
            case ZoneType.River:  return LaneType.River;
            case ZoneType.Space:  return LaneType.Space;
            default:              return LaneType.Grass;
        }
    }

    /// <summary>
    /// 구간 전환 (4개 테마 순환)
    /// </summary>
    private void SwitchZone()
    {
        // Forest → City → River → Space → Forest ...
        switch (currentZone)
        {
            case ZoneType.Forest:
                currentZone = ZoneType.City;
                break;
            case ZoneType.City:
                currentZone = ZoneType.River;
                break;
            case ZoneType.River:
                currentZone = ZoneType.Space;
                break;
            case ZoneType.Space:
                currentZone = ZoneType.Forest;
                break;
        }
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
}
