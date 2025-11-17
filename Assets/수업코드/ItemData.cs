using UnityEngine;

// [System.Serializable]을 붙여야 인스펙터 창에서 보입니다.
[System.Serializable]
public class ItemData
{
    public Sprite itemSprite; // 아이템 아이콘
    public int count;         // 아이템 개수
    public string itemName;   // 아이템 이름 (BlockType과 맞출 것)
}