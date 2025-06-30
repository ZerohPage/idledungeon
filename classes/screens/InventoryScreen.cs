using Raylib_cs;
using System.Numerics;
using RaylibGame.Classes.Gui;

namespace RaylibGame.Classes;

/// <summary>
/// Screen for displaying and managing the player's inventory
/// </summary>
public class InventoryScreen : Screen
{
    private InventoryManager? _inventory;
    private int _selectedSlot = 0;
    private Button _closeButton;
    private Button _useButton;
    private Button _dropButton;
    
    // Layout constants
    private const int SLOTS_PER_ROW = 5;
    private const int SLOT_SIZE = 60;
    private const int SLOT_SPACING = 10;
    private const int INVENTORY_START_X = 100;
    private const int INVENTORY_START_Y = 150;

    public InventoryScreen(GameManager gameManager) : base(gameManager)
    {
        var (screenWidth, screenHeight) = GetScreenSize();
        
        // Create buttons
        _closeButton = new Button(gameManager, 
            new Vector2(screenWidth - 120, 20), 
            new Vector2(100, 40), 
            "Close");
        _closeButton.OnClick += () => GameManager.SetGameState(GameState.Playing);
        
        _useButton = new Button(gameManager,
            new Vector2(INVENTORY_START_X, screenHeight - 100),
            new Vector2(80, 35),
            "Use");
        _useButton.OnClick += UseSelectedItem;
        
        _dropButton = new Button(gameManager,
            new Vector2(INVENTORY_START_X + 90, screenHeight - 100),
            new Vector2(80, 35),
            "Drop");
        _dropButton.OnClick += DropSelectedItem;
    }

    /// <summary>
    /// Sets the inventory to display
    /// </summary>
    /// <param name="inventory">The inventory manager to display</param>
    public void SetInventory(InventoryManager inventory)
    {
        _inventory = inventory;
        _selectedSlot = 0; // Reset selection when inventory changes
    }

    public override void Update(float deltaTime)
    {
        // Handle input for closing inventory
        if (Raylib.IsKeyPressed(KeyboardKey.I) || Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            GameManager.SetGameState(GameState.Playing);
            return;
        }

        // Handle slot navigation
        HandleSlotNavigation();

        // Update buttons
        _closeButton.Update(deltaTime);
        _useButton.Update(deltaTime);
        _dropButton.Update(deltaTime);

        // Update button states based on selection
        UpdateButtonStates();
    }

    public override void Draw()
    {
        var (screenWidth, screenHeight) = GetScreenSize();
        
        // Draw semi-transparent background
        Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, new Color(0, 0, 0, 180));

        // Draw inventory panel background
        var panelRect = new Rectangle(50, 100, screenWidth - 100, screenHeight - 200);
        Raylib.DrawRectangleRec(panelRect, Color.DarkGray);
        Raylib.DrawRectangleLinesEx(panelRect, 3, Color.White);

        // Draw title
        DrawCenteredText("INVENTORY", screenHeight / 2 - 200, 32, Color.White, FontType.Title);

        // Draw inventory slots
        DrawInventorySlots();

        // Draw item details for selected slot
        DrawItemDetails();

        // Draw buttons
        _closeButton.Draw();
        _useButton.Draw();
        _dropButton.Draw();

        // Draw instructions
        DrawInstructions();
    }

    private void HandleSlotNavigation()
    {
        if (_inventory == null) return;

        int maxSlots = Math.Max(_inventory.UsedSlots - 1, 0);
        
        if (Raylib.IsKeyPressed(KeyboardKey.Left))
        {
            _selectedSlot = Math.Max(0, _selectedSlot - 1);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.Right))
        {
            _selectedSlot = Math.Min(maxSlots, _selectedSlot + 1);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.Up))
        {
            _selectedSlot = Math.Max(0, _selectedSlot - SLOTS_PER_ROW);
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.Down))
        {
            _selectedSlot = Math.Min(maxSlots, _selectedSlot + SLOTS_PER_ROW);
        }

        // Handle mouse selection
        Vector2 mousePos = Raylib.GetMousePosition();
        for (int i = 0; i < (_inventory?.UsedSlots ?? 0); i++)
        {
            var slotRect = GetSlotRectangle(i);
            if (Raylib.CheckCollisionPointRec(mousePos, slotRect))
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    _selectedSlot = i;
                }
            }
        }

        // Handle item usage with Enter or Space
        if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            UseSelectedItem();
        }
    }

    private void DrawInventorySlots()
    {
        if (_inventory == null) return;

        // Draw inventory capacity info
        string capacityText = $"Slots: {_inventory.UsedSlots}/{_inventory.MaxSlots}";
        DrawCenteredText(capacityText, INVENTORY_START_Y - 30, 16, Color.LightGray, FontType.UI);

        // Draw individual slots
        for (int i = 0; i < _inventory.MaxSlots; i++)
        {
            var slotRect = GetSlotRectangle(i);
            var item = i < _inventory.UsedSlots ? _inventory.GetItemAt(i) : null;

            // Draw slot background
            Color slotColor = i == _selectedSlot ? Color.Yellow : 
                             item != null ? Color.Gray : Color.DarkGray;
            Raylib.DrawRectangleRec(slotRect, slotColor);

            // Draw slot border
            Color borderColor = i == _selectedSlot ? Color.Orange : Color.Black;
            Raylib.DrawRectangleLinesEx(slotRect, i == _selectedSlot ? 3 : 2, borderColor);

            // Draw item if present
            if (item != null)
            {
                // Draw item representation (colored square based on rarity)
                var itemRect = new Rectangle(slotRect.X + 10, slotRect.Y + 10, 
                                           slotRect.Width - 20, slotRect.Height - 30);
                Raylib.DrawRectangleRec(itemRect, item.RarityColor);
                Raylib.DrawRectangleLinesEx(itemRect, 1, Color.Black);

                // Draw stack count if stackable
                if (item.MaxStackSize > 1 && item.StackCount > 1)
                {
                    string stackText = item.StackCount.ToString();
                    FontManager.DrawText(stackText, (int)(slotRect.X + slotRect.Width - 15), 
                                                  (int)(slotRect.Y + slotRect.Height - 15), 
                            12, Color.White, FontType.UI);
                }
            }

            // Draw slot number
            string slotNumber = (i + 1).ToString();
            FontManager.DrawText(slotNumber, (int)(slotRect.X + 2), (int)(slotRect.Y + 2), 10, Color.White, FontType.UI);
        }
    }

    private void DrawItemDetails()
    {
        if (_inventory == null) return;

        var selectedItem = _inventory.GetItemAt(_selectedSlot);
        if (selectedItem == null) return;

        var (screenWidth, screenHeight) = GetScreenSize();
        int detailsX = INVENTORY_START_X + (SLOTS_PER_ROW * (SLOT_SIZE + SLOT_SPACING)) + 50;
        int detailsY = INVENTORY_START_Y;

        // Draw details background
        var detailsRect = new Rectangle(detailsX - 10, detailsY - 10, 300, 200);
        Raylib.DrawRectangleRec(detailsRect, Color.Black);
        Raylib.DrawRectangleLinesEx(detailsRect, 2, Color.White);

        // Draw item details
        string[] lines = selectedItem.GetTooltipText().Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            Color textColor = i == 0 ? selectedItem.RarityColor : Color.White;
            FontManager.DrawText(lines[i], detailsX, detailsY + i * 20, 14, textColor, FontType.UI);
        }
    }

    private void DrawInstructions()
    {
        var (screenWidth, screenHeight) = GetScreenSize();
        int instructionY = screenHeight - 60;

        string[] instructions = {
            "Arrow Keys: Navigate | Enter/Space: Use Item | I/ESC: Close",
            "Mouse: Click to select | Buttons: Use/Drop items"
        };

        for (int i = 0; i < instructions.Length; i++)
        {
            DrawCenteredText(instructions[i], instructionY + i * 20, 12, Color.LightGray, FontType.UI);
        }
    }

    private Rectangle GetSlotRectangle(int slotIndex)
    {
        int row = slotIndex / SLOTS_PER_ROW;
        int col = slotIndex % SLOTS_PER_ROW;

        return new Rectangle(
            INVENTORY_START_X + col * (SLOT_SIZE + SLOT_SPACING),
            INVENTORY_START_Y + row * (SLOT_SIZE + SLOT_SPACING),
            SLOT_SIZE,
            SLOT_SIZE
        );
    }

    private void UpdateButtonStates()
    {
        if (_inventory == null) return;

        var selectedItem = _inventory.GetItemAt(_selectedSlot);
        
        // Enable/disable buttons based on whether an item is selected
        _useButton.Enabled = selectedItem != null;
        _dropButton.Enabled = selectedItem != null;
    }

    private void UseSelectedItem()
    {
        if (_inventory == null || GameManager.CurrentPlayer == null) return;

        if (_inventory.UseItem(_selectedSlot, GameManager.CurrentPlayer))
        {
            // Item was used successfully
            // Update selection if we're now beyond the inventory size
            if (_selectedSlot >= _inventory.UsedSlots && _selectedSlot > 0)
            {
                _selectedSlot = _inventory.UsedSlots - 1;
            }
        }
    }

    private void DropSelectedItem()
    {
        if (_inventory == null || GameManager.CurrentPlayer == null || GameManager.CurrentDungeon == null) 
            return;

        var selectedItem = _inventory.GetItemAt(_selectedSlot);
        if (selectedItem == null) return;

        // Remove item from inventory
        _inventory.RemoveItemAt(_selectedSlot);

        // Drop item at player's position
        selectedItem.OnDrop(GameManager.CurrentPlayer.Position);
        GameManager.CurrentDungeon.AddItem(selectedItem);

        // Update selection if needed
        if (_selectedSlot >= _inventory.UsedSlots && _selectedSlot > 0)
        {
            _selectedSlot = _inventory.UsedSlots - 1;
        }
    }
}
