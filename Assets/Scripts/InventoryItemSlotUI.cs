using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemCountText;
    public Image slotBackground;

    public Color normalColor = new Color(0.35f, 0.35f, 0.35f, 0.75f);
    public Color selectedColor = new Color(0.85f, 0.75f, 0.35f, 0.9f);

    private ItemType itemType;
    private InventoryUI inventoryUI;

    public void SetItem(ItemType type, int count, InventoryUI ui)
    {
        itemType = type;
        inventoryUI = ui;

        if (itemNameText != null)
        {
            itemNameText.text = ItemDatabase.GetDisplayName(type);
        }

        if (itemCountText != null)
        {
            itemCountText.text = "x" + count;
        }

        SetSelected(false);
    }

    public ItemType GetItemType()
    {
        return itemType;
    }

    public void SetSelected(bool isSelected)
    {
        if (slotBackground != null)
        {
            slotBackground.color = isSelected ? selectedColor : normalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryUI != null)
        {
            inventoryUI.SelectItem(itemType);
        }
    }
}