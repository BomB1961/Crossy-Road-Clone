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
/// 플레이어 입력 처리 핸들러
/// - PC/게임패드: Input System 콜백 방식
/// - 모바일: 터치 스와이프 방식
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [Header("입력 설정")]
    [SerializeField] private InputActionAsset inputActions;

    // Input System 액션 (PC/게임패드용)
    private InputAction moveAction;
    private InputAction lookAction;

    // 모바일 스와이프 설정 (PC에서는 사용 안함)
    #if UNITY_MOBILE
    [Header("모바일 스와이프 설정")]
    [SerializeField] private float swipeThreshold = 50f;    // 스와이프 판정 최소 거리(픽셀)
    [SerializeField] private float tapTimeThreshold = 0.2f;  // 탭으로 판정할 최대 시간

    // 터치 상태 관리
    private Vector2 touchStartPosition;
    private float touchStartTime;
    private bool isTouching = false;
    #endif

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
        }
    }

    private void OnEnable()
    {
        // Input System 콜백 등록 (PC/게임패드용)
        moveAction?.Enable();
        lookAction?.Enable();

        moveAction.started += OnMoveStarted;
    }

    private void OnDisable()
    {
        // Input System 콜백 해제
        moveAction.started -= OnMoveStarted;

        moveAction?.Disable();
        lookAction?.Disable();
    }

    private void Update()
    {
        // 모바일 터치 입력 처리
        HandleTouchInput();
    }

    #region PC/게임패드 입력 (Input System)

    /// <summary>
    /// 이동 입력 시작 시 호출 (키를 누른 순간)
    /// </summary>
    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        Vector2 move = context.ReadValue<Vector2>();

        // Dead Zone
        if (move.magnitude < 0.1f) return;

        // 방향 판정
        DetermineDirection(move);
    }

    #endregion

    #region 모바일 터치 입력 (스와이프)

    /// <summary>
    /// 터치 입력 처리 (모바일 전용)
    /// </summary>
    private void HandleTouchInput()
    {
        #if !UNITY_MOBILE
        // PC/에디터: Input System 사용 (위 콜백에서 처리)
        return;
        #else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPosition = touch.position;
                    touchStartTime = Time.time;
                    isTouching = true;
                    break;

                case TouchPhase.Ended:
                    if (isTouching)
                    {
                        Vector2 touchEndPosition = touch.position;
                        Vector2 delta = touchEndPosition - touchStartPosition;
                        float touchDuration = Time.time - touchStartTime;

                        // 짧게 터치: 전진 (탭)
                        if (delta.magnitude < swipeThreshold || touchDuration < tapTimeThreshold)
                        {
                            OnDirectionDecided?.Invoke(MoveDirection.Forward);
                        }
                        // 길게 스와이프: 방향 판정
                        else
                        {
                            DetermineSwipeDirection(delta);
                        }
                    }
                    isTouching = false;
                    break;

                case TouchPhase.Canceled:
                    isTouching = false;
                    break;
            }
        }
        #endif
    }

    /// <summary>
    /// 스와이프 방향 판정 (모바일 전용)
    /// </summary>
    private void DetermineSwipeDirection(Vector2 delta)
    {
        // 수평 vs 수직 비교
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // 좌우 스와이프
            OnDirectionDecided?.Invoke(delta.x > 0 ? MoveDirection.Right : MoveDirection.Left);
        }
        else
        {
            // 상하 스와이프
            OnDirectionDecided?.Invoke(delta.y > 0 ? MoveDirection.Forward : MoveDirection.Back);
        }
    }

    #endregion

    /// <summary>
    /// Vector2에서 방향 판정 (PC/게임패드용)
    /// </summary>
    private void DetermineDirection(Vector2 move)
    {
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
}
