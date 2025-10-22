using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    // 빌드 UI 패널 (슬롯들의 부모 오브젝트)
    public GameObject buildPanel;

    // 기물 슬롯의 UI Image들
    // (인스펙터에서 1, 2, 3, 4번 기물 순서대로 연결)
    public List<Image> itemSlots;

    public Vector2 normalSize = new Vector2(100f, 100f);
    public Vector2 selectedSize = new Vector2(120f, 120f);

    void Start()
    {
        // 시작할 땐 빌드 패널 숨기기
        ShowBuildPanel(false);
    }

    // PlayerInteraction이 호출할 함수 1
    public void ShowBuildPanel(bool show)
    {
        if (buildPanel != null)
            buildPanel.SetActive(show);
    }

    // PlayerInteraction이 호출할 함수 2
    public void UpdateSelection(int selectedIndex)
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (i == selectedIndex)
            {
                // 선택된 슬롯
                itemSlots[i].rectTransform.sizeDelta = selectedSize;
                itemSlots[i].color = Color.white;
            }
            else
            {
                // 선택 안 된 슬롯
                itemSlots[i].rectTransform.sizeDelta = normalSize;
                itemSlots[i].color = Color.gray;
            }
        }
    }
}