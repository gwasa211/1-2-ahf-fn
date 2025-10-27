using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance;

    [Header("Map Settings")]
    public float tileSize = 0.11f;

    public enum TileState { Buildable, Unbuildable, NotAFloor }
    private TileState[,] tileGrid;
    private GameObject[,] floorTileObjects;
    private List<Vector2Int> availableFloorTiles = new List<Vector2Int>();

    [Header("Spawn Points")]
    // EnemySpawner가 가져다 쓸 스폰 위치 리스트 (case 1에서 채워짐)
    public List<Vector3> enemySpawnPositions = new List<Vector3>();

    [Header("Map Blueprint (Data)")]
    // 0 = 설치 발판 (floorPrefab)
    // 1 = 몬스터 스폰 (enemySpawnPrefab)
    // 2 = 벽 (wallPrefab)
    // 3 = 플레이어 스폰 (floorPrefab + 플레이어 이동)
    // 4 = 골 (goalPrefab)

    // --- [사용자님이 주신 맵 설계도 100% 반영] ---
    private int[,] mapData = new int[8, 11]
    { // x= 0  1  2  3  4  5  6  7  8  9 10
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // y=0 (몬스터 스폰 '1')
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // y=1 (몬스터 스폰 '1')
        { 4, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2 }, // y=2 (벽 '2')
        { 4, 3, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // y=3 (몬스터 스폰 '1')
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // y=4 (몬스터 스폰 '1')
        { 4, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2 }, // y=5 (벽 '2')
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // y=6 (몬스터 스폰 '1')
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }  // y=7 (몬스터 스폰 '1')
    };
    // --- [맵 설계도 끝] ---

    [Header("Prefabs (Blocks)")]
    public GameObject floorPrefab;          // 0, 3번이 사용
    public GameObject wallPrefab;           // 2번이 사용 (벽)
    public GameObject enemySpawnPrefab;     // 1번이 사용 (몬스터 스폰)
    public GameObject goalPrefab;           // 4번이 사용
    public GameObject unbuildableFloorPrefab; // 라운드 변경 시 사용

    [Header("Player")]
    public GameObject player;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        GenerateMap();
    }

    void GenerateMap()
    {
        int numRows = mapData.GetLength(0);
        int numCols = mapData.GetLength(1);

        tileGrid = new TileState[numRows, numCols];
        floorTileObjects = new GameObject[numRows, numCols];
        availableFloorTiles.Clear();
        enemySpawnPositions.Clear();

        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numCols; x++)
            {
                Vector3 position = new Vector3(x * tileSize, 0.01f, y * tileSize);
                int tileType = mapData[y, x];
                GameObject prefabToSpawn = null;
                Vector3 spawnPosition = position;
                bool isFloor = false;

                // --- [1=스폰, 2=벽 규칙에 맞는 switch문] ---
                switch (tileType)
                {
                    case 0: // 0 = 설치 발판
                    case 3: // 3 = 플레이어 스폰 (발판)
                        prefabToSpawn = floorPrefab;
                        tileGrid[y, x] = TileState.Buildable;
                        availableFloorTiles.Add(new Vector2Int(x, y));
                        isFloor = true;

                        if (tileType == 3 && player != null)
                        {
                            MovePlayerToSpawn(x, y);
                        }
                        break;

                    case 1: // 1 = [역할] -> 몬스터 스폰 (Enemy Spawn)
                        tileGrid[y, x] = TileState.NotAFloor;
                        prefabToSpawn = enemySpawnPrefab;

                        // [수정] 겹침 버그: 스폰 위치에 바닥을 생성하던 코드 [삭제]

                        // [중요] 이 위치를 '스폰 위치 리스트'에 저장
                        enemySpawnPositions.Add(spawnPosition);
                        break;

                    case 2: // 2 = [역할] -> 벽 (Wall)
                        tileGrid[y, x] = TileState.NotAFloor;
                        prefabToSpawn = wallPrefab;

                        // [수정] 겹침 버그: 벽 위치에 바닥을 생성하던 코드 [삭제]

                        break;

                    case 4: // 4 = 골 (Goal)
                        tileGrid[y, x] = TileState.NotAFloor;
                        prefabToSpawn = goalPrefab;
                        break;
                }
                // --- [switch 끝] ---

                if (prefabToSpawn != null)
                {
                    GameObject newInstance = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, this.transform);
                    if (isFloor)
                    {
                        floorTileObjects[y, x] = newInstance;
                    }
                }
            }
        }
    }

    // (이하 GoToNextRound, ResetPlayerPosition 등 모든 함수는 변경 없습니다)

    public void GoToNextRound()
    {
        Debug.Log("MapGenerator: 설치 불가 타일을 업데이트합니다.");
        int tilesToDisable = Random.Range(3, 5);

        for (int i = 0; i < tilesToDisable; i++)
        {
            if (availableFloorTiles.Count == 0) break;
            int randomIndex = Random.Range(0, availableFloorTiles.Count);
            Vector2Int tileCoords = availableFloorTiles[randomIndex];

            tileGrid[tileCoords.y, tileCoords.x] = TileState.Unbuildable;
            availableFloorTiles.RemoveAt(randomIndex);

            GameObject oldTile = floorTileObjects[tileCoords.y, tileCoords.x];

            if (oldTile != null && unbuildableFloorPrefab != null)
            {
                Vector3 position = oldTile.transform.position;
                Quaternion rotation = oldTile.transform.rotation;
                Transform parent = oldTile.transform.parent;

                Destroy(oldTile);

                GameObject newTile = Instantiate(unbuildableFloorPrefab, position, rotation, parent);
                floorTileObjects[tileCoords.y, tileCoords.x] = newTile;
            }
            Debug.Log("타일 (" + tileCoords.x + ", " + tileCoords.y + ") 설치 불가로 변경됨.");
        }
    }

    public void ResetPlayerPosition()
    {
        Debug.Log("플레이어 위치 리셋 시도...");
        for (int y = 0; y < mapData.GetLength(0); y++)
        {
            for (int x = 0; x < mapData.GetLength(1); x++)
            {
                if (mapData[y, x] == 3)
                {
                    MovePlayerToSpawn(x, y);
                    return;
                }
            }
        }
    }

    private void MovePlayerToSpawn(int x, int y)
    {
        if (player != null)
        {
            Debug.Log("플레이어 스폰 위치(" + x + ", " + y + ")로 이동!");
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = new Vector3(x * tileSize, 1f, y * tileSize);

            if (cc != null) cc.enabled = true;
        }
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / tileSize);
        int y = Mathf.RoundToInt(worldPos.z / tileSize);
        return new Vector2Int(x, y);
    }

    public TileState GetTileStateAtWorld(Vector3 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);

        if (gridPos.y < 0 || gridPos.y >= tileGrid.GetLength(0) ||
            gridPos.x < 0 || gridPos.x >= tileGrid.GetLength(1))
        {
            return TileState.NotAFloor;
        }

        return tileGrid[gridPos.y, gridPos.x];
    }
}