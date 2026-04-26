using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemCountText;

    private string itemName;
    private InventoryUI inventoryUI;

    public void SetItem(string name, int count, InventoryUI ui)
    {
        itemName = name;
        inventoryUI = ui;

        if (itemNameText != null)
        {
            itemNameText.text = name;
        }

        if (itemCountText != null)
        {
            itemCountText.text = "x" + count;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryUI != null)
        {
            inventoryUI.SelectItem(itemName);
        }
    }
}