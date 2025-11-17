using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotItem : MonoBehaviour
{
    public Image itemImage;
    public TextMeshProUGUI itemCountText;

    // 아이템 정보를 받아 슬롯을 채우는 함수
    public void ItemSetting(Sprite sprite, string text)
    {
        itemImage.sprite = sprite;
        itemImage.gameObject.SetActive(true); // 이미지를 켠다
        itemCountText.text = text;
        itemCountText.gameObject.SetActive(true); // 텍스트를 켠다
    }

    // [이 함수 추가] 슬롯을 비우는 함수
    public void ClearSlot()
    {
        itemImage.sprite = null;
        itemImage.gameObject.SetActive(false); // 이미지를 끈다
        itemCountText.text = "";
        itemCountText.gameObject.SetActive(false); // 텍스트를 끈다
    }
}