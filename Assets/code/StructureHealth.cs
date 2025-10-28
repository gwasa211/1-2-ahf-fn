using UnityEngine;
using UnityEngine.UI; // Slider를 사용하기 위해 추가
using TMPro; // TextMeshProUGUI를 사용하기 위해 추가

public class StructureHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    // --- [새 변수] UI 관련 변수 추가 ---
    [Header("UI References (Assign in Prefab)")]
    public Slider healthSlider;        // 체력 바 슬라이더
    public TextMeshProUGUI healthText; // 체력 숫자 텍스트
    public GameObject healthCanvas;    // HealthBarUI를 감싼 Canvas
    // --- [새 변수 끝] ---

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI(); // 초기 체력 상태를 UI에 반영
    }

    void Update()
    {
        if (healthCanvas != null)
        {
            // --- [새 기능] UI가 항상 카메라를 바라보도록 설정 (빌보드 효과) ---
            if (Camera.main != null)
            {
                // 몬스터와 동일하게 UI를 월드 공간에서 카메라 쪽으로 회전시킵니다.
                healthCanvas.transform.rotation = Quaternion.LookRotation(healthCanvas.transform.position - Camera.main.transform.position);
            }
            // --- [새 기능 끝] ---
        }
    }

    // 몬스터의 공격 등 외부에서 피격 시 호출
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        UpdateHealthUI(); // 피격 후 UI 업데이트

        if (currentHealth <= 0)
        {
            // 파괴될 때 몬스터 제거 로직은 없음
            Destroy(gameObject);
        }
    }

    // --- [새 함수] 체력 UI 업데이트 ---
    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        if (healthText != null)
        {
            // 정수로 표시 (예: 100 / 100)
            healthText.text = currentHealth.ToString("F0") + " / " + maxHealth.ToString("F0");
        }
    }
    // --- [새 함수 끝] ---
}