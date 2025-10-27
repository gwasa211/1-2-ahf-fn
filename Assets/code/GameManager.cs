using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.UI; // 조준선 (Image)

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Status")]
    public int totalLives = 20;
    public int currentRound = 0;

    [HideInInspector]
    public bool isGameStarted = false;
    private bool isCountingDown = false;

    // --- [수정] 이 변수가 빠져있었습니다! (클래스 레벨에 추가) ---
    private bool waitingForNextRoundInput = false;
    // --- [수정 끝] ---

    [Header("UI (Links)")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI startPromptText;
    public TextMeshProUGUI countdownText;
    public GameObject aimImageObject;
    public TextMeshProUGUI roundSuccessText;
    public TextMeshProUGUI nextRoundPromptText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[DEBUG] GameManager: Awake() 실행. Instance 설정 완료.");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Debug.Log("[DEBUG] GameManager: Start() 함수 *시작*.");

        if (roundText == null || livesText == null || startPromptText == null || countdownText == null || aimImageObject == null || roundSuccessText == null || nextRoundPromptText == null)
        {
            Debug.LogError("GameManager: UI 링크 중 하나가 비어있습니다!");
        }

        isGameStarted = false;
        isCountingDown = false;
        waitingForNextRoundInput = false; // (변수 초기화)

        UpdateUI();

        if (startPromptText != null)
        {
            startPromptText.gameObject.SetActive(true);
            Debug.Log("[DEBUG] 'StartPromptText' 활성화 시도.");
        }

        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (aimImageObject != null) aimImageObject.SetActive(false);
        if (roundSuccessText != null) roundSuccessText.gameObject.SetActive(false);
        if (nextRoundPromptText != null) nextRoundPromptText.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("[DEBUG] GameManager: Start() 함수 *종료*.");
    }

    void Update()
    {
        if (!isGameStarted && !isCountingDown && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[DEBUG] 스페이스바 입력 감지!");
            isCountingDown = true;
            StartCoroutine(StartGameCountdown());
        }

        // (waitingForNextRoundInput 변수를 사용)
        if (isGameStarted && waitingForNextRoundInput && Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("[DEBUG] 엔터 키 입력 감지!");
            GoToNextRound();
        }
    }

    IEnumerator StartGameCountdown()
    {
        Debug.Log("[DEBUG] 카운트다운 코루틴 시작!");

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

        Debug.Log("[DEBUG] 1 라운드 시작을 위해 GoToNextRound() 호출...");
        GoToNextRound();
    }

    public void MonsterReachedGoal(Enemy monster, int damageToLives)
    {
        if (monster != null) Destroy(monster.gameObject);
        totalLives -= damageToLives;
        UpdateUI();

        Debug.Log("몬스터가 골에 도달! 목숨 -" + damageToLives + " | 남은 목숨: " + totalLives);
        if (totalLives <= 0) GameOver();
    }

    public void WaveCleared()
    {
        Debug.Log("[DEBUG] GameManager: 웨이브 클리어! 엔터 키 대기 상태로 전환.");

        if (roundSuccessText != null)
        {
            roundSuccessText.gameObject.SetActive(true);
            StartCoroutine(HideTextAfterDelay(roundSuccessText, 2f));
        }

        if (nextRoundPromptText != null)
        {
            nextRoundPromptText.gameObject.SetActive(true);
        }

        // (waitingForNextRoundInput 변수를 사용)
        waitingForNextRoundInput = true;
    }

    // --- [수정] 코루틴(IEnumerator) 오류 수정 ---
    IEnumerator HideTextAfterDelay(TextMeshProUGUI textElement, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (textElement != null)
        {
            textElement.gameObject.SetActive(false);
        }
        yield break; // (CS0161 오류 방지를 위해 명시적으로 코루틴 종료)
    }
    // --- [수정 끝] ---

    public void GoToNextRound()
    {
        // (waitingForNextRoundInput 변수를 사용)
        waitingForNextRoundInput = false;
        if (nextRoundPromptText != null) nextRoundPromptText.gameObject.SetActive(false);

        currentRound++;
        UpdateUI();

        Debug.Log("--- [라운드 " + currentRound + "] ---");

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

        int monstersToSpawnThisRound = (currentRound == 1) ? 10 : (5 + (currentRound * 2));

        EnemySpawner.Instance.StartSpawning(currentRound, monstersToSpawnThisRound);

        Debug.Log("GoToNextRound: 모든 함수 호출 완료.");
    }

    void GameOver()
    {
        Debug.LogError("--- 게임 오버! ---");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
}