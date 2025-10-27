using UnityEngine;
using System.Collections;
using System.Collections.Generic; // List를 사용하기 위해

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [Header("Spawn Settings")]
    public GameObject[] monsterPrefabs;
    public float timeBetweenSpawns = 2f;

    // --- [새 기능 추가] ---
    private List<Enemy> activeEnemies = new List<Enemy>(); // 현재 살아있는 몬스터 리스트
    private bool isWaveActive = false; // 현재 웨이브 진행 중인지 여부
                                       // --- [추가 끝] ---

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Debug.Log("EnemySpawner: Awake() 실행!");
    }

    // [수정] 스폰할 몬스터 수를 매개변수로 받음
    public void StartSpawning(int roundNumber, int numberOfMonsters)
    {
        if (isWaveActive)
        {
            Debug.LogWarning("EnemySpawner: 이미 웨이브가 진행 중입니다!");
            return;
        }
        StartCoroutine(SpawnWave(numberOfMonsters));
    }

    // [수정] 스폰할 몬스터 수를 매개변수로 받음
    IEnumerator SpawnWave(int monstersToSpawn)
    {
        Debug.Log("EnemySpawner: SpawnWave() 코루틴 시작! 총 " + monstersToSpawn + "마리 스폰 예정.");
        isWaveActive = true; // 웨이브 시작!
        activeEnemies.Clear(); // 이전 라운드 찌꺼기 제거

        if (monsterPrefabs.Length == 0 || monsterPrefabs[0] == null)
        {
            Debug.LogError("EnemySpawner: 'Monster Prefabs' 배열이 비어있습니다!");
            isWaveActive = false; // 웨이브 실패
            yield break;
        }

        List<Vector3> spawnList = MapGenerator.Instance.enemySpawnPositions;

        if (spawnList.Count == 0)
        {
            Debug.LogError("EnemySpawner: MapGenerator에 등록된 스폰 위치(1)가 없습니다!");
            isWaveActive = false; // 웨이브 실패
            yield break;
        }
        else
        {
            Debug.Log("EnemySpawner: " + spawnList.Count + "개의 스폰 위치 확인.");
        }

        Debug.Log("EnemySpawner: 스폰 루프 시작. (2초 간격)");
        for (int i = 0; i < monstersToSpawn; i++)
        {
            Vector3 randomSpawnPos = spawnList[Random.Range(0, spawnList.Count)];
            GameObject monsterToSpawnPrefab = monsterPrefabs[0];

            GameObject newMonsterObject = Instantiate(monsterToSpawnPrefab, randomSpawnPos, Quaternion.identity);
            Enemy newEnemy = newMonsterObject.GetComponent<Enemy>(); // 스폰된 몬스터의 Enemy 스크립트 가져오기

            if (newEnemy != null)
            {
                activeEnemies.Add(newEnemy); // 살아있는 몬스터 리스트에 추가
            }
            else
            {
                Debug.LogError("EnemySpawner: 스폰된 몬스터 프리팹에 Enemy 스크립트가 없습니다!");
            }

            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        Debug.Log("EnemySpawner: 몬스터 스폰 루프 종료. (" + activeEnemies.Count + "마리 생성됨)");
        // 웨이브 종료 시점은 몬스터가 다 죽었을 때이므로 isWaveActive는 여기서 false로 바꾸지 않음
    }

    // --- [새 함수] Enemy.cs가 Die()할 때 이 함수를 호출 ---
    public void EnemyDied(Enemy deadEnemy)
    {
        if (activeEnemies.Contains(deadEnemy))
        {
            activeEnemies.Remove(deadEnemy); // 리스트에서 제거
            Debug.Log("EnemySpawner: 적 사망 처리. 남은 적: " + activeEnemies.Count);

            // 모든 적이 제거되었고, 웨이브가 아직 끝나지 않았다면
            if (activeEnemies.Count == 0 && isWaveActive)
            {
                WaveCleared();
            }
        }
    }
    // --- [새 함수 끝] ---

    // --- [새 함수] 모든 적이 죽었을 때 호출됨 ---
    void WaveCleared()
    {
        Debug.Log("EnemySpawner: 웨이브 클리어!");
        isWaveActive = false; // 웨이브 종료!

        // GameManager에게 웨이브 클리어 신호 보내기
        if (GameManager.Instance != null)
        {
            GameManager.Instance.WaveCleared();
        }
    }
    // --- [새 함수 끝] ---
}