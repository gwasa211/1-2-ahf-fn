using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpped = 2f;
    private Transform player;

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
}