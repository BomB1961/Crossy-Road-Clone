using UnityEngine;

/// <summary>
/// 풀밭 레인 - 안전区域, 장애물 없음
/// </summary>
public class GrassLane : BaseLane
{
    [Header("풀밭 설정")]
    [SerializeField] private GameObject[] decorationPrefabs; // 나무, 바위, 꽃 등
    [SerializeField] private int decorationCount = 3;

    public override void Initialize(int row)
    {
        base.Initialize(row);
        GenerateObstacles();
    }

    /// <summary>
    /// 장식 오브젝트 생성 (나무, 바위, 꽃 등)
    /// </summary>
    public override void GenerateObstacles()
    {
        // Decorations 폴더에서 에셋 로드 (자동)
        if (decorationPrefabs == null || decorationPrefabs.Length == 0)
        {
            LoadDecorationsFromResources();
        }

        if (decorationPrefabs == null || decorationPrefabs.Length == 0)
            return;

        for (int i = 0; i < decorationCount; i++)
        {
            int randomCol = Random.Range(-3, 4);
            Vector3 pos = new Vector3(randomCol, 0, rowIndex);

            GameObject prefab = decorationPrefabs[Random.Range(0, decorationPrefabs.Length)];
            if (prefab != null)
            {
                Instantiate(prefab, pos, Quaternion.identity, transform);
            }
        }
    }

    /// <summary>
    /// Resources에서 장식Prefab 자동 로드
    /// </summary>
    private void LoadDecorationsFromResources()
    {
        // Assets/Decorations/의 FBX 파일들을 Resources.Load로 로드
        Object[] trees = Resources.LoadAll("Decorations/tree", typeof(GameObject));
        Object[] rocks = Resources.LoadAll("Decorations/rock", typeof(GameObject));
        Object[] bushes = Resources.LoadAll("Decorations/bush", typeof(GameObject));

        System.Collections.Generic.List<GameObject> list = new System.Collections.Generic.List<GameObject>();

        foreach (Object obj in trees)
            if (obj is GameObject) list.Add((GameObject)obj);
        foreach (Object obj in rocks)
            if (obj is GameObject) list.Add((GameObject)obj);
        foreach (Object obj in bushes)
            if (obj is GameObject) list.Add((GameObject)obj);

        if (list.Count > 0)
        {
            decorationPrefabs = list.ToArray();
        }
    }

    /// <summary>
    /// 풀로 반환
    /// </summary>
    public override void ReleaseToPool()
    {
        // 자식 장식 모두 제거
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        gameObject.SetActive(false);
        transform.position = Vector3.one * 1000; // 숨김 위치
    }
}
