using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Status")]
    public int totalLives = 20;
    public int currentRound = 1;

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
            // 1. 맵 생성기에게 타일 업데이트 명령
            MapGenerator.Instance.GoToNextRound();

            // --- [새 기능 추가] ---
            // 2. 맵 생성기에게 플레이어 위치 리셋 명령
            MapGenerator.Instance.ResetPlayerPosition();
            // --- [추가 끝] ---
        }

        // 3. 스포너에게 몬스터 생성 명령
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.StartSpawning(currentRound);
        }
        else
        {
            Debug.LogError("GameManager: 씬에 'EnemySpawner' 오브젝트가 없습니다!");
        }
    }

    void GameOver()
    {
        Debug.LogError("--- 게임 오버! ---");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}