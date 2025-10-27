using UnityEngine;
// using UnityEngine.AI; // [삭제]

// [삭제] [RequireComponent(typeof(NavMeshAgent))] 
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    protected float currentHealth;
    public float moveSpeed = 1f; // (PvZ처럼 천천히)

    [Header("Components")]
    // [삭제] protected NavMeshAgent agent;
    protected Animator animator;

    // [추가] 몬스터가 골에 도달했다고 판단할 X 좌표
    public float goalLineX = 0f;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        // [삭제] agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // [삭제] AI 목표 설정 코드 전부 삭제
    }

    protected virtual void Update()
    {
        // --- [핵심 수정] ---
        // 1. AI 길찾기 대신, 그냥 왼쪽(X축 -)으로 직진
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        // (애니메이션 처리는 유지)
        if (animator != null)
        {
            // (속도가 0보다 크면 항상 걷는 애니메이션)
            animator.SetFloat("Speed", moveSpeed > 0 ? 1f : 0f);
        }

        // 2. 몬스터의 X 위치가 '골 라인'을 넘었는지 확인
        if (transform.position.x <= goalLineX)
        {
            ReachedGoal();
        }
        // --- [수정 끝] ---
    }

    // (ReachedGoal 함수는 변경 없음)
    protected virtual void ReachedGoal()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.MonsterReachedGoal(this);
        }
        this.enabled = false;
        // [삭제] agent.enabled = false; 
    }

    // (TakeDamage 함수는 변경 없음)
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // (Die 함수는 agent.enabled = false; 부분만 삭제됨)
    protected virtual void Die()
    {
        Debug.Log(gameObject.name + "가 죽었습니다.");
        if (animator != null) animator.SetTrigger("Die");

        // [삭제] agent.enabled = false; 
        GetComponent<Collider>().enabled = false;
        this.enabled = false;

        // [수정] 죽으면 속도 0으로
        moveSpeed = 0f;

        Destroy(gameObject, 2f);
    }
}