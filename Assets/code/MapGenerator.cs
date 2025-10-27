using UnityEngine;
using System.Collections.Generic;
// (UnityEngine.AI는 삭제되었습니다)

public class MapGenerator : MonoBehaviour
{
    // 1. 싱글톤 인스턴스
    public static MapGenerator Instance;

    [Header("Map Settings")]
    public float tileSize = 0.11f;

    // 2. 타일 상태 (설치가능, 설치불가, 바닥아님)
    public enum TileState { Buildable, Unbuildable, NotAFloor }
    private TileState[,] tileGrid;
    private GameObject[,] floorTileObjects;
    private List<Vector2Int> availableFloorTiles = new List<Vector2Int>();

    [Header("Spawn Points")]
    // 3. EnemySpawner가 가져다 쓸 스폰 위치 리스트 (case 2에서 채워짐)
    public List<Vector3> enemySpawnPositions = new List<Vector3>();

    [Header("Map Blueprint (Data)")]
    // 0 = 설치 발판 (floorPrefab)
    // 1 = 벽 (wallPrefab)
    // 2 = 몬스터 스폰 (enemySpawnPrefab)
    // 3 = 플레이어 스폰 (floorPrefab + 플레이어 이동)
    // 4 = 골 (goalPrefab)
    private int[,] mapData = new int[8, 11]
    { // x= 0  1  2  3  4  5  6  7  8  9 10
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // y=0 (벽 '1')
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // y=1 (벽 '1')
        { 4, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2 }, // y=2 (몬스터 스폰 '2' - 8칸)
        { 4, 3, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // y=3 (플레이어 스폰 '3')
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // y=4
        { 4, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2 }, // y=5 (몬스터 스폰 '2' - 8칸)
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // y=6
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }  // y=7 (벽 '1')
    };

    [Header("Prefabs (Blocks)")]
    public GameObject floorPrefab;          // 0, 3번이 사용
    public GameObject wallPrefab;           // 1번이 사용
    public GameObject enemySpawnPrefab;     // 2번이 사용
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
        enemySpawnPositions.Clear(); // [중요] 스폰 리스트 초기화

        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numCols; x++)
            {
                // 모든 타일은 Y=0.01f에 생성
                Vector3 position = new Vector3(x * tileSize, 0.01f, y * tileSize);
                int tileType = mapData[y, x];
                GameObject prefabToSpawn = null;
                Vector3 spawnPosition = position;
                bool isFloor = false;

                // --- [수정] case 1과 2의 역할 변경 ---
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
                            CharacterController cc = player.GetComponent<CharacterController>();
                            if (cc != null) cc.enabled = false;
                            player.transform.position = new Vector3(x * tileSize, 1f, y * tileSize);
                            if (cc != null) cc.enabled = true;
                        }
                        break;

                    case 1: // 1 = [역할 변경] -> 벽 (Wall)
                        tileGrid[y, x] = TileState.NotAFloor;
                        prefabToSpawn = wallPrefab;
                        break;

                    case 2: // 2 = [역할 변경] -> 몬스터 스폰 (Enemy Spawn)
                        tileGrid[y, x] = TileState.NotAFloor;
                        prefabToSpawn = enemySpawnPrefab;
                        // 몬스터 스폰 타일 '아래'에도 바닥(floorPrefab)을 생성
                        Instantiate(floorPrefab, position, Quaternion.identity, this.transform);

                        // [중요] 이 위치를 '스폰 위치 리스트'에 저장
                        enemySpawnPositions.Add(spawnPosition);
                        break;

                    case 4: // 4 = 골 (Goal)
                        tileGrid[y, x] = TileState.NotAFloor;
                        prefabToSpawn = goalPrefab;
                        break;
                }
                // --- [수정 끝] ---

                if (prefabToSpawn != null)
                {
                    GameObject newInstance = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, this.transform);
                    if (isFloor)
                    {
                        floorTileObjects[y, x] = newInstance;
                    }
                }
                else
                {
                    if (isFloor) floorTileObjects[y, x] = null;
                }
            }
        }
    }

    // (이하 함수들은 변경 없습니다)

    // GameManager가 호출하는 '라운드 시작' 함수
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

    // PlayerInteraction이 사용하는 '월드->그리드' 변환 함수
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / tileSize);
        int y = Mathf.RoundToInt(worldPos.z / tileSize);
        return new Vector2Int(x, y);
    }

    // PlayerInteraction이 사용하는 '타일 상태' 확인 함수
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