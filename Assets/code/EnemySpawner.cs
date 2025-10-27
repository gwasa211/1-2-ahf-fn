using UnityEngine;
using System.Collections;
using System.Collections.Generic; // List를 사용하기 위해

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [Header("Spawn Settings")]
    public GameObject[] monsterPrefabs; // 인스펙터에서 몬스터 프리팹 연결
    public float timeBetweenSpawns = 2f;

    private int monstersToSpawn = 5;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // GameManager가 "라운드 시작!"하고 호출할 함수
    public void StartSpawning(int roundNumber)
    {
        monstersToSpawn = 5 + (roundNumber * 2);
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        Debug.Log("몬스터 스폰 웨이브 시작! 총 " + monstersToSpawn + "마리");

        // MapGenerator로부터 '스폰 위치 리스트'를 가져옴
        List<Vector3> spawnList = MapGenerator.Instance.enemySpawnPositions;

        if (spawnList.Count == 0)
        {
            Debug.LogError("EnemySpawner: MapGenerator에 등록된 스폰 위치(2)가 없습니다!");
            yield break; // 코루틴(스폰) 중단
        }

        for (int i = 0; i < monstersToSpawn; i++)
        {
            // 리스트에서 랜덤 위치 Vector3를 뽑음
            Vector3 randomSpawnPos = spawnList[Random.Range(0, spawnList.Count)];

            // (일단 0번 몬스터만 스폰)
            GameObject monsterToSpawn = monsterPrefabs[0];

            // 랜덤 위치(Vector3)에 몬스터 생성
            Instantiate(monsterToSpawn, randomSpawnPos, Quaternion.identity);

            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        Debug.Log("몬스터 스폰 웨이브 종료.");
    }
}