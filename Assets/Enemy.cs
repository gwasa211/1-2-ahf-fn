using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum EnemyState {Idle, Trace, Attack}

    public EnemyState state = EnemyState.Idle;

    public float moveSpped = 2f;
    public float traceRange = 15f;
    public float attackRange = 6f;
    public float attackCooldown = 1.5f;

    public GameObject projectilePrefab;
    public Transform firePoint;
    private Transform player;

    private float lastAttackTime;


    public int maxHealth = 5;
    private int currentHealth;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
       
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * moveSpped * Time.deltaTime;
        transform.LookAt(player.position);

        float dist = Vector3.Distance(transform.position, transform.position);
        switch (state)
        {
            case EnemyState.Idle:
                // If the player is within the trace range, start tracing
                if (dist < traceRange)
                {
                    state = EnemyState.Trace;
                }
                break;

            case EnemyState.Trace:
                // If the player is within the attack range, start attacking
                if (dist < attackRange)
                {
                    state = EnemyState.Attack;
                }
                // If the player is outside the trace range, go back to idle
                else if (dist > traceRange)
                {
                    state = EnemyState.Idle;
                }
                // Otherwise, the player is in range, so continue tracing
                else
                {
                    TracePlayer();
                }
                break;

            case EnemyState.Attack:
                // If the player moves out of the attack range, start tracing again
                if (dist > attackRange)
                {
                    state = EnemyState.Trace;
                }
                // Otherwise, continue attacking
                else
                {
                    AttackPlayer();
                }
                break;
        }
    }


    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    void TracePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * moveSpped * Time.deltaTime;
        transform.LookAt(player.position);
    }

    // 참고 1개
    void AttackPlayer()
    {
        // 일정 쿨타임마다 발사
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            ShootProjectile();
        }
    }

    // 참고 1개
    void ShootProjectile()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            transform.LookAt(player.position);

            GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
            if (ep != null)
            {
                Vector3 dir = (player.position - firePoint.position).normalized;
                ep.SetDirection(dir);
            }
        }
    }
}