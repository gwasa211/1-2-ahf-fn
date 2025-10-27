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
    public List<Vector3> enemySpawnPositions = new List<Vector3>();

    [Header("Map Blueprint (Data)")]
    // 0 = 설치 발판
    // 1 = 벽
    // 2 = 몬스터 스폰
    // 3 = 플레이어 스폰
    // 4 = 골
    private int[,] mapData = new int[8, 11]
    { // x= 0  1  2  3  4  5  6  7  8  9 10
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // y=0 (벽 '1')
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // y=1 (벽 '1')
        { 4, 0, 0, 2, 2, 2, 2, 2, 2, 2, 1 }, // y=2 (몬스터 스폰 '2')
        { 4, 3, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // y=3 (플레이어 스폰 '3')
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // y=4
        { 4, 0, 0, 2, 2, 2, 2, 2, 2, 2, 1 }, // y=5 (몬스터 스폰 '2')
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, // y=6
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }  // y=7 (벽 '1')
    };

    [Header("Prefabs (Blocks)")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject enemySpawnPrefab;
    public GameObject goalPrefab;
    public GameObject unbuildableFloorPrefab;

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

                switch (tileType)
                {
                    case 0:
                    case 3:
                        prefabToSpawn = floorPrefab;
                        tileGrid[y, x] = TileState.Buildable;
                        availableFloorTiles.Add(new Vector2Int(x, y));
                        isFloor = true;

                        if (tileType == 3 && player != null)
                        {
                            MovePlayerToSpawn(x, y);
                        }
                        break;

                    case 1: // 1 = 벽
                        tileGrid[y, x] = TileState.NotAFloor;
                        prefabToSpawn = wallPrefab;
                        break;

                    case 2: // 2 = 몬스터 스폰
                        tileGrid[y, x] = TileState.NotAFloor;
                        prefabToSpawn = enemySpawnPrefab;
                        Instantiate(floorPrefab, position, Quaternion.identity, this.transform);
                        enemySpawnPositions.Add(spawnPosition);
                        break;

                    case 4: // 4 = 골
                        tileGrid[y, x] = TileState.NotAFloor;
                        prefabToSpawn = goalPrefab;
                        break;
                }

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

    // --- [오류 수정 1: WorldToGrid] ---
    // 이 함수는 '반드시' Vector2Int를 반환(return)해야 합니다.
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / tileSize);
        int y = Mathf.RoundToInt(worldPos.z / tileSize);
        return new Vector2Int(x, y); // 이 return 문이 항상 실행됩니다.
    }

    // --- [오류 수정 2: GetTileStateAtWorld] ---
    // 이 함수는 '반드시' TileState를 반환(return)해야 합니다.
    public TileState GetTileStateAtWorld(Vector3 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);

        // 1번 경로: 맵 범위를 벗어난 경우
        if (gridPos.y < 0 || gridPos.y >= tileGrid.GetLength(0) ||
            gridPos.x < 0 || gridPos.x >= tileGrid.GetLength(1))
        {
            return TileState.NotAFloor; // 여기서 반환
        }

        // 2번 경로: 맵 범위 안인 경우 (if문을 통과한 경우)
        return tileGrid[gridPos.y, gridPos.x]; // 여기서 '반드시' 반환
    }
}