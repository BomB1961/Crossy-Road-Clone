using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 이동 방향 열거형
/// </summary>
[System.Serializable]
public enum MoveDirection
{
    None,
    Forward,
    Back,
    Right,
    Left
}

/// <summary>
/// 플레이어 입력 처리 핸들러 - Input System 콜백 방식
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [Header("입력 설정")]
    [SerializeField] private InputActionAsset inputActions;

    // 입력 액션
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    // 이벤트
    public event System.Action<MoveDirection> OnDirectionDecided;

    private void Awake()
    {
        // Input Action Asset에서 Player 액션맵 가져오기
        var playerActionMap = inputActions?.FindActionMap("Player");
        if (playerActionMap != null)
        {
            moveAction = playerActionMap.FindAction("Move");
            lookAction = playerActionMap.FindAction("Look");
            jumpAction = playerActionMap.FindAction("Jump");
        }
    }

    private void OnEnable()
    {
        // 콜백 등록
        moveAction?.Enable();
        lookAction?.Enable();
        jumpAction?.Enable();

        // moveAction: 키를 누른瞬间만 입력 발생 (started 사용)
        moveAction.started += OnMoveStarted;

        // jumpAction: 키를 누른瞬間만 입력 발생
        jumpAction.started += OnJumpStarted;
    }

    private void OnDisable()
    {
        // 콜백 해제
        moveAction.started -= OnMoveStarted;
        jumpAction.started -= OnJumpStarted;

        moveAction?.Disable();
        lookAction?.Disable();
        jumpAction?.Disable();
    }

    /// <summary>
    /// 이동 입력 시작 시 호출 (키를 누른瞬間)
    /// </summary>
    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        Vector2 move = context.ReadValue<Vector2>();

        // Dead Zone
        if (move.magnitude < 0.1f) return;

        // 방향 판정 (한 번만 호출됨)
        if (move.y > 0.1f)
        {
            OnDirectionDecided?.Invoke(MoveDirection.Forward);
        }
        else if (move.y < -0.1f)
        {
            OnDirectionDecided?.Invoke(MoveDirection.Back);
        }
        else if (move.x > 0.1f)
        {
            OnDirectionDecided?.Invoke(MoveDirection.Right);
        }
        else if (move.x < -0.1f)
        {
            OnDirectionDecided?.Invoke(MoveDirection.Left);
        }
    }

    /// <summary>
    /// 점프 입력 시작 시 호출 (키를 누른瞬間)
    /// </summary>
    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        OnDirectionDecided?.Invoke(MoveDirection.Forward);
    }
}
