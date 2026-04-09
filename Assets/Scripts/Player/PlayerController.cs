using UnityEngine;

/// <summary>
/// 플레이어 컨트롤러 - 이동, 점프, 그리드 스냅, 입력 큐
/// </summary>
public class PlayerController : MonoBehaviour, IPlayerController
{
    [Header("점프 설정")]
    [SerializeField] private float jumpDuration = 0.2f;  // 점프 시간
    [SerializeField] private float jumpHeight = 0.5f;     // 점프 높이

    [Header("위치 설정")]
    [SerializeField] private LayerMask obstacleLayer;     // 레이캐스트 장애물 레이어

    // 현재 상태
    private int currentRow = 0;        // Z축 위치 (ROW)
    private int currentCol = 0;         // X축 위치 (COL)
    private MoveDirection currentDirection = MoveDirection.Forward;
    private bool isJumping = false;
    private bool isDead = false;
    private MoveDirection queuedInput = MoveDirection.None;

    // 애니메이션
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float jumpProgress = 0f;

    // 컴포넌트
    private PlayerInputHandler inputHandler;
    private IGameManager gameManager;

    // 프로퍼티
    public int CurrentRow => currentRow;
    public int CurrentCol => currentCol;
    public bool IsDead => isDead;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    private void Start()
    {
        // 의존성 해결 (Bootstrap에서 등록된 것 사용)
        if (Bootstrap.Instance != null)
        {
            gameManager = Bootstrap.Instance.Resolve<IGameManager>();
        }

        // 입력 핸들러 이벤트 구독
        if (inputHandler != null)
        {
            inputHandler.OnDirectionDecided += HandleDirectionInput;
        }

        // 초기 위치 스냅
        SnapToGrid();
    }

    private void Update()
    {
        if (isDead)
            return;

        // 점프 애니메이션 처리
        if (isJumping)
        {
            UpdateJumpAnimation();
        }
    }

    /// <summary>
    /// 입력 처리 (입력 큐 시스템)
    /// </summary>
    private void HandleDirectionInput(MoveDirection direction)
    {
        if (isDead)
            return;

        if (isJumping)
        {
            // 점프 중: 입력 저장 (최대 1개)
            queuedInput = direction;
            return;
        }

        // 점프 실행
        TryMove(direction);
    }

    /// <summary>
    /// 이동 시도 (장애물 체크 포함)
    /// </summary>
    private void TryMove(MoveDirection direction)
    {
        // 방향 벡터 계산
        Vector3 targetDelta = GetDirectionVector(direction);
        Vector3 targetPos = transform.position + targetDelta;

        // 장애물 체크 (레이캐스트)
        if (CheckObstacle(targetPos))
        {
            // 장애물 있음: 제자리 점프만 실행
            PlayIdleJumpAnimation();
            return;
        }

        // 이동 실행
        currentDirection = direction;
        StartJump(transform.position, targetPos);
    }

    /// <summary>
    /// 방향 벡터 변환 (화면 → 3D 월드)
    /// </summary>
    private Vector3 GetDirectionVector(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Forward:
                return new Vector3(0, 0, 1);   // +Z (화면 위쪽)
            case MoveDirection.Back:
                return new Vector3(0, 0, -1);  // -Z (화면 아래쪽)
            case MoveDirection.Right:
                return new Vector3(1, 0, 0);   // +X (화면 오른쪽)
            case MoveDirection.Left:
                return new Vector3(-1, 0, 0);  // -X (화면 왼쪽)
            default:
                return Vector3.zero;
        }
    }

    /// <summary>
    /// 장애물 체크 (레이캐스트)
    /// </summary>
    private bool CheckObstacle(Vector3 targetPos)
    {
        RaycastHit hit;
        if (Physics.Raycast(targetPos + Vector3.up * 0.5f, Vector3.down, out hit, 1f, obstacleLayer))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 점프 애니메이션 시작
    /// </summary>
    private void StartJump(Vector3 from, Vector3 to)
    {
        startPosition = from;
        targetPosition = to;
        jumpProgress = 0f;
        isJumping = true;

        // 캐릭터 회전 (이동 방향 바라보기)
        transform.rotation = Quaternion.LookRotation(to - from);
    }

    /// <summary>
    /// 점프 애니메이션 업데이트
    /// </summary>
    private void UpdateJumpAnimation()
    {
        jumpProgress += Time.deltaTime / jumpDuration;

        if (jumpProgress >= 1f)
        {
            // 점프 완료
            jumpProgress = 1f;
            isJumping = false;

            // 목표 위치로 이동 후 그리드 스냅
            transform.position = targetPosition;
            SnapToGrid();

            // 좌표 업데이트
            currentRow = Mathf.RoundToInt(transform.position.z);
            currentCol = Mathf.RoundToInt(transform.position.x);

            // 점수 추가 (전진 시)
            if (currentDirection == MoveDirection.Forward)
            {
                gameManager?.AddScore(1);
            }

            // 큐된 입력 처리
            if (queuedInput != MoveDirection.None)
            {
                MoveDirection queued = queuedInput;
                queuedInput = MoveDirection.None;
                TryMove(queued);
            }
        }
        else
        {
            // 포물선 애니메이션
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, jumpProgress);

            // Y축 = 점프 높이 (포물선)
            float height = Mathf.Sin(jumpProgress * Mathf.PI) * jumpHeight;
            currentPos.y = height;

            transform.position = currentPos;
        }
    }

    /// <summary>
    /// 제자리 점프 애니메이션 (장애물 있어도 실행)
    /// </summary>
    private void PlayIdleJumpAnimation()
    {
        // 간단한 Y축 떨림만 실행
        StartCoroutine(IdleJumpCoroutine());
    }

    private System.Collections.IEnumerator IdleJumpCoroutine()
    {
        float duration = 0.15f;
        float elapsed = 0f;
        Vector3 originalPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float y = Mathf.Sin(t * Mathf.PI) * 0.2f;
            transform.position = originalPos + Vector3.up * y;
            yield return null;
        }

        transform.position = originalPos;
    }

    /// <summary>
    /// 그리드 스냅 (정수 좌표로 고정)
    /// </summary>
    private void SnapToGrid()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.RoundToInt(pos.x);
        pos.y = 0f;
        pos.z = Mathf.RoundToInt(pos.z);
        transform.position = pos;
    }

    /// <summary>
    /// 플레이어 위치 초기화
    /// </summary>
    public void ResetPosition()
    {
        currentRow = 0;
        currentCol = 0;
        isJumping = false;
        isDead = false;
        queuedInput = MoveDirection.None;

        SnapToGrid();
        transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// 게임 오버 처리
    /// </summary>
    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isJumping = false;
        queuedInput = MoveDirection.None;
        gameManager?.GameOver();
    }

    private void OnDestroy()
    {
        if (inputHandler != null)
        {
            inputHandler.OnDirectionDecided -= HandleDirectionInput;
        }
    }

    #region IPlayerController 구현

    /// <summary>
    /// 지정한 행으로 이동 (앞/뒤)
    /// </summary>
    public void MoveRow(int delta)
    {
        if (delta > 0)
            TryMove(MoveDirection.Forward);
        else if (delta < 0)
            TryMove(MoveDirection.Back);
    }

    /// <summary>
    /// 지정한 열로 이동 (좌/우)
    /// </summary>
    public void MoveCol(int delta)
    {
        if (delta > 0)
            TryMove(MoveDirection.Right);
        else if (delta < 0)
            TryMove(MoveDirection.Left);
    }

    /// <summary>
    /// 점프 애니메이션 트리거
    /// </summary>
    public void Jump()
    {
        TryMove(MoveDirection.Forward);
    }

    #endregion
}
