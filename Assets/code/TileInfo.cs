using UnityEngine;

public class TileInfo : MonoBehaviour
{
    public enum TileType
    {
        PlayerMove,       // 녹색 (설치 가능)
        EnemyMove,        // 흰색 (설치 가능)
        EnemySpawn,       // 빨간색
        GameOver,         // 파란색
        Wall,             // 벽
        RoundUnbuildable  // 라운드 설치 불가
    }

    public TileType type;
}