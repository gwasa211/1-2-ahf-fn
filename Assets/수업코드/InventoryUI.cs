using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject slotItemPrefab;
    public Transform slotArea;
    public int totalSlotCount = 7; // <- [추가] 총 슬롯 개수 설정

    // [수정] GameObject 대신 SlotItem 스크립트 리스트로 관리
    private List<SlotItem> uiSlots = new List<SlotItem>();

    void Start()
    {
        // 1. 게임 시작 시 정해진 개수(7개)만큼 슬롯을 미리 생성
        for (int i = 0; i < totalSlotCount; i++)
        {
            GameObject newSlotObj = Instantiate(slotItemPrefab, slotArea);
            SlotItem newSlotScript = newSlotObj.GetComponent<SlotItem>();

            newSlotScript.ClearSlot(); // 생성 직후 빈 슬롯으로 만듦
            uiSlots.Add(newSlotScript); // 관리 리스트에 추가
        }
    }

    // [수정] UpdateInventoryUI 로직 전체 변경
    // 이 함수는 더 이상 슬롯을 생성하거나 파괴하지 않습니다.
    public void UpdateInventoryUI(List<ItemData> itemsToDisplay)
    {
        // 1. 모든 슬롯을 순회 (0번 ~ 6번)
        for (int i = 0; i < totalSlotCount; i++)
        {
            // 2. 만약 표시할 아이템이 이 슬롯에 해당하면
            if (i < itemsToDisplay.Count)
            {
                // 3. 해당 슬롯에 아이템 정보를 "채운다"
                uiSlots[i].ItemSetting(itemsToDisplay[i].itemSprite, itemsToDisplay[i].count.ToString());
            }
            // 4. 표시할 아이템이 더 이상 없으면
            else
            {
                // 5. 해당 슬롯을 "비운다"
                uiSlots[i].ClearSlot();
            }
        }
    }
}