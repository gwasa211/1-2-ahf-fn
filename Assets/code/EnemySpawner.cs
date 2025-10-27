using UnityEngine;
using System.Collections;
using System.Collections.Generic; // List를 사용하기 위해

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [Header("Spawn Settings")]
    public GameObject[] monsterPrefabs;
    public float timeBetweenSpawns = 2f;

    private int monstersToSpawn = 5;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // --- [디버깅 1] ---
        Debug.Log("EnemySpawner: Awake() 실행! (씬에 스포너가 존재합니다)");
        // --- [디버깅 끝] ---
    }

    // GameManager가 "N키"를 누르면 이 함수를 호출
    public void StartSpawning(int roundNumber)
    {
        // --- [디버깅 2] ---
        Debug.Log("EnemySpawner: StartSpawning() 호출됨! (GameManager가 N키로 명령을 내렸습니다)");
        // --- [디버깅 끝] ---

        monstersToSpawn = 5 + (roundNumber * 2);
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        // --- [디버깅 3] ---
        Debug.Log("EnemySpawner: SpawnWave() 코루틴 시작!");
        // --- [디버깅 끝] ---

        // --- [디버깅 4: 가장 흔한 오류 1] 몬스터 프리팹 연결 확인 ---
        if (monsterPrefabs.Length == 0 || monsterPrefabs[0] == null)
        {
            Debug.LogError("EnemySpawner: 'Monster Prefabs' 배열이 비어있거나 0번 프리팹이 비어있습니다! (인스펙터에서 몬스터 프리팹을 연결하세요!)");
            yield break; // 스폰 중단
        }
        // --- [디버깅 끝] ---

        // MapGenerator로부터 '스폰 위치 리스트'를 가져옴
        List<Vector3> spawnList = MapGenerator.Instance.enemySpawnPositions;

        // --- [디버깅 5: 가장 흔한 오류 2] 스폰 위치 개수 확인 ---
        if (spawnList.Count == 0)
        {
            Debug.LogError("EnemySpawner: MapGenerator에 등록된 스폰 위치(1)가 없습니다! (mapData에 '1'이 있는지, MapGenerator.cs의 switch문이 맞는지 확인!)");
            yield break; // 스폰 중단
        }
        else
        {
            Debug.Log("EnemySpawner: " + spawnList.Count + "개의 스폰 위치를 MapGenerator로부터 받았습니다.");
        }
        // --- [디버깅 끝] ---

        Debug.Log("EnemySpawner: 스폰 루프 시작! 총 " + monstersToSpawn + "마리 스폰 예정. (2초 간격)");
        for (int i = 0; i < monstersToSpawn; i++)
        {
            Vector3 randomSpawnPos = spawnList[Random.Range(0, spawnList.Count)];
            GameObject monsterToSpawn = monsterPrefabs[0];

            // --- [디버깅 6] ---
            Debug.Log("EnemySpawner: " + (i + 1) + "번째 몬스터 스폰 시도... (위치: " + randomSpawnPos + ")");
            // --- [디버깅 끝] ---

            Instantiate(monsterToSpawn, randomSpawnPos, Quaternion.identity);

            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        Debug.Log("EnemySpawner: 몬스터 스폰 웨이브 종료.");
    }
}