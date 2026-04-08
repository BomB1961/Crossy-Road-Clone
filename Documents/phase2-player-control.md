# 2단계: 플레이어 조종 구현 완료

## 수행 내용

### 생성된 스크립트 (6개)

| 스크립트 | 책임 |
|---------|------|
| **PlayerInputHandler.cs** | 터치/키보드 입력 감지, Tap/Swipe 판정 |
| **PlayerController.cs** | 이동, 점프, 그리드 스냅, 입력 큐, 회전 |
| **TerrainGenerator.cs** | 레인 생성/관리, 가중치 기반 랜덤 타입 결정 |
| **GrassLane.cs** | 풀밭 레인, 장식Prefab 배치 |
| **RoadLane.cs** | 도로 레인, 차량 생성 |
| **ObstacleCollision.cs** | 장애물-플레이어 충돌 감지 |

---

## 핵심 구현 상세

### PlayerInputHandler - 터치 감지 로직

```csharp
// On Finger Up 기준 판정
TouchPhase.Ended → JudgeDirection() 호출
  ├── 거리 < 50px → Tap → Forward
  ├── 거리 ≥ 50px + 상하 swipes → Forward/Back
  └── 거리 ≥ 50px + 좌우 swipes → Left/Right
```

**키보드 지원**: W/A/S/D + 화살표키 (PC 테스트용)

---

### PlayerController - 이동 시스템

**입력 큐 (Input Queueing)**:
```csharp
if (isJumping)
{
    queuedInput = direction; // 최대 1개만 저장
}
else
{
    TryMove(direction); // 즉시 실행
}
```

**그리드 스냅**:
```csharp
// 이동 완료 후 정수 좌표로 고정
pos.x = Mathf.RoundToInt(pos.x); // 예: 1.02 → 1
pos.z = Mathf.RoundToInt(pos.z);
```

**레이캐스트 장애물 체크**:
```csharp
if (Physics.Raycast(targetPos + Vector3.up * 0.5f, Vector3.down, out hit, 1f, obstacleLayer))
{
    PlayIdleJumpAnimation(); // 제자리 점프만
    return;
}
```

---

### TerrainGenerator - 레인 생성

**가중치 기반 랜덤 타입 결정**:
| 타입 | 가중치 | 비고 |
|------|--------|------|
| Grass | 40 | 안전区域 |
| Road | 30 | 차량 장애물 |
| River | 15 | 통나무 필요 |
| Railroad | 15 | 기차 통과 |

---

## 디버깅 필요 항목 (3단계에서 해결)

| # | 문제 | 상태 |
|---|------|------|
| 1 | 쿼터뷰 방향 매핑 | ❌ 미테스트 |
| 2 | gameManager null 체크 | ❌ 미구현 |
| 3 | Resources.Load 실패 | ⚠️ 폴백 필요 |
| 4 | SetObstaclesActive 미구현 | ❌ 미구현 |
| 5 | 차량 풀링 미적용 | ⚠️ 3단계에서 |
| 6 | 장식Prefab 미할당 | ⚠️ Unity에서 할당 |

---

## 설계 결정사항

### 채택한 방식
- **입력 큐**: 최대 1개, 연타 대응
- **그리드 스냅**: 정수 좌표 고정
- **레이캐스트**: 상방 Raycast → 하방 히트
- **점프 높이**: 0.5단위 (보통 점프)
- **점프 시간**: 0.2초

### 추후 최적화 필요
- SetActive → 숨김 위치 이동 패턴
- 장애물 풀링
- 쿼터뷰 방향 실제 테스트 후 확정

---

## 다음 단계: 3단계

1. 장애물 시스템 (차량, 기차, 통나무)
2. 카메라追徏控制系统
3. 독수리 시스템 (데드존)
4. UI 연동 (HUD, GameOver)
