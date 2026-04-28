[System.Serializable]
public struct ItemCost
{
    public ItemType itemType;
    public int amount;

    public ItemCost(ItemType itemType, int amount)
    {
        this.itemType = itemType;
        this.amount = amount;
    }
}