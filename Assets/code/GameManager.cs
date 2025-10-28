using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Status")]
    public int totalLives = 20;
    public int currentRound = 0;

    [HideInInspector]
    public bool isGameStarted = false;
    private bool isCountingDown = false;
    private bool waitingForNextRoundInput = false;

    [HideInInspector]
    public bool isGameOver = false;

    [Header("UI (Links)")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI startPromptText;
    public TextMeshProUGUI countdownText;
    public GameObject aimImageObject;
    public TextMeshProUGUI nextRoundPromptText;
    public TextMeshProUGUI remainingMonstersText;

    [Header("Monster Count")]
    private int baseMonsterCount = 10;
    private int monsterIncrement = 5;
    private int totalMonstersThisRound = 0;
    private int remainingMonsters = 0;

    // --- [수정] DontDestroyOnLoad 삭제 ---
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // [수정] 이 줄을 삭제하거나 주석 처리!
            Debug.Log("[DEBUG] GameManager: Awake() 실행. Instance 설정 완료.");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    // --- [수정 끝] ---

    void Start()
    {
        Debug.Log("[DEBUG] GameManager: Start() 함수 *시작*. (Round 0 대기 상태)");

        if (roundText == null || livesText == null || startPromptText == null || countdownText == null || aimImageObject == null || nextRoundPromptText == null || remainingMonstersText == null)
        { Debug.LogError("GameManager: UI 링크 중 하나가 비어있습니다!"); }

        isGameStarted = false;
        isCountingDown = false;
        waitingForNextRoundInput = false;
        isGameOver = false;

        UpdateUI();
        UpdateRemainingMonstersUI();

        if (startPromptText != null)
        {
            startPromptText.text = "Press Spacebar to Start";
            startPromptText.gameObject.SetActive(true);
        }
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (aimImageObject != null) aimImageObject.gameObject.SetActive(false);
        if (nextRoundPromptText != null) nextRoundPromptText.gameObject.SetActive(false);
        if (remainingMonstersText != null) remainingMonstersText.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Game Over: 스페이스바 입력! 씬 재시작.");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("Game Over: 엔터 입력! 게임 종료.");
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
            return;
        }

        if (!isGameStarted && !isCountingDown && Input.GetKeyDown(KeyCode.Space))
        {
            isCountingDown = true;
            StartCoroutine(StartGameCountdown());
        }

        if (isGameStarted && waitingForNextRoundInput && Input.GetKeyDown(KeyCode.Return))
        {
            GoToNextRound();
        }

        if (isGameStarted && !waitingForNextRoundInput && Input.GetKeyDown(KeyCode.N))
        {
            Enemy[] currentEnemies = FindObjectsOfType<Enemy>();
            foreach (Enemy enemy in currentEnemies)
            {
                Destroy(enemy.gameObject);
            }
            WaveCleared();
        }
    }

    IEnumerator StartGameCountdown()
    {
        if (startPromptText != null) startPromptText.gameObject.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        countdownText.text = "3"; yield return new WaitForSeconds(1f);
        countdownText.text = "2"; yield return new WaitForSeconds(1f);
        countdownText.text = "1"; yield return new WaitForSeconds(1f);
        countdownText.text = "START!"; yield return new WaitForSeconds(0.5f);

        if (countdownText != null) countdownText.gameObject.SetActive(false);

        isGameStarted = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (aimImageObject != null) aimImageObject.SetActive(true);

        GoToNextRound();

        yield break;
    }

    public void MonsterReachedGoal(Enemy monster, int damageToLives)
    {
        OnMonsterRemoved();

        if (monster != null) Destroy(monster.gameObject);

        totalLives -= damageToLives;
        UpdateUI();

        if (totalLives <= 0 && !isGameOver)
        {
            GameOver();
        }
    }

    public void OnMonsterRemoved()
    {
        if (!isGameStarted || remainingMonsters <= 0) return;

        remainingMonsters--;
        UpdateRemainingMonstersUI();

        if (remainingMonsters <= 0 && !waitingForNextRoundInput)
        {
            WaveCleared();
        }
    }

    public void WaveCleared()
    {
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.WaveCleared();
        }

        if (nextRoundPromptText != null)
        {
            nextRoundPromptText.text = "Round Success!";
            nextRoundPromptText.gameObject.SetActive(true);
            StartCoroutine(ChangeTextAfterDelay(nextRoundPromptText, "Press Enter for Next Round", 2f));
        }
        waitingForNextRoundInput = true;
    }

    IEnumerator ChangeTextAfterDelay(TextMeshProUGUI textElement, string newText, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (textElement != null && waitingForNextRoundInput)
        {
            textElement.text = newText;
        }
        yield break;
    }

    public void GoToNextRound()
    {
        waitingForNextRoundInput = false;
        if (nextRoundPromptText != null) nextRoundPromptText.gameObject.SetActive(false);

        currentRound++;
        UpdateUI();

        if (MapGenerator.Instance == null || EnemySpawner.Instance == null)
        {
            Debug.LogError("GoToNextRound 실패: MapGenerator 또는 EnemySpawner가 없습니다!");
            return;
        }

        if (currentRound > 1)
        {
            MapGenerator.Instance.GoToNextRound();
        }

        MapGenerator.Instance.ResetPlayerPosition();

        totalMonstersThisRound = baseMonsterCount + (currentRound - 1) * monsterIncrement;
        remainingMonsters = totalMonstersThisRound;
        UpdateRemainingMonstersUI();
        if (remainingMonstersText != null) remainingMonstersText.gameObject.SetActive(true);

        EnemySpawner.Instance.StartSpawning(currentRound, totalMonstersThisRound);
    }

    void GameOver()
    {
        Debug.LogError("--- 게임 오버! ---");
        isGameOver = true;
        isGameStarted = false;

        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.StopAllSpawning();
        }

        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        Debug.Log("[DEBUG] GameOver: " + allEnemies.Length + "마리의 몬스터를 즉시 제거합니다.");
        foreach (Enemy enemy in allEnemies)
        {
            Destroy(enemy.gameObject);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (roundText != null) roundText.gameObject.SetActive(false);
        if (livesText != null) livesText.gameObject.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (aimImageObject != null) aimImageObject.gameObject.SetActive(false);
        if (nextRoundPromptText != null) nextRoundPromptText.gameObject.SetActive(false);
        if (remainingMonstersText != null) remainingMonstersText.gameObject.SetActive(false);

        if (startPromptText != null)
        {
            startPromptText.text = "GAME OVER\n<size=60%>Spacebar: Restart / Enter: Quit</size>";
            startPromptText.gameObject.SetActive(true);
        }
    }

    void UpdateUI()
    {
        if (roundText != null)
        {
            if (currentRound == 0)
            {
                roundText.gameObject.SetActive(false);
            }
            else
            {
                roundText.gameObject.SetActive(true);
                roundText.text = "Round: " + currentRound;
            }
        }
        if (livesText != null) livesText.text = "Lives: " + totalLives;
    }

    void UpdateRemainingMonstersUI()
    {
        if (remainingMonstersText != null)
        {
            if (isGameStarted && !waitingForNextRoundInput && currentRound > 0)
            {
                remainingMonstersText.gameObject.SetActive(true);
                remainingMonstersText.text = "Monsters: " + remainingMonsters + "/" + totalMonstersThisRound;
            }
            else
            {
                remainingMonstersText.gameObject.SetActive(false);
            }
        }
    }
}