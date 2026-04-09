using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

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
/// 플레이어 입력 처리 핸들러 - Input System 사용
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

            // 점프 액션 콜백 바인딩
            jumpAction.performed += OnJumpPerformed;
        }
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        lookAction?.Enable();
        jumpAction?.Enable();
    }

    private void OnDisable()
    {
        jumpAction.performed -= OnJumpPerformed;

        moveAction?.Disable();
        lookAction?.Disable();
        jumpAction?.Disable();
    }

    private void Update()
    {
        // 이동 입력 처리
        ProcessMoveInput();
    }

    /// <summary>
    /// 점프 액션 콜백
    /// </summary>
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        OnDirectionDecided?.Invoke(MoveDirection.Forward);
    }

    /// <summary>
    /// 이동 입력 처리 (WASD, 방향키, 조이패드)
    /// </summary>
    private void ProcessMoveInput()
    {
        if (moveAction == null) return;

        Vector2 move = moveAction.ReadValue<Vector2>();

        // Dead Zone
        if (move.magnitude < 0.1f) return;

        // Y축 입력을 Forward/Back으로 변환
        if (move.y > 0.1f)
        {
            OnDirectionDecided?.Invoke(MoveDirection.Forward);
        }
        else if (move.y < -0.1f)
        {
            OnDirectionDecided?.Invoke(MoveDirection.Back);
        }

        // X축 입력을 Left/Right로 변환
        if (move.x > 0.1f)
        {
            OnDirectionDecided?.Invoke(MoveDirection.Right);
        }
        else if (move.x < -0.1f)
        {
            OnDirectionDecided?.Invoke(MoveDirection.Left);
        }
    }
}
