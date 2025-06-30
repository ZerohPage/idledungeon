using Raylib_cs;
using System.Numerics;

namespace RaylibGame.Classes;

/// <summary>
/// Screen for the main gameplay experience
/// </summary>
public class GameScreen : Screen
{
    private InventoryScreen _inventoryScreen;
    private bool _showInventory = false;

    public GameScreen(GameManager gameManager) : base(gameManager)
    {
        _inventoryScreen = new InventoryScreen(gameManager);
    }

    public override void Update(float deltaTime)
    {
        // Handle inventory toggle
        if (Raylib.IsKeyPressed(KeyboardKey.I))
        {
            _showInventory = !_showInventory;
            if (_showInventory && GameManager.CurrentPlayer != null)
            {
                // Set the player's inventory in the inventory screen
                _inventoryScreen.SetInventory(GameManager.CurrentPlayer.Inventory);
            }
        }

        // If showing inventory, update inventory screen instead of game logic
        if (_showInventory)
        {
            _inventoryScreen.Update(deltaTime);
            return;
        }

        // Check for pause
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            GameManager.SetGameState(GameState.Paused);
            return;
        }

        // Update combat system
        GameManager.Combat.Update(deltaTime);
        
        // Update floating numbers
        GameManager.FloatingNumbers.Update(deltaTime);
        
        // Check combat results
        if (GameManager.Combat.State == CombatState.PlayerLoses)
        {
            GameManager.SetGameState(GameState.GameOver);
            GameManager.Combat.EndCombat();
            return;
        }
        else if (GameManager.Combat.State == CombatState.PlayerWins)
        {
            // Remove defeated enemy
            if (GameManager.Combat.CurrentEnemy != null)
            {
                GameManager.RemoveEnemy(GameManager.Combat.CurrentEnemy);
            }
            GameManager.Combat.EndCombat();
        }
        
        // Update player
        if (GameManager.CurrentPlayer != null)
        {
            GameManager.CurrentPlayer.Update(deltaTime);
            
            // Update explored areas based on player position
            if (GameManager.CurrentDungeon != null)
            {
                GameManager.CurrentDungeon.UpdateExploredAreas(GameManager.CurrentPlayer.Position);
            }
            
            // Check if player is dead
            if (!GameManager.CurrentPlayer.IsAlive())
            {
                GameManager.SetGameState(GameState.GameOver);
                return;
            }
        }
        
        // Update enemies
        GameManager.UpdateEnemies(deltaTime);

        // Update dungeon items
        if (GameManager.CurrentDungeon != null)
        {
            GameManager.CurrentDungeon.UpdateItems(deltaTime);
        }

        // Check for item pickups (only if not in combat)
        if (!GameManager.Combat.IsInCombat && GameManager.CurrentPlayer != null && GameManager.CurrentDungeon != null)
        {
            GameManager.CheckForItemPickups();
        }
        
        // Check for combat encounters (only if not already in combat)
        if (!GameManager.Combat.IsInCombat && GameManager.CurrentPlayer != null)
        {
            GameManager.CheckForCombatEncounters();
        }
    }

    public override void Draw()
    {
        Raylib.ClearBackground(Color.Black);
        
        // Draw the dungeon if it exists
        GameManager.CurrentDungeon?.Draw();
        
        // Draw enemies
        GameManager.DrawEnemies();
        
        // Draw the player if it exists
        GameManager.CurrentPlayer?.Draw();
        
        // Draw combat UI if in combat
        GameManager.Combat.Draw();
        
        // Draw floating damage numbers
        GameManager.FloatingNumbers.Draw();
        
        // Draw UI
        DrawGameUI();

        // Draw inventory screen on top if showing
        if (_showInventory)
        {
            _inventoryScreen.Draw();
        }
    }

    private void DrawGameUI()
    {
        FontManager.DrawText("Dungeon Game", 10, 10, 20, Color.White, FontType.UI);
        FontManager.DrawText("Use WASD to move, ESC to pause, I for inventory", 10, 35, 16, Color.LightGray, FontType.UI);
        
        // Draw player health bar
        if (GameManager.CurrentPlayer != null)
        {
            GameManager.CurrentPlayer.DrawHealthBar(new Vector2(10, 70));
        }
        
        Raylib.DrawFPS(10, 100);
    }
}
