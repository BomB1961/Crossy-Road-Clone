# Crossy Road Clone - 테마 시스템 설계 (최종안)

## 작성일: 2026-04-08

---

## 1. 개요

두 가지 테마(Theme) 시스템 기반의 Crossy Road 스타일 엔드리스 러너 게임.

### 테마 종류

| 테마 | 바닥 에셋 | 이동 장애물 | 고정 장애물 |
|------|-----------|-------------|-------------|
| **City** | City Voxel Pack 도로 | Voxel Vehicles 자동차 | - |
| **Forest** | Voxel Exterior 잔디/흙 | VoxBox 동물 (점프 이동) | 나무, 바위 |

---

## 2. 에셋 매핑

### City 테마
| 구분 | 에셋 | 출처 |
|------|------|------|
| 바닥 | 도로 타일 | City Voxel Pack |
| 차량 | Sedan, Truck, Taxi, Bus | Voxel Vehicles |

### Forest 테마
| 구분 | 에셋 | 출처 |
|------|------|------|
| 바닥 | 잔디/흙 타일 | Voxel Exterior Decor |
| 동물 | Chicken, Sheep, Pig 등 | VoxBox |
| 나무/바위 | Trees, Rocks | Voxel Exterior Decor |

---

## 3. 레인 구조 설계

### ThemeBasedLane (베이스 레인)

```csharp
public enum Theme { City, Forest }

public abstract class ThemeBasedLane : BaseLane
{
    public abstract Theme LaneTheme { get; }
    public abstract GameObject[] ObstaclePrefabs { get; }
    public abstract GameObject[] StaticObstaclePrefabs { get; }
}
```

### CityLane
- **Theme**: City
- **ObstaclePrefabs**: Voxel Vehicles (Sedan, Truck, Taxi, Bus)
- **이동 방식**: 일정 속도 좌우 이동
- **방향**: 랜덤 (Lane 생성 시 결정)

### ForestLane
- **Theme**: Forest
- **ObstaclePrefabs**: VoxBox 동물 (점프 이동)
- **StaticObstaclePrefabs**: 나무, 바위 (고정 배치)
- **이동 방식**: 캐릭터처럼 한 칸 점프 이동
- **방향**: 랜덤 (Lane 생성 시 결정)

---

## 4. 장애물 이동 방식

### City - 차량 (Car)
```
일정 속도 = 3~5 units/sec
방향: L→R 또는 R→L (랜덤)
화면 밖으로 나가면 Pool로 반환
```

### Forest - 동물 (Animal)
```
이동 방식: 한 칸 점프 (캐릭터와 유사)
점프 높이: 0.3~0.5 unit
점프 간격: 0.5~1.0 sec
애니메이션: VoxBox 캐릭터 내장 애니메이션 활용
방향: L→R 또는 R→L (랜덤)
```

### Forest - 고정 장애물 (Static)
```
움직이지 않음
랜덤 위치에 고정 배치
레이캐스트 감지 가능 (Obstacle Layer)
```

---

## 5. Object Pooling 설계

### 풀 구조
```csharp
// Bootstrap에서 관리
ObjectPool<VehicleController> cityPool;  // 차량 풀
ObjectPool<AnimalController> forestPool; // 동물 풀
ObjectPool<Tree/Rock> staticPool;        // 고정 장애물 풀
```

### 차량/동물生命周期
1. Lane 생성 시 Pool에서 Rent
2. 이동 → 화면 밖으로 나가면 Pool로 Return
3. 재스폰 시 다시 Rent

---

## 6. 테마 생성 규칙

### Spawn Rules
```csharp
Direction: 랜덤 (Lane 생성마다 L→R 또는 R→L)
Density: 플레이어 점수 기반 (점수 ↑ → 장애물 밀도 ↑)
Theme: 번갈아 생성 또는 랜덤
```

### 난이도 스케일링
| 점수 구간 | 차량/동물 간격 | 속도 |
|-----------|---------------|------|
| 0~100 | 넓음 (건널 틈 충분) | 느림 |
| 100~500 | 보통 | 보통 |
| 500~1000 | 좁음 (빠른 판단 필요) | 빠름 |
| 1000+ | 매우 좁음 | 매우 빠름 |

---

## 7. 레인 Prefab 구조

```
Assets/Resources/Prefabs/Lanes/
├── City/
│   └── CityLane.prefab
├── Forest/
│   └── ForestLane.prefab
```

### CityLane 내부 구조
```
CityLane
├── Ground (도로 바닥)
└── VehicleSpawnPoints[] (차량 스폰 위치)
```

### ForestLane 내부 구조
```
ForestLane
├── Ground (잔디/흙 바닥)
├── StaticObstacles[] (나무, 바위)
└── AnimalSpawnPoints[] (동물 스폰 위치)
```

---

## 8. ObstacleController 공통 인터페이스

```csharp
public interface IMovableObstacle
{
    void Initialize(Vector3 position, int direction);
    void Move();
    void ReturnToPool();
}
```

---

## 9. 구현 파일 목록

| 파일 | 용도 |
|------|------|
| `Theme.cs` | Theme enum |
| `ThemeBasedLane.cs` | 베이스 레인 (추상 클래스) |
| `CityLane.cs` | 도시 레인 |
| `ForestLane.cs` | 숲 레인 |
| `VehicleController.cs` | 차량 이동 로직 |
| `AnimalController.cs` | 동물 이동 로직 (점프) |
| `StaticObstacleController.cs` | 고정 장애물 |
| `ThemeSpawner.cs` | 테마 기반 생성/관리 |

---

## 10. 테스트 체크리스트

- [ ] CityLane에 차량 스폰 및 이동
- [ ] ForestLane에 동물 스폰 및 점프 이동
- [ ] ForestLane에 나무/바위 고정 배치
- [ ] Gap(틈) 존재 확인 (통과 가능)
- [ ] Object Pool 재사용 확인
- [ ] 난이도 스케일링 확인
- [ ] 쿼터뷰 방향 일치 확인
