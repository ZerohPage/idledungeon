using System.Numerics;

namespace RaylibGame.Classes.Items;

/// <summary>
/// Old boots that can be equipped by the player
/// </summary>
public class OldBoots : Item
{
    /// <summary>
    /// Initializes a new instance of the OldBoots class
    /// </summary>
    public OldBoots() 
        : base(2, "Old Boots", "Worn leather boots that have seen better days", ItemRarity.Common, 5)
    {
        MaxStackSize = 1; // Equipment items don't stack
        Size = new Vector2(14, 18); // Boot-shaped size
    }

    /// <summary>
    /// Called when the old boots are picked up by a player
    /// </summary>
    /// <param name="player">The player picking up the boots</param>
    /// <returns>True if the boots were successfully picked up and added to inventory</returns>
    public override bool OnPickup(Player player)
    {
        if (!CanPickup) return false;

        // Try to add to player's inventory
        if (player.Inventory.TryAddItem(this))
        {
            // Mark as invisible since they've been picked up
            IsVisible = false;
            return true;
        }
        
        return false; // Inventory full
    }

    /// <summary>
    /// Called when the old boots are used from inventory
    /// </summary>
    /// <param name="player">The player using the boots</param>
    /// <returns>True if the boots were successfully used (equipped)</returns>
    public override bool OnUse(Player player)
    {
        // For now, boots can't be used directly from inventory
        // This will be implemented later when we add equipment system
        return false;
    }

    /// <summary>
    /// Creates a copy of these old boots
    /// </summary>
    /// <returns>A new OldBoots instance with the same properties</returns>
    public override Item Clone()
    {
        var clone = new OldBoots()
        {
            Position = Position,
            IsVisible = IsVisible,
            StackCount = StackCount
        };
        return clone;
    }

    /// <summary>
    /// Gets detailed tooltip text for the old boots
    /// </summary>
    public override string GetTooltipText()
    {
        var baseText = base.GetTooltipText();
        return $"{baseText}\nType: Equipment (Boots)\nWill be useful later...";
    }
}
