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

    [Header("Round Management")]
    public int currentRound = 1;
    private List<Vector2Int> availableFloorTiles = new List<Vector2Int>();

    [Header("Map Blueprint (Data)")]
    private int[,] mapData = new int[8, 11]
    { // x= 0  1  2  3  4  5  6  7  8  9 10
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 }, // y=0
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 }, // y=1
        { 4, 0, 0, 1, 1, 1, 1, 1, 1, 1, 2 }, // y=2
        { 4, 0, 3, 0, 0, 0, 0, 0, 0, 0, 2 }, // y=3
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 }, // y=4
        { 4, 0, 0, 1, 1, 1, 1, 1, 1, 1, 2 }, // y=5
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 }, // y=6
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 }  // y=7
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            GoToNextRound();
        }
    }

    void GenerateMap()
    {
        int numRows = mapData.GetLength(0);
        int numCols = mapData.GetLength(1);

        tileGrid = new TileState[numRows, numCols];
        floorTileObjects = new GameObject[numRows, numCols];
        availableFloorTiles.Clear();

        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numCols; x++)
            {
                // 기본 Y축 위치 0.01f
                Vector3 position = new Vector3(x * tileSize, 0.01f, y * tileSize);

                int tileType = mapData[y, x];
                GameObject prefabToSpawn = null;

                // [수정] spawnPosition 변수가 position을 그대로 사용 (Y=0.01f 고정)
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
                            CharacterController cc = player.GetComponent<CharacterController>();
                            if (cc != null) cc.enabled = false;
                            // 플레이어 Y는 1f로 유지 (타일 높이와 무관)
                            player.transform.position = new Vector3(x * tileSize, 1f, y * tileSize);
                            if (cc != null) cc.enabled = true;
                        }
                        break;

                    case 1:
                        tileGrid[y, x] = TileState.NotAFloor;
                        prefabToSpawn = enemySpawnPrefab;
                        Instantiate(floorPrefab, position, Quaternion.identity, this.transform);
                        break;
                    case 2:
                        tileGrid[y, x] = TileState.NotAFloor;
                        prefabToSpawn = wallPrefab;

                        // --- [수정] ---
                        // 벽의 중심 높이 계산 삭제. (Y=0.01f)
                        // (벽 프리팹의 Pivot이 바닥에 있어야 함)
                        // spawnPosition.y = 0.01f + (tileSize / 2f); // [삭제]
                        // --- [수정 끝] ---
                        break;
                    case 4:
                        tileGrid[y, x] = TileState.NotAFloor;
                        prefabToSpawn = goalPrefab;
                        break;
                }

                if (prefabToSpawn != null)
                {
                    // [중요] spawnPosition.y는 항상 0.01f
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

    // (GoToNextRound 함수는 변경 없음)
    public void GoToNextRound()
    {
        currentRound++;
        Debug.Log("--- 라운드 " + currentRound + " 시작 ---");

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
                Vector3 position = oldTile.transform.position; // (Y=0.01f)
                Quaternion rotation = oldTile.transform.rotation;
                Transform parent = oldTile.transform.parent;

                Destroy(oldTile);

                GameObject newTile = Instantiate(unbuildableFloorPrefab, position, rotation, parent);
                floorTileObjects[tileCoords.y, tileCoords.x] = newTile;
            }

            Debug.Log("타일 (" + tileCoords.x + ", " + tileCoords.y + ") 설치 불가로 변경됨.");
        }
    }

    // (WorldToGrid 와 GetTileStateAtWorld 함수는 변경 없음)
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