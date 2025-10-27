using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Status")]
    public int totalLives = 20;
    public int currentRound = 1;

    // --- [삭제] ---
    // (EnemySpawner 연결 변수 삭제)
    // --- [삭제 끝] ---

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Debug.Log("--- 게임 매니저 시작 ---");
        Debug.Log("현재 목숨: " + totalLives + " | 현재 라운드: " + currentRound);

        // [삭제] 스포너 연결 확인 코드 삭제
    }

    void Update()
    {
        // N키로 다음 라운드 테스트
        if (Input.GetKeyDown(KeyCode.N))
        {
            GoToNextRound();
        }
    }

    public void MonsterReachedGoal(Enemy monster)
    {
        if (monster != null) Destroy(monster.gameObject);
        totalLives--;
        Debug.Log("몬스터가 골에 도달! 남은 목숨: " + totalLives);
        if (totalLives <= 0) GameOver();
    }

    public void GoToNextRound()
    {
        currentRound++;
        Debug.Log("--- 라운드 " + currentRound + " 시작 ---");

        if (MapGenerator.Instance != null)
        {
            MapGenerator.Instance.GoToNextRound();
        }

        // --- [수정] ---
        // 3. 인스펙터 연결 대신, 싱글톤 인스턴스를 바로 호출!
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.StartSpawning(currentRound);
        }
        else
        {
            Debug.LogError("GameManager: 씬에 'EnemySpawner' 오브젝트가 없습니다!");
        }
        // --- [수정 끝] ---
    }

    void GameOver()
    {
        Debug.LogError("--- 게임 오버! ---");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}