# Crossy Road Clone - 테마 시스템 설계 (최종안)

## 작성일: 2026-04-10

---

## 1. 개요

4가지 테마 시스템 기반의 Crossy Road 스타일 엔드리스 러너 게임.

### 테마 종류

| 테마 | 레인 스크립트 | 바닥 | 고정 장애물 | 이동 장애물 |
|------|-------------|------|-------------|-------------|
| **Forest** | GrassLane | 잔디/흙 | 나무, 바위 | 동물 (VoxBox) |
| **City** | RoadLane | 도로/고속도로 | 가로등, 방호벽 | 차량 (Voxel Vehicles) |
| **River** | RiverLane | 물/습지 | 목재, 나무, 식물, 돌 | 배 (Boat) |
| **Space** | SpaceLane | 화성/달 | 암석, 안테나 | 우주선 |

---

## 2. ObstacleType 시스템

### 타입 종류

| 타입 | 설명 | 고정 장애물 | 이동 장애물 |
|------|------|-------------|-------------|
| **FixedOnly** | 고정만 | 2~4개 (랜덤, 최소 간격 유지) | 0개 |
| **MovingOnly** | 이동만 | 0개 | 랜덤 스폰 간격, 랜덤 속도 |

### 이동 장애물 특성

| 특성 | 설명 |
|------|------|
| **방향** | 좌 → 우만 (절대 우 → 좌 X) |
| **생성** | 화면 왼쪽 밖에서 Spawn |
| **소멸** | 화면 오른쪽 밖으로 나가면 즉시 제거 |
| **속도** | 동일 종류 = 동일 속도 |
| **메모리** | 풀링 불필요, Spawn/Despawn 반복 |

### 메모리 최적화

| 구분 | 방식 | 이유 |
|------|------|------|
| **고정 장애물** | 풀링 고려 | Lane内有재사용 |
| **이동 장애물** | Spawn/Despawn | 화면 밖 즉시 제거로 풀링 불필요 |

---

## 3. LanePrefabType 구조

프리팹과 장애물 타입을 **1:1 pairing**:

```csharp
[System.Serializable]
public class LanePrefabType
{
    public BaseLane prefab;          // 레인 프리팹
    public ObstacleType obstacleType; // FixedOnly 또는 MovingOnly
}
```

### Inspector 설정 예시

```
forestLaneOptions (Size: 2)
  [0] prefab: GrassLane_Green   obstacleType: MovingOnly  → 동물 이동
  [1] prefab: GrassLane_Brown   obstacleType: FixedOnly  → 나무/바위 고정
```

---

## 4. 이동 장애물 Inspector 설정

### CityLane 예시

```
movingObstaclePrefabs (배열: 3개)
  [0] Sedan.prefab     ← 느린 차량
  [1] Taxi.prefab      ← 보통 차량
  [2] Sportscar.prefab ← 빠른 차량

speeds (배열: 3개)
  [0] 2f   ← 느림
  [1] 4f   ← 보통
  [2] 6f   ← 빠름

spawnInterval: 2f  ← 스폰 간격
```

---

## 5. 고정 장애물 Inspector 설정

```
fixedObstaclePrefabs (배열)
  [0] Tree.prefab
  [1] Rock.prefab
  [2] Bush.prefab

minFixedCount: 2   ← 최소 개수
maxFixedCount: 4   ← 최대 개수
minGap: 1.5f       ← 최소 간격 (균형 잡힌 배치)
```

---

## 6. Resources 폴더 구조

```
Assets/Resources/
├── Prefabs/Lanes/              # 레인 프리팹
│   ├── GrassLane_Green.prefab   (MovingOnly)
│   ├── GrassLane_Brown.prefab    (FixedOnly)
│   ├── RoadLane_City.prefab      (MovingOnly)
│   ├── RoadLane_Highway.prefab   (FixedOnly)
│   ├── RiverLane_Water.prefab    (MovingOnly)
│   ├── RiverLane_Swamp.prefab    (FixedOnly)
│   ├── SpaceLane_Mars.prefab     (MovingOnly)
│   └── SpaceLane_Moon.prefab      (FixedOnly)
└── Decorations/               # 고정 장애물
    ├── Forest/                # 나무, 바위
    ├── City/                  # 가로등, 방호벽
    ├── River/                 # 목재, 나무, 식물, 돌
    └── Space/                 # 암석, 안테나
```

---

## 7. 구간 생성 순서

```
Forest(10) → City(10) → River(10) → Space(10) → Forest(10) → ...
```

### 구간 내 레인 예시

```
Forest (10개 레인):
  Lane 1: GrassLane_Green + MovingOnly   (동물 이동)
  Lane 2: GrassLane_Brown + FixedOnly    (나무/바위 고정)
  Lane 3: GrassLane_Green + MovingOnly   (동물 이동)
  Lane 4: GrassLane_Brown + FixedOnly    (나무/바위 고정)
  ... (랜덤 선택)
```

---

## 8. 구현 파일 목록

| 파일 | 용도 |
|------|------|
| GrassLane.cs | 숲 레인 - 동물 이동 OR 나무/바위 고정 |
| RoadLane.cs | 도시 레인 - 차량 이동 OR 가로등/방호벽 고정 |
| RiverLane.cs | 강 레인 - 배 이동 OR 목재/나무/식물/돌 고정 |
| SpaceLane.cs | 우주 레인 - 우주선 이동 OR 암석/안테나 고정 |
| TerrainGenerator.cs | 4개 테마 + LanePrefabType 시스템 |
| ObstacleType.cs | FixedOnly/MovingOnly 열거형 |
| MovingObstacleMover.cs | 이동 장애물 제어 (좌→우, 화면 밖 즉시 제거) |
