using UnityEngine;

/// <summary>
/// 레인 베이스 클래스 - 모든 레인의 공통 기능 제공
/// </summary>
public abstract class BaseLane : MonoBehaviour
{
    [Header("레인 기본 설정")]
    [SerializeField] protected LaneType laneType = LaneType.Grass;
    [SerializeField] protected int rowIndex = 0;
    [SerializeField] protected float moveSpeed = 0f;

    public LaneType LaneType => laneType;
    public int RowIndex => rowIndex;
    public bool HasObstacles => laneType != LaneType.Grass;

    /// <summary>
    /// 레인 초기화
    /// </summary>
    public virtual void Initialize(int row)
    {
        rowIndex = row;
        transform.position = new Vector3(0, 0, row);
    }

    /// <summary>
    /// 레인 타입별 장애물 생성 (자식 클래스에서 구현)
    /// </summary>
    public abstract void GenerateObstacles();

    /// <summary>
    /// 풀로 반환
    /// </summary>
    public abstract void ReleaseToPool();

    /// <summary>
    /// 장애물 활성화/비활성화 (기본 구현)
    /// </summary>
    public virtual void SetObstaclesActive(bool active)
    {
        // 기본: 자식 오브젝트 전체 토글
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(active);
        }
    }
}
