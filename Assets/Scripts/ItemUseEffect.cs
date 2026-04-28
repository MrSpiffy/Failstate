public enum ItemUseEffectType
{
    None,
    RestorePlayerSystem
}

public struct ItemUseEffect
{
    public ItemUseEffectType effectType;
    public PlayerSystemType targetSystem;
    public float amount;

    public ItemUseEffect(ItemUseEffectType effectType, PlayerSystemType targetSystem, float amount)
    {
        this.effectType = effectType;
        this.targetSystem = targetSystem;
        this.amount = amount;
    }
}