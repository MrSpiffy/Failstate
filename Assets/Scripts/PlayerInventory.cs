using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private Dictionary<ItemType, int> itemCounts = new Dictionary<ItemType, int>();

    public event Action OnInventoryChanged;

    void Awake()
    {
        InitializeInventory();
    }

    void InitializeInventory()
    {
        foreach (ItemType itemType in ItemDatabase.GetAllItemTypes())
        {
            if (!itemCounts.ContainsKey(itemType))
            {
                itemCounts[itemType] = 0;
            }
        }
    }

    void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    public int GetItemCount(ItemType itemType)
    {
        if (!itemCounts.ContainsKey(itemType))
        {
            itemCounts[itemType] = 0;
        }

        return itemCounts[itemType];
    }

    public void AddItem(ItemType itemType, int amount)
    {
        if (!itemCounts.ContainsKey(itemType))
        {
            itemCounts[itemType] = 0;
        }

        itemCounts[itemType] += amount;
        NotifyInventoryChanged();

        Debug.Log("Added " + amount + " " + ItemDatabase.GetDisplayName(itemType));
    }

    public bool CanAfford(ItemCost[] costs)
    {
        for (int i = 0; i < costs.Length; i++)
        {
            if (GetItemCount(costs[i].itemType) < costs[i].amount)
            {
                return false;
            }
        }

        return true;
    }

    public bool SpendItems(ItemCost[] costs)
    {
        if (!CanAfford(costs))
        {
            return false;
        }

        for (int i = 0; i < costs.Length; i++)
        {
            itemCounts[costs[i].itemType] -= costs[i].amount;
        }

        NotifyInventoryChanged();
        return true;
    }

    public bool TryRemoveItem(ItemType itemType, int amount)
    {
        if (GetItemCount(itemType) < amount)
        {
            return false;
        }

        itemCounts[itemType] -= amount;
        NotifyInventoryChanged();
        return true;
    }
}