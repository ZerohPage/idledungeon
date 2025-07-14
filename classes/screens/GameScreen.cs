using Raylib_cs;
using System.Numerics;
using RaylibGame.Classes.Gui;
using RaylibGame.Classes.Items;

namespace RaylibGame.Classes;

/// <summary>
/// Screen for the main gameplay experience
/// </summary>
public class GameScreen : Screen
{
    private Button _inventoryButton;
    private RaycastRenderer _raycastRenderer;
    private float _autoHealCooldown = 0f;
    private const float AUTO_HEAL_COOLDOWN_TIME = 2.0f; // 2 seconds between auto-heals

    public GameScreen(GameManager gameManager) : base(gameManager)
    {
        // Create inventory button in the top-right corner
        var (screenWidth, screenHeight) = GetScreenSize();
        _inventoryButton = new Button(gameManager,
            new Vector2(screenWidth - 120, 10),
            new Vector2(100, 30),
            "Inventory");
        _inventoryButton.OnClick += ToggleInventory;
        
        // Create raycast renderer for 3D view
        _raycastRenderer = new RaycastRenderer();
        if (gameManager.CurrentDungeon != null)
        {
            _raycastRenderer.SetDungeon(gameManager.CurrentDungeon);
        }
    }

    public override void Update(float deltaTime)
    {
        // Handle inventory toggle
        if (InputManager.IsInventoryTogglePressed)
        {
            ToggleInventory();
        }

        // Handle debug info toggle
        if (InputManager.IsDebugTogglePressed)
        {
            GameManager.Debug.ToggleDebug();
        }
        
        // Handle raycast 3D view toggle
        if (InputManager.IsRaycastTogglePressed)
        {
            _raycastRenderer.ToggleVisibility();
        }

        // Update inventory button
        _inventoryButton.Update(deltaTime);

        // Update auto-heal cooldown
        if (_autoHealCooldown > 0f)
        {
            _autoHealCooldown -= deltaTime;
        }

        // Check for pause
        if (InputManager.IsPausePressed)
        {
            GameManager.SetGameState(GameState.Paused);
            return;
        }

        // Update combat system
        GameManager.Combat.Update(deltaTime);
        
        // Update camera to follow player
        if (GameManager.CurrentPlayer != null)
        {
            CameraManager.SetTarget(GameManager.CurrentPlayer.Position);
            CameraManager.Update(deltaTime);
            
            // Update raycast renderer with player position and direction
            if (GameManager.CurrentDungeon != null)
            {
                // Make sure the raycast renderer has the current dungeon
                _raycastRenderer.SetDungeon(GameManager.CurrentDungeon);
                
                // Get player direction (from auto explorer or default)
                Vector2 playerDirection = Vector2.UnitX; // Default facing right
                if (GameManager.CurrentPlayer.IsAutoExploring)
                {
                    // Get the current exploration direction or use a default
                    playerDirection = GetPlayerViewDirection();
                }
                
                // Render the 3D view
                _raycastRenderer.Render(GameManager.CurrentPlayer.GridPosition, playerDirection);
            }
        }
        
        // Update raycast renderer fog animation
        _raycastRenderer.UpdateFogAnimation(deltaTime);
        
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
            
            // Auto-use healing potion if health is below 25%
            CheckAutoHeal();
            
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
        
        // Begin camera mode for world rendering
        CameraManager.BeginMode();
        
        // Draw world objects (no offset needed - camera handles transformation)
        GameManager.CurrentDungeon?.Draw();
        GameManager.DrawEnemies();
        GameManager.CurrentPlayer?.Draw();
        GameManager.FloatingNumbers.Draw();
        
        // End camera mode
        CameraManager.EndMode();
        
        // Draw UI (rendered in screen space, outside camera mode)
        DrawGameUI();
        
        // Draw combat UI if in combat (screen space)
        GameManager.Combat.Draw();

        // Draw debug info if enabled
        GameManager.Debug.DrawDebugInfo();

        // Draw inventory button (screen space)
        _inventoryButton.Draw();
        
        // Draw 3D raycast view (screen space overlay)
        _raycastRenderer.Draw();
    }

    private void DrawGameUI()
    {
        FontManager.DrawText("Dungeon Game", 10, 10, 20, Color.White, FontType.UI);
        
        // Draw player health bar
        if (GameManager.CurrentPlayer != null)
        {
            GameManager.CurrentPlayer.DrawHealthBar(new Vector2(10, 80));
        }
        
        Raylib.DrawFPS(10, 110);
    }

    private void ToggleInventory()
    {
        // Set the player's inventory in the inventory screen before switching
        if (GameManager.CurrentPlayer != null)
        {
            // We'll need to access the inventory screen through GameManager
            GameManager.SetGameState(GameState.Inventory);
        }
    }

    private void CheckAutoHeal()
    {
        if (GameManager.CurrentPlayer == null) return;

        var player = GameManager.CurrentPlayer;
        
        // Check if health is below 25%
        float healthPercentage = (float)player.Health / player.MaxHealth;
        if (healthPercentage < 0.25f)
        {
            // Find the first healing potion in inventory
            var inventory = player.Inventory;
            int potionSlot = inventory.FindFirstItemOfType<HealingPotion>();
            
            if (potionSlot >= 0)
            {
                var healingPotion = inventory.GetItemAt(potionSlot) as HealingPotion;
                // Use the healing potion
                if (inventory.UseItem(potionSlot, player))
                {
                    // Add floating heal number for feedback
                    GameManager.FloatingNumbers.AddHealNumber(player.Position, healingPotion!.HealingAmount);
                }
            }
        }
    }

    /// <summary>
    /// Gets the direction the player is currently facing/moving
    /// </summary>
    private Vector2 GetPlayerViewDirection()
    {
        if (GameManager.CurrentPlayer == null) return Vector2.UnitX;
        
        // Get direction from auto explorer if available
        Vector2 direction = GameManager.CurrentPlayer.CurrentDirection;
        
        // If no direction (standing still), default to facing right
        if (direction == Vector2.Zero)
            return Vector2.UnitX;
            
        return direction;
    }

    /// <summary>
    /// Cleanup resources
    /// </summary>
    public void Cleanup()
    {
        _raycastRenderer?.Dispose();
    }
}
