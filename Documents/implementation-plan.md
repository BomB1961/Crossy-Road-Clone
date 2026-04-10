# 구현计划: Crossy Road Clone

## 1. 개요

Unity 6.4环境下, Assets Store 복셀 에셋을 활용한 Crossy Road 스타일 엔드리스 러너 게임.

## 2. 설계 원칙

### SOLID 적용
| 원칙 | 적용 방법 |
|------|-----------|
| **S** (단일 책임) | 입력/이동/충돌을 별도 클래스로 분리 |
| **O** (개방-폐쇄) | 장애물은 BaseObstacle 상속, 새 장애물 추가 용이 |
| **L** (리스코프 치환) | 모든 레인/장애물이基底클래스로 교체 가능 |
| **I** (인터페이스 분리) | IPlayerController, ITerrainGenerator 등 인터페이스 기반 |
| **D** (의존성 역전) | Bootstrap에서 인터페이스에 의존성 주입 |

### DI (의존성 주입)
- Bootstrap.cs가 모든 의존성을 관리
- 직접 참조 대신 인터페이스 사용
- 테스트 및 확장 용이

### 오브젝트 풀링
- 레인: 최대 20개 재사용
- 장애물: 풀로 관리하여 가비지 컬렉션 최소화

## 3. 폴더 구조

```
Assets/Scripts/
├── Bootstrap/          # DI 컨테이너, 씬 초기화
├── Core/
│   ├── Interfaces/    # IPlayerController, ITerrainGenerator, IObstacle 등
│   └── BaseClasses/  # BaseLane, BaseObstacle 등
├── Player/            # 플레이어 이동, 입력 처리
├── Terrain/          # 레인 생성, 관리
├── Obstacles/        # 차량
├── Pool/             # 오브젝트 풀링 관리
├── UI/               # HUD, GameOver
└── Game/             # 게임 매니저, 점수
```

## 4. 구현 순서

| 순서 | 단계 | 설명 |
|------|------|------|
| 1 | Bootstrap + 인터페이스 | DI 설정, 인터페이스 정의 |
| 2 | 플레이어 조종 | 이동/점프/충돌 감지 |
| 3 | 레인 생성 | 풀밭/도로 레인 생성 및 관리 |
| 4 | 장애물 시스템 | 차량, 배, 우주선 |
| 5 | 게임 로직 | 점수, 독수리, 게임 오버 |
| 6 | UI 연동 | HUD, 재시작 |

## 5. 핵심 메커니즘

### 레인 시스템
- 화면에 9개 레인 표시 (고정)
- 플레이어 전진 시 새로운 레인 생성,古いレーン削除
- 레인 타입: Grass(풀밭), Road(도로)

### 장애물 이동
- 각 레인 타입별 장애물 풀 관리
-Road: 차량 좌/우 이동

### 독수리 시스템
- 플레이어 5초 이상 정지 시 Eagle 등장
- 일정 시간 내 이동 않으면 게임 오버

### 점수 시스템
- 전진한 칸 수 = 점수
- 최고 9,999점

## 6. 폴더별 책임

| 폴더 | 책임 |
|------|------|
| Bootstrap | DI 컨테이너, 씬 초기화 |
| Core/Interfaces | 클래스 간 계약 정의 |
| Core/BaseClasses | 공통基底クラス |
| Player | 플레이어 이동, 입력 |
| Terrain | 레인 생성/관리 |
| Obstacles | 장애물 종류별 행동 |
| Pool | 오브젝트 풀링 |
| UI | UI 표시 |
| Game | 게임 상태, 점수 |

## 7. 사용 에셋

| 게임 요소 | 활용 에셋 |
|----------|----------|
| 도로/차량 | Assets/Voxel Vehicles |
| 나무/바위 | Assets/Decorations |
| 건물 배경 | Assets/CityVoxelPack |
| 물/강 | Shader로 표현 |
