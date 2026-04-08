using UnityEngine;

/// <summary>
/// 장애물 충돌 감지 - 트리거 기반 플레이어死亡 감지
/// </summary>
/// <remarks>
/// 차량, 기차, 통나무 등 장애물 오브젝트에 attachして 사용
/// Collider의 Is Trigger 옵션必須
/// </remarks>
public class ObstacleCollision : MonoBehaviour
{
    /// <summary>
    /// 충돌 감지 (트리거)
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어와 충돌 시
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.IsDead)
            {
                player.Die();
            }
        }
    }
}
