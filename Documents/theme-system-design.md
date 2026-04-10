# Crossy Road Clone - 테마 시스템 설계 (최종안)

## 작성일: 2026-04-10

---

## 1. 개요

4가지 테마 시스템 기반의 Crossy Road 스타일 엔드리스 러너 게임.

### 테마 종류

| 테마 | 바닥 | 이동 장애물 | 고정 장애물 |
|------|------|-------------|-------------|
| **Forest** | 잔디 | 동물 (VoxBox) | 나무, 바위 |
| **City** | 도로 | 차량 (Voxel Vehicles) | 가로등, 방호벽 |
| **River** | 물 | 배 (Boat) | 없음 |
| **Space** | 화성 흙 | 우주선 | 암석, 통신 안테나 |

---

## 2. 테마별 장애물

### 이동 장애물

| 테마 | 장애물 | 이동 방식 |
|------|--------|----------|
| Forest | VoxBox 동물 | 점프처럼 한 칸 이동 |
| City | 차량 | 좌/우로 일정 속도 |
| River | 배 | 좌/우로 일정 속도 |
| Space | 우주선 | 좌/우로 일정 속도 |

### 고정 장애물

| 테마 | 장애물 | 효과 |
|------|--------|------|
| Forest | 나무, 바위 | 전진 시 장애물로 판정, 제자리 점프 |
| City | 가로등, 방호벽 | 전진 시 장애물로 판정, 제자리 점프 |
| River | 없음 | - |
| Space | 암석, 통신 안테나 | 전진 시 장애물로 판정, 제자리 점프 |

---

## 3. 레인 구조

### 레인 스크립트 (4개)

```
Assets/Scripts/Terrain/
├── GrassLane.cs    // Forest 테마
├── RoadLane.cs     // City 테마
├── RiverLane.cs    // River 테마
└── SpaceLane.cs    // Space 테마
```

### LaneType enum

```csharp
public enum LaneType
{
    Grass,   // 숲 - 동물 이동 장애물
    Road,    // 도시 - 차량 이동 장애물
    River,   // 강 - 배 이동 장애물
    Space    // 우주 - 우주선 이동 장애물
}
```

### ZoneType enum

```csharp
public enum ZoneType
{
    Forest,  // 숲 (GrassLane)
    City,    // 도시 (RoadLane)
    River,   // 강 (RiverLane)
    Space    // 우주 (SpaceLane)
}
```

---

## 4. 고정 장애물 Resources 경로

```
Assets/Resources/Decorations/
├── Forest/      (tree_0.prefab, rock_0.prefab)
├── City/        (lamp_0.prefab, barrier_0.prefab)
├── River/       (비어있음)
└── Space/       (meteor_0.prefab, antenna_0.prefab)
```

---

## 5. 구간 생성 순서

```
Forest(6) → City(6) → River(6) → Space(6) → Forest(6) → ...
```

---

## 6. 구현 파일 목록

| 파일 | 용도 |
|------|------|
| GrassLane.cs | 숲 레인 - 동물 + 나무/바위 |
| RoadLane.cs | 도시 레인 - 차량 + 가로등/방호벽 |
| RiverLane.cs | 강 레인 - 배 (고정 장애물 없음) |
| SpaceLane.cs | 우주 레인 - 우주선 + 암석/안테나 |
| TerrainGenerator.cs | 4개 테마 레인 생성 관리 |
