using System.Collections.Generic;
using UnityEngine;

// 이 클래스는 아이템 타입과 스프라이트를 인스펙터에서 연결하기 위한 용도입니다.
[System.Serializable]
public class ItemDefinition
{
    public BlockType itemType; // ItemType 대신 BlockType을 사용
    public Sprite itemSprite;
}

public class ItemDatabase : MonoBehaviour
{
    // 1. 싱글턴: 어디서든 쉽게 접근하기 위함
    public static ItemDatabase Instance { get; private set; }

    // 2. [인스펙터에서 설정] 게임에 존재하는 모든 아이템 목록
    public List<ItemDefinition> allItems;

    // 3. [내부용] 빠른 검색을 위한 딕셔너리
    private Dictionary<BlockType, Sprite> itemDictionary;

    void Awake()
    {
        // 싱글턴 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // 4. 딕셔너리 초기화
        itemDictionary = new Dictionary<BlockType, Sprite>();
        foreach (var itemDef in allItems)
        {
            if (!itemDictionary.ContainsKey(itemDef.itemType))
            {
                itemDictionary.Add(itemDef.itemType, itemDef.itemSprite);
            }
        }
    }

    // 5. [외부 호출용] BlockType을 주면 Sprite를 반환하는 함수
    public Sprite GetSprite(BlockType type)
    {
        if (itemDictionary.ContainsKey(type))
        {
            return itemDictionary[type];
        }
        Debug.LogWarning("ItemDatabase에 " + type + " 의 스프라이트가 없습니다.");
        return null; // 해당하는 스프라이트가 없으면 null 반환
    }
}