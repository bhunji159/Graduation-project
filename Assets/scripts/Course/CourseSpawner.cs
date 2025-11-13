using UnityEngine;
using System.Collections.Generic;
using DuckRunning.Course;
using DuckRunning.Core;

public class CourseSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform courseRoot;
    public Transform player;
    public CourseData courseData;   // 자동 주입됨(GameManager)

    [Header("Tile Prefabs")]
    public GameObject startTilePrefab0;   // z = 0
    public GameObject startTilePrefab1;   // z = 1
    public GameObject middleTilePrefab;
    public GameObject finishTilePrefab;

    [Header("Tile Layout")]
    public int tileCountZ = 20;       // 중간 타일 재활용 개수
    public int tileCountX = 4;        // 가로 타일 개수

    [Header("Decor Settings")]
    public List<GameObject> decorPrefabs;
    public float decorYOffset = 0f;
    public Vector3 decorScale = Vector3.one;

    private float tileWidth;
    private float tileLength;

    private float goalZ;             // final Z (정수배)
    private float lastPlayerZ;

    private readonly List<List<GameObject>> tileRows = new();

    void Start()
    {
        // ==== CourseData 자동 로드 ====
        courseData = GameManager.Instance?.currentCourse ?? courseData;
        if (!courseData)
        {
            Debug.LogError("[Spawner] CourseData missing!");
            return;
        }

        if (!player) { Debug.LogError("[Spawner] Player missing!"); return; }
        if (!middleTilePrefab) { Debug.LogError("[Spawner] middleTilePrefab missing!"); return; }

        // ==== 목표 Z ====
        goalZ = courseData.totalLengthMeters * 5;   // 정수배라고 주인님이 보장함

        // ==== 타일 크기 계산 ====
        BoxCollider col = middleTilePrefab.GetComponent<BoxCollider>();
        Vector3 scaled = Vector3.Scale(col.size, middleTilePrefab.transform.localScale);
        tileWidth = scaled.x;
        tileLength = scaled.z;

        // ==== 스타트 타일 z=0,1 추가 ====
        InitStartTiles();

        // ==== middle tiles: z=2부터 tileCountZ 개 생성 ====
        InitMiddleTiles(startZ: 2);

        lastPlayerZ = player.position.z;
    }

    void Update()
    {
        if (tileRows.Count == 0) return;

        // 오리보다 뒤에 있고, 3칸 뒤로 완전히 지나간 row만 재활용
        float recycleLimit = player.position.z - tileLength * 10;

        List<GameObject> firstRow = tileRows[0];
        float rowZ = firstRow[0].transform.position.z;

        if (rowZ < recycleLimit)
        {
            RecycleRow();
        }
    }

    // ===========================================================
    // z=0,1 스타트 타일 생성 → tileRows에 포함
    // ===========================================================
    void InitStartTiles()
    {
        CreateStartRow(0, startTilePrefab0);
        CreateStartRow(1, startTilePrefab1);
    }

    void CreateStartRow(int zIndex, GameObject prefab)
    {
        float zPos = zIndex * tileLength;
        var row = new List<GameObject>(tileCountX);

        for (int x = 0; x < tileCountX; x++)
        {
            float xCenter = (tileCountX - 1) * 0.5f;
            float xPos = (x - xCenter) * tileWidth;
            Vector3 pos = new Vector3(xPos, 0, zPos);

            GameObject tile = Instantiate(prefab, pos, Quaternion.identity, courseRoot);
            EnsureDecorRoot(tile.transform);

            // 스타트 타일에는 데코 없음
            row.Add(tile);
        }

        //tileRows.Add(row);
    }

    // ===========================================================
    // middle tile 초기 생성
    // ===========================================================
    void InitMiddleTiles(int startZ)
    {
        for (int i = 0; i < tileCountZ; i++)
        {
            float zPos = (startZ + i) * tileLength;
            var row = new List<GameObject>();

            for (int x = 0; x < tileCountX; x++)
            {
                float xCenter = (tileCountX - 1) * 0.5f;
                float xPos = (x - xCenter) * tileWidth;

                Vector3 pos = new Vector3(xPos, 0, zPos);
                GameObject tile = Instantiate(middleTilePrefab, pos, Quaternion.identity, courseRoot);

                EnsureDecorRoot(tile.transform);

                if (x == 0 || x == tileCountX - 1)
                    SpawnDecor(tile.transform);

                row.Add(tile);
            }

            tileRows.Add(row);
        }
    }

    // ===========================================================
    // 타일 재활용
    // ===========================================================
    void RecycleRow()
    {
        List<GameObject> row = tileRows[0];
        tileRows.RemoveAt(0);

        // 가장 마지막 row의 Z를 기준으로 nextZ 구함
        float lastZ = tileRows[^1][0].transform.position.z;
        float nextZ = lastZ + tileLength;

        // ⭐ goalZ 도달 시: middleTile 파괴 + finishTile을 스폰
        if (Mathf.Approximately(nextZ, goalZ))
        {
            foreach (var tile in row)
                Destroy(tile);

            Debug.Log("[Spawner] Goal reached → finish tile spawned at Z=" + nextZ);

            SpawnFinishTile(nextZ);
            return;
        }

        // ⭐ 일반 재활용
        for (int x = 0; x < row.Count; x++)
        {
            GameObject tile = row[x];

            Vector3 p = tile.transform.position;
            p.z = nextZ;
            tile.transform.position = p;

            Transform decorRoot = EnsureDecorRoot(tile.transform);
            for (int i = decorRoot.childCount - 1; i >= 0; i--)
                Destroy(decorRoot.GetChild(i).gameObject);

            if (x == 0 || x == row.Count - 1)
                SpawnDecor(tile.transform);
        }

        tileRows.Add(row);
    }

    // ===========================================================
    // 결승 타일 스폰
    // ===========================================================
    void SpawnFinishTile(float zPos)
    {
        var row = new List<GameObject>(tileCountX);

        for (int x = 0; x < tileCountX; x++)
        {
            float xCenter = (tileCountX - 1) * 0.5f;
            float xPos = (x - xCenter) * tileWidth;

            Vector3 pos = new Vector3(xPos, 0, zPos);
            GameObject tile = Instantiate(finishTilePrefab, pos, Quaternion.identity, courseRoot);

            EnsureDecorRoot(tile.transform);

            row.Add(tile);
        }

        tileRows.Add(row);
    }

    // ===========================================================
    // DecorRoot
    // ===========================================================
    Transform EnsureDecorRoot(Transform tile)
    {
        Transform root = tile.Find("DecorRoot");
        if (!root)
        {
            GameObject obj = new GameObject("DecorRoot");
            root = obj.transform;
            root.SetParent(tile, false);
        }
        return root;
    }

    // ===========================================================
    // 데코 스폰
    // ===========================================================
    void SpawnDecor(Transform tile)
    {
        if (decorPrefabs == null || decorPrefabs.Count == 0) return;

        GameObject prefab = decorPrefabs[Random.Range(0, decorPrefabs.Count)];
        if (!prefab) return;

        Transform root = EnsureDecorRoot(tile);

        Vector3 pos = tile.position + new Vector3(0, decorYOffset, 0);
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity, root);
        obj.transform.localScale = decorScale;
    }
}
