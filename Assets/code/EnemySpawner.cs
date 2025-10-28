using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [Header("Spawn Settings")]
    public GameObject[] monsterPrefabs;
    public float timeBetweenSpawns = 2f;

    private bool isWaveActive = false;
    private Coroutine currentSpawnCoroutine = null; // [새 변수] 현재 실행 중인 스폰 코루틴

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartSpawning(int roundNumber, int numberOfMonsters)
    {
        Debug.Log("[DEBUG] EnemySpawner: StartSpawning() 호출 시도... (isWaveActive = " + isWaveActive + ")");
        if (isWaveActive)
        {
            Debug.LogError("[DEBUG] EnemySpawner: StartSpawning() 실패! isWaveActive가 true 상태입니다.");
            return;
        }

        // [수정] 코루틴을 변수에 저장
        currentSpawnCoroutine = StartCoroutine(SpawnWave(numberOfMonsters));
    }

    IEnumerator SpawnWave(int monstersToSpawn)
    {
        isWaveActive = true;

        if (monsterPrefabs.Length == 0 || monsterPrefabs[0] == null)
        {
            Debug.LogError("EnemySpawner: 'Monster Prefabs' 배열이 비어있습니다!");
            isWaveActive = false;
            yield break;
        }
        List<Vector3> spawnList = MapGenerator.Instance.enemySpawnPositions;
        if (spawnList.Count == 0)
        {
            Debug.LogError("EnemySpawner: MapGenerator에 등록된 스폰 위치(1)가 없습니다!");
            isWaveActive = false;
            yield break;
        }

        for (int i = 0; i < monstersToSpawn; i++)
        {
            // --- [수정] 게임 오버 감지 (GameManager.isGameOver 접근) ---
            if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            {
                Debug.Log("[DEBUG] EnemySpawner: 게임 오버 상태 감지! 몬스터 스폰을 즉시 중단합니다.");
                isWaveActive = false;
                yield break; // 코루틴 종료
            }
            // --- [수정 끝] ---

            Vector3 randomSpawnPos = spawnList[Random.Range(0, spawnList.Count)];
            Instantiate(monsterPrefabs[0], randomSpawnPos, Quaternion.identity);

            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        Debug.Log("스폰 루프 종료. (isWaveActive는 true 상태 유지)");
        currentSpawnCoroutine = null; // 코루틴 정상 종료
    }

    // 몬스터가 죽었을 때 (Enemy.cs가 호출)
    public void EnemyDied()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMonsterRemoved();
        }
    }

    // 라운드 클리어 시 (GameManager.cs가 호출)
    public void WaveCleared()
    {
        Debug.Log("[DEBUG] EnemySpawner: WaveCleared() 호출됨. isWaveActive = false로 리셋!");
        isWaveActive = false;
        currentSpawnCoroutine = null; // 웨이브가 끝났으니 코루틴 참조도 클리어
    }

    // --- [새 함수] 게임 오버 시 GameManager가 호출 ---
    public void StopAllSpawning()
    {
        Debug.Log("[DEBUG] EnemySpawner: StopAllSpawning() 호출됨. 모든 스폰 코루틴 중지!");

        // 1. 현재 실행 중인 스폰 코루틴이 있다면 중지
        if (currentSpawnCoroutine != null)
        {
            StopCoroutine(currentSpawnCoroutine);
            currentSpawnCoroutine = null;
        }

        // 2. 스포너 상태를 즉시 리셋 (스폰 불가 상태로)
        isWaveActive = false;
    }
    // --- [새 함수 끝] ---
}