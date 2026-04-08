using UnityEngine;

/// <summary>
/// 장애물 베이스 클래스 - 모든 장애물의 공통 기능 제공
/// </summary>
public abstract class BaseObstacle : MonoBehaviour, IObstacle
{
    [Header("장애물 기본 설정")]
    [SerializeField] protected int row = 0;
    [SerializeField] protected int col = 0;
    [SerializeField] protected float speed = 2f;
    [SerializeField] protected int direction = 1; // 1 = 오른쪽, -1 = 왼쪽

    protected bool isActive = false;
    protected Vector3 startPosition;

    public bool IsActive => isActive;
    public int Row => row;
    public int Col => col;

    public virtual void Initialize(int row, int col, float speed)
    {
        this.row = row;
        this.col = col;
        this.speed = speed;
        isActive = false;
        startPosition = new Vector3(col, 0, row);
        transform.position = startPosition;
    }

    public virtual void Activate()
    {
        isActive = true;
        gameObject.SetActive(true);
    }

    public virtual void Deactivate()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 이동 로직 (자식 클래스에서 구현)
    /// </summary>
    public abstract void Move();
}
