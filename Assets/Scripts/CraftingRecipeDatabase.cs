public static class CraftingRecipeDatabase
{
    public static CraftingRecipe[] GetAllRecipes()
    {
        return new CraftingRecipe[]
        {
            new CraftingRecipe(
                ItemType.RepairKit,
                1,
                new ItemCost[]
                {
                    new ItemCost(ItemType.MetalScrap, 2),
                    new ItemCost(ItemType.Wiring, 1)
                }
            ),

            new CraftingRecipe(
                ItemType.MobilityPatch,
                1,
                new ItemCost[]
                {
                    new ItemCost(ItemType.Wiring, 2)
                }
            ),

            new CraftingRecipe(
                ItemType.SensorPatch,
                1,
                new ItemCost[]
                {
                    new ItemCost(ItemType.CoreFragment, 2)
                }
            )
        };
    }
}