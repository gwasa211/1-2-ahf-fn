// BasicEnemy.cs 파일은 이전과 동일합니다. (Enemy.cs가 직진하도록 수정되었기 때문)

using UnityEngine;

public class BasicEnemy : Enemy
{
    private const float BASIC_HEALTH = 50f;
    private const float BASIC_SPEED = 2f; // 이 속도로 Vector3.left으로 이동합니다.
    private const float BASIC_ATTACK_DAMAGE = 5f;
    private const int BASIC_LIVES_DAMAGE = 1;


    protected override void Start()
    {
        // 1. Enemy의 원래 Start() 기능을 먼저 실행 
        base.Start();

        // 2. 이 BasicEnemy만의 능력치로 부모 클래스(Enemy)의 변수 값을 '덮어쓰기'
        maxHealth = BASIC_HEALTH;
        currentHealth = maxHealth;

        moveSpeed = BASIC_SPEED;
        attackDamage = BASIC_ATTACK_DAMAGE;
        livesDamage = BASIC_LIVES_DAMAGE;

        // 3. UI 갱신
        UpdateHealthUI();

        Debug.Log("BasicEnemy 생성! 체력:" + maxHealth + ", 속도:" + moveSpeed + ", 공격력:" + attackDamage + ", 목숨피해:" + livesDamage);
    }
}