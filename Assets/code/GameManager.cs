using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro UI
using System.Collections; // [새로 추가] 코루틴(카운트다운)을 위해

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Status")]
    public int totalLives = 20;
    public int currentRound = 0; // 0에서 시작
    private bool isGameStarted = false; // [새로 추가] 게임 시작 상태

    [Header("UI (Links)")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI livesText;
    // [수정] '시작 버튼' 대신 '텍스트'로 변경
    public TextMeshProUGUI startPromptText; // "Press Space..."
    public TextMeshProUGUI countdownText;   // "3, 2, 1..."

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
        Debug.Log("--- 게임 매니저 시작 (현재 0 라운드 준비 중) ---");

        if (roundText == null || livesText == null || startPromptText == null || countdownText == null)
        {
            Debug.LogError("GameManager: UI 텍스트 (Round, Lives, Start, Countdown)가 모두 연결되지 않았습니다!");
        }

        isGameStarted = false; // 게임 시작 전
        UpdateUI();

        // [수정] 시작 텍스트는 켜고, 카운트다운 텍스트는 끔
        if (startPromptText != null) startPromptText.gameObject.SetActive(true);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    void Update()
    {
        // [수정] 게임이 시작되지 않았고(false), 스페이스바를 누르면
        if (!isGameStarted && Input.GetKeyDown(KeyCode.Space))
        {
            isGameStarted = true; // 중복 입력을 막기 위해 즉시 true로 변경
            StartCoroutine(StartGameCountdown()); // 카운트다운 시작!
        }

        // [수정] 게임이 시작된 후 (true) N키가 작동
        if (isGameStarted && currentRound > 0 && Input.GetKeyDown(KeyCode.N))
        {
            GoToNextRound();
        }
    }

    // --- [삭제] ---
    // public void StartFirstRound() { ... } // 버튼 함수 삭제
    // --- [삭제 끝] ---

    // --- [새 함수] 스페이스바를 누르면 호출되는 카운트다운 코루틴 ---
    IEnumerator StartGameCountdown()
    {
        Debug.Log("카운트다운 시작!");

        // 1. "Press Space" 텍스트 숨기기
        if (startPromptText != null) startPromptText.gameObject.SetActive(false);

        // 2. 카운트다운 텍스트 보이기
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        // 3. 카운트다운
        countdownText.text = "3";
        yield return new WaitForSeconds(1f); // 1초 대기

        countdownText.text = "2";
        yield return new WaitForSeconds(1f); // 1초 대기

        countdownText.text = "1";
        yield return new WaitForSeconds(1f); // 1초 대기

        countdownText.text = "START!";
        yield return new WaitForSeconds(0.5f); // 0.5초 대기

        // 4. 카운트다운 텍스트 숨기기
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        // 5. [중요] 1 라운드 시작
        GoToNextRound();
    }
    // --- [새 함수 끝] ---

    public void MonsterReachedGoal(Enemy monster, int damageToLives)
    {
        if (monster != null) Destroy(monster.gameObject);
        totalLives -= damageToLives;
        UpdateUI();

        Debug.Log("몬스터가 골에 도달! 목숨 -" + damageToLives + " | 남은 목숨: " + totalLives);
        if (totalLives <= 0) GameOver();
    }

    public void GoToNextRound()
    {
        currentRound++; // (0 -> 1 또는 1 -> 2)
        UpdateUI();

        Debug.Log("--- 라운드 " + currentRound + " 시작 ---");

        if (MapGenerator.Instance != null)
        {
            MapGenerator.Instance.GoToNextRound();
            MapGenerator.Instance.ResetPlayerPosition();
        }

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

    void UpdateUI()
    {
        if (roundText != null)
        {
            roundText.text = "Round: " + currentRound;
        }

        if (livesText != null)
        {
            livesText.text = "Lives: " + totalLives;
        }
    }
}