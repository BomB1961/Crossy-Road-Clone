using UnityEngine;
using Cinemachine;

/// <summary>
/// 카메라 후진 추적 제한 - 플레이어가 뒤로 가면 카메라가 따라가지 않음
/// </summary>
public class CameraFollowConstraint : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private float maxForwardZ = 0f;    // 플레이어 최대 전진 Z 위치
    [SerializeField] private float backOffsetMin = 10f;  // 카메라와 플레이어 사이의 Z 간격

    private CinemachineVirtualCamera virtualCamera;

    private void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    private void LateUpdate()
    {
        if (virtualCamera == null || virtualCamera.Follow == null) return;

        Transform player = virtualCamera.Follow;

        // 1. 플레이어의 최대 전진 거리 갱신
        if (player.position.z > maxForwardZ)
        {
            maxForwardZ = player.position.z;
        }

        // 2. 카메라가 뒤로 못 가게 월드 좌표 강제 고정
        // 카메라의 목표 Z 위치는 (최대 전진 거리 - 오프셋) 보다 작아질 수 없음
        float minAllowedCameraZ = maxForwardZ - backOffsetMin;

        if (transform.position.z < minAllowedCameraZ)
        {
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                minAllowedCameraZ
            );
        }
    }
}
