using UnityEngine;
using DG.Tweening;

/// <summary>
/// 플레이어 컨트롤러 - 그리드 기반 이동, DOTween 애니메이션
/// </summary>
public class PlayerController : MonoBehaviour, IPlayerController
{
    [Header("DOTween 이동 설정")]
    [SerializeField] private float moveDuration = 0.15f;     // 이동 시간
    [SerializeField] private float jumpDuration = 0.2f;       // 점프 시간
    [SerializeField] private float jumpHeight = 0.5f;         // 점프 높이
    [SerializeField] private float lookAtDuration = 0.1f;    // 회전 시간

    [Header("입력 설정")]
    [SerializeField] private float inputCooldown = 0.1f;     // 입력 쿨다운 (연속 입력 방지)

    [Header("스쿼시 앤 스트레치 설정")]
    [SerializeField] private Vector3 jumpStretchScale = new Vector3(0.8f, 1.2f, 0.8f);    // 상승 중拉伸
    [SerializeField] private Vector3 landSquashScale = new Vector3(1.2f, 0.8f, 1.2f);    // 착지 시 납작
    [SerializeField] private float squashDuration = 0.08f;   // 스쿼시 애니메이션 시간

    // 그리드 좌표
    private int currentRow = 0;    // Z축 위치 (전후)
    private int currentCol = 0;    // X축 위치 (좌우)

    // 상태
    private MoveDirection currentDirection = MoveDirection.Forward;
    private bool isMoving = false;
    private bool isJumping = false;
    private bool isDead = false;
    private MoveDirection queuedInput = MoveDirection.None;
    private float lastInputTime = -999f;

    // Tween 참조 (이동/점프 도중 취소용)
    private Tween currentMoveTween;
    private Tween currentJumpTween;
    private Tween currentScaleTween;

    // 컴포넌트
    private PlayerInputHandler inputHandler;
    private IGameManager gameManager;

    // 프로퍼티
    public int CurrentRow => currentRow;
    public int CurrentCol => currentCol;
    public bool IsDead => isDead;

    // 애니메이션 이징 (통통 튀는 느낌)
    private const Ease MOVE_EASE = Ease.OutQuad;
    private const Ease JUMP_RISE_EASE = Ease.OutCubic;   // 상승: 빠르고 힘차게
    private const Ease JUMP_FALL_EASE = Ease.InCubic;    // 하강: 중력감
    private const Ease SQUASH_EASE = Ease.OutElastic;     // 스쿼시: 탄성있게 복원

    // 스케일 기본값
    private Vector3 originalScale = Vector3.one;

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

        // 초기 스케일 저장
        originalScale = transform.localScale;

        // 초기 위치 스냅
        SnapToGrid();
    }

    private void Update()
    {
        // 죽었을 때 처리 안함
        if (isDead)
            return;
    }

    /// <summary>
    /// 방향 입력 처리 (입력 큐 시스템)
    /// </summary>
    private void HandleDirectionInput(MoveDirection direction)
    {
        if (isDead)
            return;

        // 입력 쿨다운 체크
        if (Time.time - lastInputTime < inputCooldown)
            return;

        lastInputTime = Time.time;

        // 이동 중: 입력 저장 (최대 1개)
        if (isMoving || isJumping)
        {
            queuedInput = direction;
            return;
        }

        // 이동 실행
        TryMove(direction);
    }

    /// <summary>
    /// 이동 시도
    /// </summary>
    private void TryMove(MoveDirection direction)
    {
        // 목표 좌표 계산
        Vector3 targetPos = CalculateTargetPosition(direction);

        // 장애물 체크 (레이캐스트) - 이동 전 체크
        if (CheckObstacle(targetPos))
        {
            // 장애물 있음: 제자리 점프만 실행
            PlayIdleJumpAnimation();
            return;
        }

        // 방향 저장
        currentDirection = direction;

        // 좌표 업데이트
        UpdateGridPosition(direction);

        // DOTween 이동 + 점프 실행
        StartMoveSequence(targetPos);
    }

    /// <summary>
    /// 방향에 따른 목표 위치 계산
    /// </summary>
    private Vector3 CalculateTargetPosition(MoveDirection direction)
    {
        Vector3 delta = GetDirectionVector(direction);
        return transform.position + delta;
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
    /// 그리드 좌표 업데이트
    /// </summary>
    private void UpdateGridPosition(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Forward:
                currentRow++;
                break;
            case MoveDirection.Back:
                currentRow--;
                break;
            case MoveDirection.Right:
                currentCol++;
                break;
            case MoveDirection.Left:
                currentCol--;
                break;
        }
    }

    /// <summary>
    /// 장애물 체크 (레이캐스트)
    /// </summary>
    private bool CheckObstacle(Vector3 targetPos)
    {
        // 레이어 마스크가 설정되어 있지 않으면 체크 생략
        // TODO: 레인에서 장애물 정보 조회로 변경 (Phase 4)
        return false;
    }

    /// <summary>
    /// DOTween 이동 시퀀스 (이동 + 점프 + 스쿼시 앤 스트레치)
    /// </summary>
    private void StartMoveSequence(Vector3 targetPos)
    {
        // 이전 애니메이션 강제 중지 및 스케일 초기화
        KillAllTweens();
        transform.localScale = originalScale;

        isMoving = true;

        // 회전: 목표 위치를 바라보기
        transform.DOLookAt(targetPos, lookAtDuration)
            .SetEase(Ease.OutQuad);

        float halfJumpDuration = jumpDuration * 0.5f;

        // 상승 중 스쿼시 앤 스트레치 시작 (길쭉해짐)
        currentScaleTween = transform.DOScale(jumpStretchScale, halfJumpDuration * 0.6f)
            .SetEase(JUMP_RISE_EASE);

        // Y축 점프 상승 (빠르고 힘차게)
        currentJumpTween = transform.DOMoveY(jumpHeight, halfJumpDuration)
            .SetEase(JUMP_RISE_EASE)
            .OnComplete(() =>
            {
                // 하강 시작: 원래 스케일로 복원 후 납작해짐
                transform.DOScale(landSquashScale, halfJumpDuration * 0.4f)
                    .SetEase(Ease.OutQuad);

                // Y축 하강 (중력감)
                transform.DOMoveY(0f, halfJumpDuration)
                    .SetEase(JUMP_FALL_EASE)
                    .OnComplete(() =>
                    {
                        // 착지: 스쿼시 효과 (탄성있게 복원)
                        PlayLandSquashAnimation();
                    });
            });

        // XZ 평면 이동 (이동 완료 시 하강 시작)
        currentMoveTween = transform.DOMoveX(targetPos.x, moveDuration)
            .SetEase(MOVE_EASE);
        transform.DOMoveZ(targetPos.z, moveDuration)
            .SetEase(MOVE_EASE);
    }

    /// <summary>
    /// 착지 시 스쿼시 애니메이션 (탄성있게 복원)
    /// </summary>
    private void PlayLandSquashAnimation()
    {
        // 납작한 상태에서 원래 크기로 탄성있게 복원
        currentScaleTween = transform.DOScale(originalScale, squashDuration)
            .SetEase(SQUASH_EASE)
            .OnComplete(() =>
            {
                OnMoveComplete();
            });
    }

    /// <summary>
    /// 이동 완료 시 호출
    /// </summary>
    private void OnMoveComplete()
    {
        isMoving = false;

        // 스케일 보장
        transform.localScale = originalScale;

        // 그리드 스냅 (부동소수점 오차 보정)
        SnapToGrid();

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

    /// <summary>
    /// 제자리 점프 애니메이션 (장애물 있어도 실행)
    /// </summary>
    private void PlayIdleJumpAnimation()
    {
        // 이전 애니메이션 강제 중지 및 스케일 초기화
        KillAllTweens();
        transform.localScale = originalScale;

        isJumping = true;

        float halfDuration = jumpDuration * 0.3f;

        // 상승: 길쭉해짐
        currentScaleTween = transform.DOScale(jumpStretchScale * 0.5f, halfDuration)
            .SetEase(JUMP_RISE_EASE);

        // Y축 떨림 애니메이션
        currentJumpTween = transform.DOMoveY(jumpHeight * 0.3f, halfDuration)
            .SetEase(JUMP_RISE_EASE)
            .OnComplete(() =>
            {
                // 하강: 원래 크기로
                transform.DOScale(originalScale, halfDuration)
                    .SetEase(JUMP_FALL_EASE);

                transform.DOMoveY(0f, halfDuration)
                    .SetEase(JUMP_FALL_EASE)
                    .OnComplete(() =>
                    {
                        // 착지 스쿼시
                        PlayIdleLandSquash();
                    });
            });
    }

    /// <summary>
    /// 제자리 착지 스쿼시
    /// </summary>
    private void PlayIdleLandSquash()
    {
        currentScaleTween = transform.DOScale(landSquashScale, squashDuration * 0.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                currentScaleTween = transform.DOScale(originalScale, squashDuration)
                    .SetEase(SQUASH_EASE)
                    .OnComplete(() =>
                    {
                        isJumping = false;
                        SnapToGrid();

                        // 큐된 입력 처리
                        if (queuedInput != MoveDirection.None)
                        {
                            MoveDirection queued = queuedInput;
                            queuedInput = MoveDirection.None;
                            TryMove(queued);
                        }
                    });
            });
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
        // Tween 중지
        KillAllTweens();

        currentRow = 0;
        currentCol = 0;
        isMoving = false;
        isJumping = false;
        isDead = false;
        queuedInput = MoveDirection.None;
        lastInputTime = -999f;

        transform.localScale = originalScale;
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
        isMoving = false;
        isJumping = false;
        queuedInput = MoveDirection.None;

        // Tween 중지
        KillAllTweens();

        gameManager?.GameOver();
    }

    /// <summary>
    /// 모든 Tween 중지 및 정리
    /// </summary>
    private void KillAllTweens()
    {
        currentMoveTween?.Kill();
        currentJumpTween?.Kill();
        currentScaleTween?.Kill();
        transform.DOKill();

        // 스케일 복원
        transform.localScale = originalScale;
    }

    private void OnDestroy()
    {
        if (inputHandler != null)
        {
            inputHandler.OnDirectionDecided -= HandleDirectionInput;
        }
        KillAllTweens();
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

    #endregion
}
