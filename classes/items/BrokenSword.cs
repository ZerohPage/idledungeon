using Raylib_cs;
using System.Numerics;

namespace RaylibGame.Classes.Items;

/// <summary>
/// A damaged weapon that once belonged to a fallen warrior
/// </summary>
public class BrokenSword : Item
{
    public BrokenSword() : base(
        id: 3,
        name: "Broken Sword",
        description: "A rusty, chipped blade that has seen better days. Still sharp enough to be dangerous.",
        rarity: ItemRarity.Common,
        value: 15)
    {
        MaxStackSize = 1; // Weapons don't stack
        Size = new Vector2(18, 8); // Slightly different size for weapon
    }

    public override void Draw()
    {
        if (!IsVisible) return;

        // Draw the sword blade (gray for broken metal) - camera transformation handled by Raylib
        Vector2 bladeStart = new Vector2(Position.X - Size.X / 2, Position.Y - 2);
        Vector2 bladeEnd = new Vector2(Position.X + Size.X / 2 - 4, Position.Y - 2);
        Raylib.DrawLineEx(bladeStart, bladeEnd, 4.0f, Color.Gray);
        
        // Draw the hilt (brown for leather grip)
        Vector2 hiltPos = new Vector2(Position.X + Size.X / 2 - 4, Position.Y);
        Raylib.DrawRectangleV(new Vector2(hiltPos.X, hiltPos.Y - 3), new Vector2(4, 6), Color.Brown);
        
        // Draw some rust spots (dark red dots)
        Raylib.DrawCircleV(new Vector2(Position.X - 3, Position.Y - 1), 1.0f, Color.Maroon);
        Raylib.DrawCircleV(new Vector2(Position.X + 2, Position.Y), 1.0f, Color.Maroon);
        
        // Draw outline for visibility
        Raylib.DrawRectangleLinesEx(
            new Rectangle(Position.X - Size.X / 2, Position.Y - Size.Y / 2, Size.X, Size.Y),
            1,
            Color.Black
        );
    }

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

    public override bool OnUse(Player player)
    {
        // Broken sword could be used as a weak weapon or eventually repaired
        // For now, just return false (can't be used directly)
        return false;
    }

    public override Item Clone()
    {
        var clone = new BrokenSword();
        clone.Position = Position;
        clone.IsVisible = IsVisible;
        clone.StackCount = StackCount;
        return clone;
    }

    public override string GetTooltipText()
    {
        return base.GetTooltipText() + "\n\n\"Perhaps this could be repaired by a skilled blacksmith...\"";
    }
}
