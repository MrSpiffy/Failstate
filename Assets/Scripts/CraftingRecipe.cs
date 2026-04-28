[System.Serializable]
public class CraftingRecipe
{
    public ItemType outputItem;
    public int outputAmount;
    public ItemCost[] costs;

    public CraftingRecipe(ItemType outputItem, int outputAmount, ItemCost[] costs)
    {
        this.outputItem = outputItem;
        this.outputAmount = outputAmount;
        this.costs = costs;
    }

    public string GetCostText()
    {
        if (costs == null || costs.Length == 0)
        {
            return "Free";
        }

        string result = "";

        for (int i = 0; i < costs.Length; i++)
        {
            result += costs[i].amount + " " + ItemDatabase.GetDisplayName(costs[i].itemType);

            if (i < costs.Length - 1)
            {
                result += " + ";
            }
        }

        return result;
    }
}