# 디버깅 로그

## 2단계 디버깅 완료 - 수정 내역

### 수정 완료 ✅

| # | 파일 | 문제 | 수정 내용 | 상태 |
|---|------|------|----------|------|
| 1 | BaseLane.cs | SetObstaclesActive 미구현 | 기본 구현 추가 (자식 SetActive 토글) | ✅ 수정 |
| 2 | PlayerController.cs | gameManager null 참조 | Bootstrap.Instance null 체크 추가 | ✅ 수정 |
| 3 | PlayerController.cs | 쿼터뷰 방향 주석 | 쿼터뷰 따라 다름注明 추가 | ✅ 주석 |
| 4 | TerrainGenerator.cs | Resources 로드 실패 | 폴백 처리: 동적 생성 fallback | ✅ 수정 |
| 5 | TerrainGenerator.cs | RentLane null 체크 | prefab null 시 구체적 타입 동적 생성 | ✅ 수정 |
| 6 | GrassLane.cs | 장식Prefab 미할당 | Resources.LoadAll 자동 로드 추가 | ✅ 수정 |
| 7 | ObstacleCollision.cs | playerLayer 미사용 | 미사용 필드 제거, 코드 정리 | ✅ 수정 |

### 해결됨 ✅

1. **gameManager null 체크** - Bootstrap.Instance != null 조건 추가
2. **Resources.Load 폴백** - 프리팹 없으면 동적 생성 fallback
3. **SetObstaclesActive** - BaseLane에 기본 구현 제공
4. **GrassLane 장식** - Resources.LoadAll로 자동 로드

---

## 미해결 (3단계에서 해결 예정)

| # | 항목 | 상태 | 비고 |
|---|------|------|------|
| 1 | 쿼터뷰 방향 매핑 | ⚠️ 미테스트 | 카메라 설정 후 실제 테스트 필요 |
| 2 | 차량 풀링 | ❌ 미구현 | 3단계에서 장애물 풀링 구현 |
| 3 | 레인 풀링 | ❌ 미구현 | Bootstrap의 CreateNewLane/Obstacle 수정 필요 |
| 4 | 카메라追徏 제어 | ❌ 미구현 | 3단계에서 CameraController 구현 |
| 5 | 독수리 시스템 | ❌ 미구현 | 3단계에서 EagleController 구현 |
| 6 | UI 연동 | ❌ 미구현 | 3단계에서 HUD, GameOver 구현 |

---

## 새 파일 추가 (2단계)

| 파일 | 용도 |
|------|------|
| RiverLane.cs | 강 레인 (통나무) |
| RailroadLane.cs | 철도 레인 (기차) |
| MoveDirection.cs | 이동 방향 enum (PlayerInputHandler와 공유) |

---

## 테스트 필요 항목

| 항목 | 테스트 방법 |
|------|------------|
| **터치 Tap/Swipe** |实機 테스트 (PC는 키보드로 대체) |
| **쿼터뷰 방향** | 캐릭터 이동 시 방향 관찰 (수정 필요 가능) |
| **그리드 스냅** | 좌표가 소수점 이하 없이 고정되는지 확인 |
| **입력 큐** | 빠르게 연타 시 부드럽게 이동하는지 확인 |
| **레이캐스트** | 나무/바위 앞에서 이동 시 점프만 하는지 확인 |
| **레인 생성** | 랜덤 타입 생성, 장애물 배치 확인 |
| **충돌 감지** | 차량/기차/통나무 충돌 시 Die() 호출 확인 |

---

## 3단계 구현 목표

1. **카메라控制系统** - 플레이어 추적, 데드존
2. **독수리 시스템** - 5초 정지 시 Eagle 등장
3. **UI 연동** - HUD (점수), GameOver 화면
4. **프리팹 생성** - Resources/Prefabs/Lanes/에 레인 프리팹
5. **Bootstrap 수정** - 구체적 레인/장애물 풀링

---

## 참고: SetActive 비용

| 방법 | 비용 | 비고 |
|------|------|------|
| SetActive(true/false) | 높음 | Hierarchy 변경, Awake/OnEnable 호출 |
| transform.position 변경 | 낮음 | 위치만 변경 |

**현재 구조**: 풀링 시 SetActive 사용 중
**향후 최적화**: 숨김 위치(pool position) 패턴 적용 검토
