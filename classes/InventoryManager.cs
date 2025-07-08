using System.Numerics;
using RaylibGame.Classes.Items;

namespace RaylibGame.Classes;

/// <summary>
/// Manages the player's inventory system
/// </summary>
public class InventoryManager
{
    private List<Item> _items;
    private int _maxSlots;

    /// <summary>
    /// Gets the list of items in the inventory (read-only)
    /// </summary>
    public IReadOnlyList<Item> Items => _items;

    /// <summary>
    /// Gets the maximum number of inventory slots
    /// </summary>
    public int MaxSlots => _maxSlots;

    /// <summary>
    /// Gets the current number of used slots
    /// </summary>
    public int UsedSlots => _items.Count;

    /// <summary>
    /// Gets the number of available slots
    /// </summary>
    public int AvailableSlots => _maxSlots - _items.Count;

    /// <summary>
    /// Checks if the inventory is full
    /// </summary>
    public bool IsFull => _items.Count >= _maxSlots;

    /// <summary>
    /// Initializes a new instance of the InventoryManager class
    /// </summary>
    /// <param name="maxSlots">Maximum number of inventory slots (default: 20)</param>
    public InventoryManager(int maxSlots = 20)
    {
        _maxSlots = maxSlots;
        _items = new List<Item>();
    }

    /// <summary>
    /// Attempts to add an item to the inventory
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <returns>True if the item was successfully added or stacked</returns>
    public bool TryAddItem(Item item)
    {
        if (item == null) return false;

        // Try to stack with existing items first
        foreach (var existingItem in _items)
        {
            if (existingItem.CanStackWith(item))
            {
                int remainingCount = existingItem.StackWith(item);
                
                // If all items were stacked successfully
                if (remainingCount == 0)
                {
                    return true;
                }
                
                // Update the item's stack count to the remaining amount
                item.StackCount = remainingCount;
            }
        }

        // If we couldn't stack everything, try to add as a new item
        if (!IsFull)
        {
            _items.Add(item);
            return true;
        }

        // Inventory is full and couldn't stack
        return false;
    }

    /// <summary>
    /// Removes an item from the inventory
    /// </summary>
    /// <param name="item">The item to remove</param>
    /// <returns>True if the item was successfully removed</returns>
    public bool RemoveItem(Item item)
    {
        return _items.Remove(item);
    }

    /// <summary>
    /// Removes an item at a specific slot index
    /// </summary>
    /// <param name="slotIndex">The slot index to remove from</param>
    /// <returns>The removed item, or null if index is invalid</returns>
    public Item? RemoveItemAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _items.Count)
            return null;

        var item = _items[slotIndex];
        _items.RemoveAt(slotIndex);
        return item;
    }

    /// <summary>
    /// Gets an item at a specific slot index
    /// </summary>
    /// <param name="slotIndex">The slot index</param>
    /// <returns>The item at the specified slot, or null if index is invalid</returns>
    public Item? GetItemAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _items.Count)
            return null;

        return _items[slotIndex];
    }

    /// <summary>
    /// Uses an item from the inventory
    /// </summary>
    /// <param name="slotIndex">The slot index of the item to use</param>
    /// <param name="player">The player using the item</param>
    /// <returns>True if the item was successfully used</returns>
    public bool UseItem(int slotIndex, Player player)
    {
        var item = GetItemAt(slotIndex);
        if (item == null) return false;

        bool wasUsed = item.OnUse(player);
        
        if (wasUsed)
        {
            // Reduce stack count or remove item
            if (item.StackCount > 1)
            {
                item.StackCount--;
            }
            else
            {
                RemoveItemAt(slotIndex);
            }
        }

        return wasUsed;
    }

    /// <summary>
    /// Finds all items of a specific type
    /// </summary>
    /// <typeparam name="T">The type of item to find</typeparam>
    /// <returns>List of items of the specified type</returns>
    public List<T> GetItemsOfType<T>() where T : Item
    {
        return _items.OfType<T>().ToList();
    }

    /// <summary>
    /// Finds the first item of a specific type and returns its slot index
    /// </summary>
    /// <typeparam name="T">The type of item to find</typeparam>
    /// <returns>The slot index of the first item of the specified type, or -1 if not found</returns>
    public int FindFirstItemOfType<T>() where T : Item
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] is T)
            {
                return i;
            }
        }
        return -1; // Not found
    }

    /// <summary>
    /// Gets the total count of a specific item type
    /// </summary>
    /// <typeparam name="T">The type of item to count</typeparam>
    /// <returns>Total count including stacks</returns>
    public int GetItemCount<T>() where T : Item
    {
        return _items.OfType<T>().Sum(item => item.StackCount);
    }

    /// <summary>
    /// Clears all items from the inventory
    /// </summary>
    public void Clear()
    {
        _items.Clear();
    }

    /// <summary>
    /// Checks if the inventory contains a specific item
    /// </summary>
    /// <param name="item">The item to check for</param>
    /// <returns>True if the inventory contains the item</returns>
    public bool Contains(Item item)
    {
        return _items.Contains(item);
    }

    /// <summary>
    /// Checks if the inventory has at least the specified count of an item type
    /// </summary>
    /// <typeparam name="T">The type of item to check</typeparam>
    /// <param name="count">The minimum count required</param>
    /// <returns>True if the inventory has enough of the specified item type</returns>
    public bool HasItemCount<T>(int count) where T : Item
    {
        return GetItemCount<T>() >= count;
    }

    /// <summary>
    /// Gets debug information about the inventory
    /// </summary>
    /// <returns>String containing inventory information</returns>
    public string GetDebugInfo()
    {
        return $"Inventory: {UsedSlots}/{MaxSlots} slots used\n" +
               $"Items: {string.Join(", ", _items.Select(item => item.GetDisplayText()))}";
    }
}
