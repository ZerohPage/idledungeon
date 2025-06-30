using System.Numerics;

namespace RaylibGame.Classes.Items;

/// <summary>
/// A healing potion that restores health to the player when picked up
/// </summary>
public class HealingPotion : Item
{
    /// <summary>
    /// Amount of health this potion restores
    /// </summary>
    public int HealingAmount { get; private set; }

    /// <summary>
    /// Initializes a new instance of the HealingPotion class
    /// </summary>
    /// <param name="healingAmount">Amount of health to restore (default: 10)</param>
    public HealingPotion(int healingAmount = 10) 
        : base(1, "Healing Potion", $"Restores {healingAmount} health when picked up", ItemRarity.Common, 15)
    {
        HealingAmount = healingAmount;
        MaxStackSize = 5; // Potions can stack up to 5
        Size = new Vector2(12, 16); // Smaller size for potion bottle
    }

    /// <summary>
    /// Called when the healing potion is picked up by a player
    /// </summary>
    /// <param name="player">The player picking up the potion</param>
    /// <returns>True if the potion was successfully picked up and added to inventory</returns>
    public override bool OnPickup(Player player)
    {
        if (!CanPickup) return false;

        // Try to add to player's inventory
        if (player.Inventory.TryAddItem(this))
        {
            // Mark as invisible since it's been picked up
            IsVisible = false;
            return true;
        }
        
        return false; // Inventory full
    }

    /// <summary>
    /// Called when the healing potion is used from inventory
    /// </summary>
    /// <param name="player">The player using the potion</param>
    /// <returns>True if the potion was successfully used</returns>
    public override bool OnUse(Player player)
    {
        // Only use if player needs healing
        if (player.Health >= player.MaxHealth) return false;

        // Heal the player
        player.Heal(HealingAmount);
        
        return true; // Potion was consumed
    }

    /// <summary>
    /// Creates a copy of this healing potion
    /// </summary>
    /// <returns>A new HealingPotion instance with the same properties</returns>
    public override Item Clone()
    {
        var clone = new HealingPotion(HealingAmount)
        {
            Position = Position,
            IsVisible = IsVisible,
            StackCount = StackCount
        };
        return clone;
    }

    /// <summary>
    /// Gets detailed tooltip text for the healing potion
    /// </summary>
    public override string GetTooltipText()
    {
        var baseText = base.GetTooltipText();
        return $"{baseText}\nHealing: +{HealingAmount} HP\nUse from inventory to heal";
    }
}
