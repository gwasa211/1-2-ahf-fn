using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 10;

    void Update()
    {
        // 매 프레임 앞으로 날아감
        // (transform.forward가 총알의 앞방향이 되도록 프리팹 설정)
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // 'Is Trigger'가 켜진 Collider가 다른 Collider와 부딪혔을 때 호출됨
    void OnTriggerEnter(Collider other)
    {
        // 1. 부딪힌 오브젝트에서 TileInfo 스크립트를 가져오기
        TileInfo tile = other.GetComponent<TileInfo>();

        // 2. 만약 TileInfo가 있고, 그 타입이 'Wall'이라면
        if (tile != null && tile.type == TileInfo.TileType.Wall)
        {
            // 이 탄환(gameObject)을 파괴
            Destroy(gameObject);
            return; // 아래 코드는 실행 안 함
        }

        // 3. (나중에 추가) 만약 '적(Enemy)' 태그를 가진 녀석과 부딪혔다면
        if (other.CompareTag("Enemy"))
        {
            // 적에게 데미지를 주고
            // other.GetComponent<EnemyHealth>().TakeDamage(damage);

            // 이 탄환(gameObject)을 파괴
            Destroy(gameObject);
        }

        // (참고) 만약 '벽(Wall)'이 아닌 다른 타일(녹색, 흰색 등)에 맞아도
        // 사라지게 하려면, 'return' 키워드 위에
        // else if (tile != null) { Destroy(gameObject); return; }
        // 를 추가하면 됩니다.
    }
}