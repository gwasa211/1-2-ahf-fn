using System.Collections.Generic;
using UnityEngine;
using System.Linq; // 딕셔너리 정렬 등을 위해 추가 (선택 사항)

public class Inventory : MonoBehaviour
{
    // [데이터] 딕셔너리로 아이템 타입과 개수 저장
    public Dictionary<BlockType, int> items = new Dictionary<BlockType, int>();

    [Header("Settings")]
    public int maxStack = 64;       // 최대 스택
    public int maxInventorySize = 7; // 최대 슬롯 종류 (UI 칸 수와 동일)

    [Header("Connections")]
    public InventoryUI inventoryUI; // UI 매니저 (인스펙터에서 연결)

    void Start()
    {
        if (inventoryUI == null)
        {
            Debug.LogError("InventoryUI가 연결되지 않았습니다!");
        }
        // 시작할 때 빈 UI로 업데이트
        UpdateUI();
    }

    /// <summary>
    /// 아이템을 인벤토리에 추가합니다. (Block.cs가 이 함수를 호출합니다)
    /// </summary>
    public void Add(BlockType type, int count = 1)
    {
        // 1. 'None' 타입은 추가하지 않음
        if (type == BlockType.None) return;

        // 2. 이미 가지고 있는 아이템인가? (스택 쌓기)
        if (items.ContainsKey(type))
        {
            items[type] = Mathf.Min(items[type] + count, maxStack);
        }
        // 3. 새 아이템인가? (새 슬롯 차지)
        // (0개인 아이템을 제외하고 실제 아이템 종류가 7개 미만인지 확인)
        else if (items.Count < maxInventorySize)
        {
            items[type] = Mathf.Min(count, maxStack);
        }
        // 4. 인벤토리 꽉 참
        else
        {
            Debug.Log("인벤토리가 꽉 찼습니다! (새 종류 아이템 추가 불가)");
            return; // 아이템을 추가하지 않았으므로 UI 업데이트 안 함
        }

        Debug.Log($"[Inventory] +{count} {type} (현재 총 개수: {items[type]})");
        UpdateUI(); // UI 새로고침
    }

    /// <summary>
    /// 아이템을 소비합니다.
    /// </summary>
    public bool Consume(BlockType type, int count = 1)
    {
        if (!items.TryGetValue(type, out int have) || have < count)
        {
            return false;
        }

        int newCount = have - count;

        // [중요] 개수가 0 이하가 되면 딕셔너리에서 "제거"
        if (newCount <= 0)
        {
            items.Remove(type);
        }
        else
        {
            items[type] = newCount; // 0보다 크면 개수 업데이트
        }

        Debug.Log($"[Inventory] -{count} {type} (남은 개수: {(newCount <= 0 ? 0 : newCount)})");
        UpdateUI(); // UI 새로고침
        return true;
    }

    // [추가됨] 딕셔너리 데이터를 UI가 알아볼 수 있는 List<ItemData>로 변환
    private void UpdateUI()
    {
        if (inventoryUI == null) return;

        // 1. 딕셔너리 -> List<ItemData>로 변환
        List<ItemData> uiList = new List<ItemData>();

        // 딕셔너리는 순서가 보장되지 않으므로, 키(BlockType) 순서대로 정렬 (선택 사항)
        // var sortedItems = items.OrderBy(pair => pair.Key.ToString());

        foreach (var pair in items) // 또는 sortedItems
        {
            // 수량이 0보다 큰 아이템만 UI 리스트에 추가
            if (pair.Value > 0)
            {
                uiList.Add(new ItemData
                {
                    itemName = pair.Key.ToString(),
                    count = pair.Value,
                    itemSprite = ItemDatabase.Instance.GetSprite(pair.Key) // 데이터베이스에서 스프라이트 찾아오기
                });
            }
        }

        // 2. UI 업데이트 호출 (변환된 리스트 전달)
        inventoryUI.UpdateInventoryUI(uiList);
    }
}