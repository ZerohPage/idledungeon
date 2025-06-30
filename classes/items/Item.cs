using Raylib_cs;
using System.Numerics;

namespace RaylibGame.Classes.Items;

/// <summary>
/// Item rarity levels affecting appearance and value
/// </summary>
public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

/// <summary>
/// Base class for all items in the game
/// </summary>
public abstract class Item
{
    /// <summary>
    /// Unique identifier for the item
    /// </summary>
    public int Id { get; protected set; }

    /// <summary>
    /// Display name of the item
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Detailed description of the item
    /// </summary>
    public string Description { get; protected set; }

    /// <summary>
    /// Item's rarity level
    /// </summary>
    public ItemRarity Rarity { get; protected set; }

    /// <summary>
    /// Base value of the item for trading/selling
    /// </summary>
    public int Value { get; protected set; }

    /// <summary>
    /// Current position of the item in the world
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Whether the item is currently visible and can be interacted with
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Whether the item can be picked up by the player
    /// </summary>
    public bool CanPickup { get; protected set; } = true;

    /// <summary>
    /// Maximum stack size for this item (1 means not stackable)
    /// </summary>
    public int MaxStackSize { get; protected set; } = 1;

    /// <summary>
    /// Current stack count if stackable
    /// </summary>
    public int StackCount { get; set; } = 1;

    /// <summary>
    /// Color used to render the item based on rarity
    /// </summary>
    public Color RarityColor => Rarity switch
    {
        ItemRarity.Common => Color.White,
        ItemRarity.Uncommon => Color.Green,
        ItemRarity.Rare => Color.Blue,
        ItemRarity.Epic => Color.Purple,
        ItemRarity.Legendary => Color.Gold,
        _ => Color.Gray
    };

    /// <summary>
    /// Size of the item for rendering and collision
    /// </summary>
    public virtual Vector2 Size { get; protected set; } = new Vector2(16, 16);

    /// <summary>
    /// Gets the bounding rectangle for collision detection
    /// </summary>
    public Rectangle Bounds => new Rectangle(Position.X - Size.X / 2, Position.Y - Size.Y / 2, Size.X, Size.Y);

    /// <summary>
    /// Initializes a new instance of the Item class
    /// </summary>
    /// <param name="id">Unique identifier</param>
    /// <param name="name">Display name</param>
    /// <param name="description">Item description</param>
    /// <param name="rarity">Item rarity</param>
    /// <param name="value">Base value</param>
    protected Item(int id, string name, string description, ItemRarity rarity = ItemRarity.Common, int value = 0)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Rarity = rarity;
        Value = value;
    }

    /// <summary>
    /// Updates the item's state (e.g., animations, effects)
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update</param>
    public virtual void Update(float deltaTime)
    {
        // Base implementation - can be overridden for animated items
    }

    /// <summary>
    /// Renders the item to the screen
    /// </summary>
    /// <param name="cameraOffset">Camera offset for world positioning</param>
    public virtual void Draw(Vector2 cameraOffset = default)
    {
        if (!IsVisible) return;

        Vector2 screenPosition = Position + cameraOffset;
        
        // Draw item as a colored rectangle by default
        // Derived classes should override this for custom rendering
        Raylib.DrawRectangleV(
            new Vector2(screenPosition.X - Size.X / 2, screenPosition.Y - Size.Y / 2),
            Size,
            RarityColor
        );

        // Draw outline
        Raylib.DrawRectangleLinesEx(
            new Rectangle(screenPosition.X - Size.X / 2, screenPosition.Y - Size.Y / 2, Size.X, Size.Y),
            1,
            Color.Black
        );
    }

    /// <summary>
    /// Called when the item is picked up by a player
    /// </summary>
    /// <param name="player">The player picking up the item</param>
    /// <returns>True if the item was successfully picked up</returns>
    public virtual bool OnPickup(Player player)
    {
        if (!CanPickup) return false;

        // Base implementation - derived classes should override for specific behavior
        IsVisible = false;
        return true;
    }

    /// <summary>
    /// Called when the item is used
    /// </summary>
    /// <param name="player">The player using the item</param>
    /// <returns>True if the item was successfully used</returns>
    public virtual bool OnUse(Player player)
    {
        // Base implementation - derived classes should override for specific behavior
        return false;
    }

    /// <summary>
    /// Called when the item is dropped
    /// </summary>
    /// <param name="position">Position where the item is dropped</param>
    public virtual void OnDrop(Vector2 position)
    {
        Position = position;
        IsVisible = true;
    }

    /// <summary>
    /// Checks if this item can stack with another item
    /// </summary>
    /// <param name="other">The other item to check</param>
    /// <returns>True if items can be stacked together</returns>
    public virtual bool CanStackWith(Item other)
    {
        return other != null &&
               other.GetType() == GetType() &&
               other.Id == Id &&
               MaxStackSize > 1 &&
               StackCount < MaxStackSize &&
               other.StackCount < other.MaxStackSize;
    }

    /// <summary>
    /// Attempts to stack this item with another
    /// </summary>
    /// <param name="other">The item to stack with</param>
    /// <returns>The remaining count that couldn't be stacked</returns>
    public virtual int StackWith(Item other)
    {
        if (!CanStackWith(other)) return other.StackCount;

        int totalCount = StackCount + other.StackCount;
        int maxCanStack = MaxStackSize;
        
        if (totalCount <= maxCanStack)
        {
            StackCount = totalCount;
            return 0; // All stacked successfully
        }
        else
        {
            StackCount = maxCanStack;
            return totalCount - maxCanStack; // Return overflow
        }
    }

    /// <summary>
    /// Creates a copy of this item
    /// </summary>
    /// <returns>A new item instance with the same properties</returns>
    public abstract Item Clone();

    /// <summary>
    /// Gets the display text for the item (name with stack count if applicable)
    /// </summary>
    public virtual string GetDisplayText()
    {
        if (MaxStackSize > 1 && StackCount > 1)
        {
            return $"{Name} ({StackCount})";
        }
        return Name;
    }

    /// <summary>
    /// Gets detailed tooltip text for the item
    /// </summary>
    public virtual string GetTooltipText()
    {
        var text = $"{Name}\n{Description}\nRarity: {Rarity}";
        if (Value > 0)
        {
            text += $"\nValue: {Value}";
        }
        return text;
    }
}
