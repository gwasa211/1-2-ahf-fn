using UnityEngine;

public class Enemy : MonoBehaviour
{
    // (... 변수 선언은 동일 ...)
    [Header("Stats")]
    public float maxHealth = 100f;
    protected float currentHealth;
    public float moveSpeed = 1f;
    public int attackDamage = 5;
    public float attackRate = 1f;
    public float attackDistance = 0.5f;
    public int livesDamage = 1;
    [Header("Components")]
    protected Animator animator;
    [Header("Goal")]
    public float goalLineX = 0.05f;

    protected float nextAttackTime = 0f;
    protected bool isAttacking = false;
    protected Transform currentTargetStructure = null;

    protected virtual void Start() { /* ... */ }
    protected virtual void Update() { /* ... */ }
    protected virtual void Move() { /* ... */ }
    protected virtual void CheckGoal() { /* ... */ }
    protected virtual void DetectAndAttackStructure() { /* ... */ }
    protected virtual void AttackStructure(GameObject structure) { /* ... */ }
    protected virtual void UpdateAnimation() { /* ... */ }
    protected virtual void ReachedGoal() { /* ... */ }
    public void TakeDamage(int damageAmount) { /* ... */ }

    // --- [수정] Die 함수 ---
    protected virtual void Die()
    {
        Debug.Log(gameObject.name + "가 죽었습니다.");
        if (animator != null) animator.SetTrigger("Die");

        GetComponent<Collider>().enabled = false;
        this.enabled = false;
        moveSpeed = 0f;

        // [추가!] 내가 죽었다고 EnemySpawner에게 알림
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.EnemyDied(this);
        }

        Destroy(gameObject, 2f); // (알림 후에 파괴)
    }
    // --- [수정 끝] ---
}

// (StructureHealth 예시 스크립트는 변경 없음)
public class StructureHealth : MonoBehaviour { /* ... */ }