using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    protected float currentHealth;
    public float moveSpeed = 1f;
    public int attackDamage = 5; // [추가] 기물에 주는 공격력
    public float attackRate = 1f; // [추가] 공격 속도 (1초에 1번)
    public float attackDistance = 0.5f; // [추가] 공격 사거리
    public int livesDamage = 1; // [추가] 골 도착 시 목숨 데미지

    [Header("Components")]
    protected Animator animator;

    [Header("Goal")]
    public float goalLineX = 0.05f;

    // [추가] 공격 관련 변수
    protected float nextAttackTime = 0f;
    protected bool isAttacking = false;
    protected Transform currentTargetStructure = null; // 현재 공격 중인 기물

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        // 공격 중이 아닐 때만 이동 및 골 체크
        if (!isAttacking)
        {
            Move();
            CheckGoal();
        }

        // 기물 감지 및 공격 처리
        DetectAndAttackStructure();

        // 애니메이션 업데이트
        UpdateAnimation();
    }

    // [수정] 이동 로직 분리
    protected virtual void Move()
    {
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
    }

    // [수정] 골 체크 로직 분리
    protected virtual void CheckGoal()
    {
        if (transform.position.x <= goalLineX)
        {
            ReachedGoal();
        }
    }

    // [새 함수] 기물 감지 및 공격
    protected virtual void DetectAndAttackStructure()
    {
        RaycastHit hit;
        // 몬스터 정면 약간 앞에서 아래로 레이를 쏨 (기물이 바닥에 있으므로)
        Vector3 rayStart = transform.position + transform.forward * 0.1f + Vector3.up * 0.1f; // 약간 앞, 약간 위
        Debug.DrawRay(rayStart, Vector3.left * attackDistance, Color.red); // 디버그용 레이 표시

        // 왼쪽(Vector3.left)으로 attackDistance만큼 레이를 쏴서 'Block' 태그를 가진 물체 감지
        if (Physics.Raycast(rayStart, Vector3.left, out hit, attackDistance))
        {
            if (hit.collider.CompareTag("Block")) // 'Block' 태그 감지!
            {
                isAttacking = true; // 공격 상태로 전환
                currentTargetStructure = hit.transform; // 공격 대상 저장

                // 공격 쿨타임 확인
                if (Time.time >= nextAttackTime)
                {
                    AttackStructure(hit.collider.gameObject); // 공격 실행
                    nextAttackTime = Time.time + 1f / attackRate; // 다음 공격 시간 설정
                }
                return; // 공격 중이므로 아래 로직(isAttacking = false) 실행 안 함
            }
        }

        // 레이에 아무것도 맞지 않거나, 'Block' 태그가 아니면
        isAttacking = false; // 공격 상태 해제
        currentTargetStructure = null; // 공격 대상 초기화
    }

    // [새 함수] 기물 공격 실행 (애니메이션, 데미지 처리)
    protected virtual void AttackStructure(GameObject structure)
    {
        Debug.Log(gameObject.name + "가 " + structure.name + "을 공격!");
        if (animator != null)
        {
            animator.SetTrigger("Attack"); // 공격 애니메이션 (트리거 이름은 애니메이터 설정에 맞게)
        }

        // 기물에 데미지 주기 (기물 스크립트에 TakeDamage 함수가 있다고 가정)
        StructureHealth structureHealth = structure.GetComponent<StructureHealth>(); // 예시 스크립트 이름
        if (structureHealth != null)
        {
            structureHealth.TakeDamage(attackDamage);
        }
        else
        {
            // 임시: TakeDamage가 없으면 그냥 파괴 (테스트용)
            // Destroy(structure);
            Debug.LogWarning(structure.name + "에 데미지를 줄 스크립트(예: StructureHealth)가 없습니다.");
        }
    }


    // [수정] 애니메이션 업데이트 로직 분리
    protected virtual void UpdateAnimation()
    {
        if (animator != null)
        {
            // 공격 중이면 속도 0, 아니면 이동 속도 반영
            float currentSpeed = isAttacking ? 0f : moveSpeed;
            animator.SetFloat("Speed", currentSpeed > 0 ? 1f : 0f);
        }
    }


    // [수정] 골 도달 시 livesDamage 사용
    protected virtual void ReachedGoal()
    {
        if (GameManager.Instance != null)
        {
            // GameManager에 목숨 데미지 값(livesDamage) 전달
            GameManager.Instance.MonsterReachedGoal(this, livesDamage);
        }
        this.enabled = false;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log(gameObject.name + "가 죽었습니다.");
        if (animator != null) animator.SetTrigger("Die");

        GetComponent<Collider>().enabled = false;
        this.enabled = false;

        moveSpeed = 0f;
        Destroy(gameObject, 2f);
    }
}


// --- [추가] 기물이 가질 예시 체력 스크립트 ---
// 기물 프리팹에 이 스크립트를 붙이고 체력을 설정하세요.
public class StructureHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " 체력: " + currentHealth + "/" + maxHealth);
        if (currentHealth <= 0)
        {
            Destroy(gameObject); // 체력이 0 이하면 파괴
        }
    }
}
// --- [예시 스크립트 끝] ---