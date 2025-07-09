using Raylib_cs;
using System.Numerics;
using RaylibGame.Classes.Items;

namespace RaylibGame.Classes;

/// <summary>
/// Manages debug visualization and information display
/// </summary>
public class DebugManager
{
    private bool _showDebugInfo = false;
    private GameManager _gameManager;

    public bool IsDebugEnabled => _showDebugInfo;

    public DebugManager(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    /// <summary>
    /// Toggles debug information display
    /// </summary>
    public void ToggleDebug()
    {
        _showDebugInfo = !_showDebugInfo;
    }

    /// <summary>
    /// Draws debug information overlay
    /// </summary>
    public void DrawDebugInfo()
    {
        if (!_showDebugInfo || _gameManager.CurrentDungeon == null) return;

        // Begin camera mode for world items
        CameraManager.BeginMode();
        
        // Draw all items with colored circles (rendered in world space)
        foreach (var item in _gameManager.CurrentDungeon.Items)
        {
            if (item.IsVisible)
            {
                // Draw a colored circle based on item type
                Color itemColor = item switch
                {
                    HealingPotion => Color.Red,
                    OldBoots => Color.Brown,
                    _ => Color.Yellow
                };
                
                Raylib.DrawCircleV(item.Position, 4.0f, itemColor);
                // Draw outline for better visibility
                Raylib.DrawCircleLinesV(item.Position, 4.0f, Color.White);
            }
        }

        // Draw enemy positions with purple X marks (rendered in world space)
        foreach (var enemy in _gameManager.Enemies)
        {
            if (enemy.IsAlive)
            {
                // Draw an X to mark enemy position
                Vector2 pos = enemy.Position;
                float size = 6.0f;
                
                // Draw X lines
                Raylib.DrawLineV(new Vector2(pos.X - size, pos.Y - size), 
                                new Vector2(pos.X + size, pos.Y + size), Color.Purple);
                Raylib.DrawLineV(new Vector2(pos.X + size, pos.Y - size), 
                                new Vector2(pos.X - size, pos.Y + size), Color.Purple);
            }
        }
        
        // End camera mode
        CameraManager.EndMode();

        // Show debug information and legend (rendered in screen space)
        DrawDebugText();
    }

    /// <summary>
    /// Draws debug text information
    /// </summary>
    private void DrawDebugText()
    {
        if (_gameManager.CurrentDungeon == null) return;

        // Show debug information in a more visible location (right side of screen)
        string debugInfo = $"DEBUG: {_gameManager.CurrentDungeon.Items.Count} items, {_gameManager.Enemies.Count} enemies";
        FontManager.DrawText(debugInfo, 400, 10, 16, Color.Yellow, FontType.UI);
        
        // Show legend
        FontManager.DrawText("Red=Potions, Brown=Boots, Purple X=Enemies", 400, 30, 14, Color.White, FontType.UI);
        
        // Show player info if available
        if (_gameManager.CurrentPlayer != null)
        {
            var player = _gameManager.CurrentPlayer;
            string playerInfo = $"Player: HP {player.Health}/{player.MaxHealth}, Inv {player.Inventory.UsedSlots}/{player.Inventory.MaxSlots}";
            FontManager.DrawText(playerInfo, 400, 50, 14, Color.SkyBlue, FontType.UI);
        }
    }
}
