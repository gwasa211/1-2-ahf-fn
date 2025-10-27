using UnityEngine;

public class BasicEnemy : Enemy // Enemy 스크립트를 상속받음
{
    // protected override void Start() // Start 함수를 '덮어쓰기'
    // {
    //     // 1. Enemy의 원래 Start() 기능을 먼저 실행 (체력 초기화, 컴포넌트 찾기 등)
    //     base.Start();

    //     // 2. 이 BasicEnemy만의 능력치로 값을 '덮어쓰기'
    //     maxHealth = 50f;
    //     currentHealth = maxHealth; // 현재 체력도 최대치로 설정
    //     moveSpeed = 2f;
    //     attackDamage = 5;
    //     livesDamage = 1;

    //     Debug.Log("BasicEnemy 생성! 체력:" + maxHealth + ", 속도:" + moveSpeed + ", 공격력:" + attackDamage + ", 목숨피해:" + livesDamage);
    // }

    protected override void Start()
    {
        // 1. Enemy의 원래 Start() 기능을 먼저 실행 (체력 초기화, 컴포넌트 찾기 등)
        base.Start();

        // 2. 이 BasicEnemy만의 능력치로 값을 '덮어쓰기'
        //    (maxHealth는 Enemy 스크립트 인스펙터에서 직접 50으로 설정 가능하므로 생략 가능)
        // maxHealth = 50f; 
        currentHealth = maxHealth; // 현재 체력은 maxHealth 값으로 초기화
        moveSpeed = 2f;
        attackDamage = 5;
        livesDamage = 1;

        // 에이전트 속도 설정 (만약 NavMeshAgent를 다시 사용한다면 필요)
        // if (agent != null) agent.speed = moveSpeed;

        Debug.Log("BasicEnemy 생성! 체력:" + maxHealth + ", 속도:" + moveSpeed + ", 공격력:" + attackDamage + ", 목숨피해:" + livesDamage);
    }

    // Update, ReachedGoal, TakeDamage, Die 등 다른 함수는
    // Enemy.cs의 기능을 그대로 사용하므로 여기서 다시 만들 필요 없음!
}