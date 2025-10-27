using UnityEngine;
using System.Collections; // 코루틴(시간차 스폰)을 위해

public class EnemySpawner : MonoBehaviour
{
    // [추가] 싱글톤
    public static EnemySpawner Instance;

    [Header("Spawn Settings")]
    public GameObject[] monsterPrefabs; // 1. 5종류 몬스터 프리팹 배열
    public Transform[] spawnPoints; // 2. 6개의 스폰 위치 (빈 오브젝트)
    public float timeBetweenSpawns = 2f; // 3. 몬스터 생성 간격

    private int monstersToSpawn = 5; // 이번 라운드에 스폰할 총 몬스터 수

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // GameManager가 "라운드 시작!"하고 호출할 함수
    public void StartSpawning(int roundNumber)
    {
        // (예시: 라운드마다 스폰할 몬스터 수 증가)
        monstersToSpawn = 5 + (roundNumber * 2);

        // 코루틴(시간차 스폰) 시작
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        Debug.Log("몬스터 스폰 웨이브 시작! 총 " + monstersToSpawn + "마리");

        for (int i = 0; i < monstersToSpawn; i++)
        {
            // 1. 6개의 스폰 위치 중 1곳을 랜덤으로 선택
            Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // 2. 5종류 몬스터 중 1종을 랜덤으로 선택 (일단 0번만)
            // (나중에 라운드별로 다른 몬스터가 나오게 수정 가능)
            GameObject monsterToSpawn = monsterPrefabs[0];

            // 3. 몬스터 생성
            Instantiate(monsterToSpawn, randomSpawnPoint.position, randomSpawnPoint.rotation);

            // 4. 다음 스폰까지 2초 대기
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        Debug.Log("몬스터 스폰 웨이브 종료.");
    }
}