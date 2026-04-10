using UnityEngine;

/// <summary>
/// 이동 장애물 이동 제어 (메모리 최적화: Spawn/Despawn)
/// </summary>
public class MovingObstacleMover : MonoBehaviour
{
    private float speed;
    private float despawnX;  // 이 위치보다 넘어가면 제거

    public void Initialize(float moveSpeed, float despawnBoundary)
    {
        speed = moveSpeed;
        despawnX = despawnBoundary;
    }

    private void Update()
    {
        // 좌 → 우 이동
        transform.position += Vector3.right * speed * Time.deltaTime;

        // 화면 오른쪽 밖으로 나가면 즉시 제거 (풀링 불필요)
        if (transform.position.x > despawnX)
        {
            Object.Destroy(gameObject);
        }
    }
}