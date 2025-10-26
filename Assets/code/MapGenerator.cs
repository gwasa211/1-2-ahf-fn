using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 추가

public class MapGenerator : MonoBehaviour
{
    // --- [추가] 싱글톤 ---
    // (PlayerInteraction 스크립트가 MapGenerator를 쉽게 찾기 위함)
    public static MapGenerator Instance;

    [Header("Map Settings")]
    public float tileSize = 0.11f;

    // --- [추가] 타일 상태 ---
    public enum TileState { Buildable, Unbuildable, NotAFloor }
    private TileState[,] tileGrid; // 맵 타일의 상태를 저장할 2D 배열

    // --- [추가] 라운드 관리 ---
    public int currentRound = 1;
    // (설치 불가능하게 만들 수 있는) 남은 바닥 타일 좌표 리스트
    private List<Vector2Int> availableFloorTiles = new List<Vector2Int>();

    [Header("Map Blueprint (Data)")]
    // (스폰 위치 [3, 2]로 수정됨)
    private int[,] mapData = new int[7, 11]
    {
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 },
        { 4, 0, 0, 0, 1, 1, 1, 1, 1, 0, 2 },
        { 4, 0, 0, 0, 1, 1, 1, 1, 1, 0, 2 },
        { 4, 0, 3, 0, 0, 0, 0, 0, 0, 0, 2 },
        { 4, 0, 0, 0, 1, 1, 1, 1, 1, 0, 2 },
        { 4, 0, 0, 0, 1, 1, 1, 1, 1, 0, 2 },
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 }
    };

    [Header("Prefabs (Blocks)")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject enemySpawnPrefab;
    public GameObject goalPrefab;

    [Header("Player")]
    public GameObject player;

    // Start() 대신 Awake() 사용 (싱글톤 초기화)
    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        GenerateMap();
    }

    // [임시] 다음 라운드 테스트용
    void Update()
    {
        // N 키를 누르면 다음 라운드로 (테스트용)
        if (Input.GetKeyDown(KeyCode.N))
        {
            GoToNextRound();
        }
    }

    void GenerateMap()
    {
        int numRows = mapData.GetLength(0);
        int numCols = mapData.GetLength(1);

        // [추가] 맵 데이터 저장소 초기화
        tileGrid = new TileState[numRows, numCols];
        availableFloorTiles.Clear();

        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numCols; x++)
            {
                Vector3 position = new Vector3(x * tileSize, 0, y * tileSize);
                int tileType = mapData[y, x];
                GameObject prefabToSpawn = null;
                Vector3 spawnPosition = position;

                switch (tileType)
                {
                    case 0: // 0 = 바닥 (흰색)
                    case 3: // 3 = 플레이어 스폰 (흰색 바닥)
                        prefabToSpawn = floorPrefab;

                        // [추가] 맵 데이터에 "설치 가능"으로 기록
                        tileGrid[y, x] = TileState.Buildable;
                        availableFloorTiles.Add(new Vector2Int(x, y)); // 랜덤 선택 후보로 추가

                        if (tileType == 3 && player != null) // 스폰 기능
                        {
                            CharacterController cc = player.GetComponent<CharacterController>();
                            if (cc != null) cc.enabled = false;
                            player.transform.position = new Vector3(x * tileSize, 1f, y * tileSize);
                            if (cc != null) cc.enabled = true;
                        }
                        break;

                    case 1: // 1 = 적 스폰
                    case 2: // 2 = 벽
                    case 4: // 4 = 골
                        // [추가] 맵 데이터에 "바닥 아님"으로 기록
                        tileGrid[y, x] = TileState.NotAFloor;

                        if (tileType == 1) // 적 스폰
                        {
                            prefabToSpawn = enemySpawnPrefab;
                            Instantiate(floorPrefab, position, Quaternion.identity, this.transform);
                        }
                        else if (tileType == 2) // 벽
                        {
                            prefabToSpawn = wallPrefab;
                            spawnPosition.y = tileSize / 2f;
                        }
                        else if (tileType == 4) // 골
                        {
                            prefabToSpawn = goalPrefab;
                        }
                        break;
                }

                if (prefabToSpawn != null)
                {
                    Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, this.transform);
                }
            }
        }
    }

    // --- [새 함수] 다음 라운드 시작 ---
    public void GoToNextRound()
    {
        currentRound++;
        Debug.Log("--- 라운드 " + currentRound + " 시작 ---");

        // 3~4칸을 랜덤으로 설치 불가로 변경
        int tilesToDisable = Random.Range(3, 5); // 3 또는 4

        for (int i = 0; i < tilesToDisable; i++)
        {
            // 남은 후보가 없으면 중단
            if (availableFloorTiles.Count == 0)
            {
                Debug.Log("더 이상 비활성화할 타일이 없습니다.");
                break;
            }

            // 후보 중에서 랜덤으로 1개 선택
            int randomIndex = Random.Range(0, availableFloorTiles.Count);
            Vector2Int tileCoords = availableFloorTiles[randomIndex];

            // 맵 데이터 변경: 설치 불가
            tileGrid[tileCoords.y, tileCoords.x] = TileState.Unbuildable;

            // 후보 리스트에서 제거 (다시 선택 안 되도록)
            availableFloorTiles.RemoveAt(randomIndex);

            Debug.Log("타일 (" + tileCoords.x + ", " + tileCoords.y + ") 설치 불가로 변경됨.");
            // (참고: 여기에 '설치 불가' 마크(데칼)를 생성하는 코드를 넣을 수 있음)
        }
    }

    // --- [새 함수] PlayerInteraction이 맵 좌표를 물어볼 때 사용 ---
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        // 3D 월드 좌표 -> 2D 맵 좌표로 변환
        int x = Mathf.RoundToInt(worldPos.x / tileSize);
        int y = Mathf.RoundToInt(worldPos.z / tileSize); // Z축이 맵의 Y
        return new Vector2Int(x, y);
    }

    // --- [새 함수] PlayerInteraction이 타일 상태를 물어볼 때 사용 ---
    public TileState GetTileStateAtWorld(Vector3 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);

        // 맵 범위를 벗어났는지 확인
        if (gridPos.y < 0 || gridPos.y >= tileGrid.GetLength(0) ||
            gridPos.x < 0 || gridPos.x >= tileGrid.GetLength(1))
        {
            return TileState.NotAFloor;
        }

        return tileGrid[gridPos.y, gridPos.x];
    }
}