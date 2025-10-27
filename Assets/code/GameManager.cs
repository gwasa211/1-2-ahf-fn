using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Status")]
    public int totalLives = 20;
    public int currentRound = 1;

    // --- [추가] ---
    [Header("Links")]
    public EnemySpawner enemySpawner; // 1. 스포너 연결
                                      // --- [추가 끝] ---

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

        // --- [추가] ---
        // 2. 스포너가 연결되었는지 확인
        if (enemySpawner == null)
        {
            Debug.LogError("GameManager: 'Enemy Spawner'가 연결되지 않았습니다!");
        }
        // --- [추가 끝] ---
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            GoToNextRound();
        }
    }

    public void MonsterReachedGoal(Enemy monster)
    {
        if (monster != null)
        {
            Destroy(monster.gameObject);
        }
        totalLives--;
        Debug.Log("몬스터가 골에 도달! 남은 목숨: " + totalLives);

        if (totalLives <= 0)
        {
            GameOver();
        }
    }

    public void GoToNextRound()
    {
        currentRound++;
        Debug.Log("--- 라운드 " + currentRound + " 시작 ---");

        if (MapGenerator.Instance != null)
        {
            MapGenerator.Instance.GoToNextRound();
        }

        // --- [추가] ---
        // 3. 스포너에게 스폰 시작 명령!
        if (enemySpawner != null)
        {
            enemySpawner.StartSpawning(currentRound);
        }
        // --- [추가 끝] ---
    }

    void GameOver()
    {
        Debug.LogError("--- 게임 오버! ---");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}