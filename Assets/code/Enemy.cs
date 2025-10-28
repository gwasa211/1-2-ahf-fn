using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 30f;
    protected float currentHealth;
    public float damageToStructure = 1f;

    // --- [BasicEnemy에서 덮어쓸 변수들을 protected로 선언] ---
    protected float moveSpeed = 1f;
    protected float attackDamage = 1;
    protected int livesDamage = 1;
    // --- [End] ---

    [Header("UI References (Assign in Prefab)")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public GameObject healthCanvas;

    // --- [수정] goalPosition 변수 제거 (직진 이동) ---
    // private Transform goalPosition; // 제거
    // --- [수정 끝] ---

    // Start() 함수를 상속받은 클래스(BasicEnemy)에서 덮어쓸 수 있도록 'virtual' 추가
    protected virtual void Start()
    {
        // 몬스터 체력 초기화
        currentHealth = maxHealth;
        UpdateHealthUI();

        // --- [수정] Goal 찾기 로직 제거 (직진 이동) ---
        // GameObject goal = GameObject.FindWithTag("Goal"); // 제거
        // if (goal != null) { goalPosition = goal.transform; } // 제거
        // else { Debug.LogError("Enemy.cs: 씬에서 'Goal' 태그를 찾을 수 없습니다!"); } // 제거
        // --- [수정 끝] ---

        // 몬스터가 기본적으로 왼쪽을 바라보도록 초기 회전 설정 (선택 사항)
        transform.rotation = Quaternion.LookRotation(Vector3.left);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        // 1. UI 빌보드 (항상 카메라 바라보기)
        if (healthCanvas != null)
        {
            if (Camera.main != null)
            {
                healthCanvas.transform.rotation = Quaternion.LookRotation(healthCanvas.transform.position - Camera.main.transform.position);
            }
        }

        // --- [수정] 직진 이동 로직 복원 ---
        // 2. 이동 (Vector3.left 방향으로 직진)
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;
        // --- [수정 끝] ---

        // 3. 방향 전환 (Vector3.left 방향을 유지하므로 불필요)
        // transform.rotation = Quaternion.LookRotation(Vector3.left); // 이미 Start에서 설정됨
    }

    // 체력 UI 업데이트
    protected void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString("F0") + " / " + maxHealth.ToString("F0");
        }
    }

    // 외부(플레이어)에서 피격 시 호출
    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.EnemyDied();
        }
        Destroy(gameObject);
    }

    // 골에 도달했을 때 (MonsterReachedGoal 호출)
    void ReachedGoal()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.MonsterReachedGoal(this, livesDamage);
        }
    }

    // 충돌 감지 로직 (Goal 및 Block)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            ReachedGoal();
        }
        else if (other.CompareTag("Block"))
        {
            // Block 태그 감지 시 이동 멈추고 공격하는 로직이 필요하지만,
            // 현재는 직진으로만 가정하고 Goal 처리만 유지합니다.
            // (참고: 이전에 이 로직을 넣지 않기로 결정했습니다.)
        }
    }
}