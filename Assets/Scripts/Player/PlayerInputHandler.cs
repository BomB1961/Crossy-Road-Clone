using UnityEngine;

/// <summary>
/// 이동 방향 열거형
/// </summary>
public enum MoveDirection
{
    None,
    Forward,  // +Z
    Back,     // -Z
    Right,    // +X
    Left      // -X
}

/// <summary>
/// 플레이어 입력 처리 핸들러 - 터치/키보드/게임패드 입력 감지
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [Header("터치 설정")]
    [SerializeField] private float swipeThreshold = 50f; // px 기준
    [SerializeField] private float tapThreshold = 50f;

    // 터치 데이터
    private Vector2 touchStartPosition;
    private Vector2 touchEndPosition;
    private bool isTouching = false;

    // 이벤트
    public event System.Action<MoveDirection> OnDirectionDecided;

    private void Update()
    {
        // 터치 입력 처리
        ProcessTouchInput();

        // 키보드 입력 (개발/PC 테스트용)
        ProcessKeyboardInput();
    }

    /// <summary>
    /// 터치 입력 처리 (On Finger Up 기준)
    /// </summary>
    private void ProcessTouchInput()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                touchStartPosition = touch.position;
                isTouching = true;
                break;

            case TouchPhase.Ended:
                if (isTouching)
                {
                    touchEndPosition = touch.position;
                    JudgeDirection();
                    isTouching = false;
                }
                break;

            case TouchPhase.Canceled:
                isTouching = false;
                break;
        }
    }

    /// <summary>
    /// 터치 거리/방향 판정
    /// </summary>
    private void JudgeDirection()
    {
        Vector2 delta = touchEndPosition - touchStartPosition;
        float distance = delta.magnitude;

        // Tap: 거리 < 임계값
        if (distance < tapThreshold)
        {
            OnDirectionDecided?.Invoke(MoveDirection.Forward);
            return;
        }

        // Swipe: 거리 ≥ 임계값 - 방향 판정
        float absX = Mathf.Abs(delta.x);
        float absY = Mathf.Abs(delta.y);

        if (absY > absX)
        {
            // 상하 방향
            if (delta.y > 0)
            {
                OnDirectionDecided?.Invoke(MoveDirection.Forward); // Swipe Up
            }
            else
            {
                OnDirectionDecided?.Invoke(MoveDirection.Back);    // Swipe Down
            }
        }
        else
        {
            // 좌우 방향
            if (delta.x > 0)
            {
                OnDirectionDecided?.Invoke(MoveDirection.Right);  // Swipe Right
            }
            else
            {
                OnDirectionDecided?.Invoke(MoveDirection.Left);    // Swipe Left
            }
        }
    }

    /// <summary>
    /// 키보드 입력 (PC/개발용)
    /// </summary>
    private void ProcessKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            OnDirectionDecided?.Invoke(MoveDirection.Forward);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            OnDirectionDecided?.Invoke(MoveDirection.Back);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnDirectionDecided?.Invoke(MoveDirection.Right);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnDirectionDecided?.Invoke(MoveDirection.Left);
        }
    }
}
