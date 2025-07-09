using Raylib_cs;
using System.Numerics;
using RaylibGame.Classes.Items;

namespace RaylibGame.Classes;

public enum GameState
{
    Intro,
    Playing,
    Inventory,
    Paused,
    GameOver
}

public class GameManager
{
    private GameState _currentState;
    private Dungeon? _dungeon;
    private Player? _player;
    private List<Enemy> _enemies;
    private Random _random;
    private Combat _combat;
    private FloatingNumberManager _floatingNumbers;
    private IntroScreen _introScreen;
    private GameOverScreen _gameOverScreen;
    private GameScreen _gameScreen;
    private InventoryScreen _inventoryScreen;
    private DebugManager _debugManager;
    private CameraManager _cameraManager;
    
    public GameState CurrentState => _currentState;
    public Dungeon? CurrentDungeon => _dungeon;
    public Player? CurrentPlayer => _player;
    public FloatingNumberManager FloatingNumbers => _floatingNumbers;
    public Combat Combat => _combat;
    public IReadOnlyList<Enemy> Enemies => _enemies;
    public DebugManager Debug => _debugManager;
    public CameraManager Camera => _cameraManager;
    
    public GameManager()
    {
        _currentState = GameState.Intro;
        _enemies = new List<Enemy>();
        _random = new Random();
        _combat = new Combat();
        _floatingNumbers = new FloatingNumberManager();
        _introScreen = new IntroScreen(this);
        _gameOverScreen = new GameOverScreen(this);
        _gameScreen = new GameScreen(this);
        _inventoryScreen = new InventoryScreen(this);
        _debugManager = new DebugManager(this);
        _cameraManager = new CameraManager();
        
        // Connect floating numbers to combat system
        _combat.SetFloatingNumberManager(_floatingNumbers);
    }
    
    public void Initialize()
    {
        // Load fonts first
        FontManager.LoadFonts();
        
        // Don't start the game automatically - let IntroScreen handle the flow
    }
    
    public void StartNewGame()
    {
        // Create a new dungeon (doubled size for more exploration)
        _dungeon = new Dungeon(120, 80);
        
        // Create player and set position to dungeon entrance
        _player = new Player(Vector2.Zero);
        _player.SetDungeon(_dungeon);
        
        // Setup camera
        _cameraManager.SetDungeonBounds(_dungeon.Width * _dungeon.TileSize, _dungeon.Height * _dungeon.TileSize);
        _cameraManager.SetTarget(_player.Position);
        _cameraManager.SnapToTarget(); // Start centered on player
        
        // Spawn enemies and items at random walkable locations
        SpawnEntities();
        
        _currentState = GameState.Playing;
    }
    
    public void Update()
    {
        switch (_currentState)
        {
            case GameState.Intro:
                _introScreen.Update(Raylib.GetFrameTime());
                break;
            case GameState.Playing:
                _gameScreen.Update(Raylib.GetFrameTime());
                break;
            case GameState.Inventory:
                _inventoryScreen.Update(Raylib.GetFrameTime());
                break;
            case GameState.Paused:
                UpdatePaused();
                break;
            case GameState.GameOver:
                _gameOverScreen.Update(Raylib.GetFrameTime());
                break;
        }
    }
    
    public void Draw()
    {
        switch (_currentState)
        {
            case GameState.Intro:
                _introScreen.Draw();
                break;
            case GameState.Playing:
                _gameScreen.Draw();
                break;
            case GameState.Inventory:
                _inventoryScreen.Draw();
                break;
            case GameState.Paused:
                DrawPaused();
                break;
            case GameState.GameOver:
                _gameOverScreen.Draw();
                break;
        }
    }
    
    private void UpdateMenu()
    {
        // Check for input to start the game
        if (Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            StartNewGame();
        }
    }
    
    private void UpdatePaused()
    {
        // Check for unpause
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            _currentState = GameState.Playing;
        }
        
        // Check for return to intro
        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            _currentState = GameState.Intro;
        }
    }

    private void DrawMenu()
    {
        Raylib.ClearBackground(Color.Black);
        
        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();
        
        // Title
        string title = "DUNGEON GAME";
        int titleFontSize = 40;
        int titleWidth = FontManager.MeasureText(title, titleFontSize, FontType.Title);
        FontManager.DrawText(title, (screenWidth - titleWidth) / 2, screenHeight / 2 - 60, titleFontSize, Color.White, FontType.Title);
        
        // Instructions
        string instruction = "Press SPACE or ENTER to start";
        int instructionFontSize = 20;
        int instructionWidth = FontManager.MeasureText(instruction, instructionFontSize, FontType.UI);
        FontManager.DrawText(instruction, (screenWidth - instructionWidth) / 2, screenHeight / 2 + 20, instructionFontSize, Color.Gray, FontType.UI);
    }
    
    private void DrawPaused()
    {
        // Draw the game state first (grayed out)
        _gameScreen.Draw();
        
        // Draw pause overlay
        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();
        
        // Semi-transparent overlay
        Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, new Color(0, 0, 0, 128));
        
        // Pause text
        string pauseText = "PAUSED";
        int pauseFontSize = 40;
        int pauseWidth = FontManager.MeasureText(pauseText, pauseFontSize, FontType.Title);
        FontManager.DrawText(pauseText, (screenWidth - pauseWidth) / 2, screenHeight / 2 - 40, pauseFontSize, Color.White, FontType.Title);
        
        // Instructions
        string instruction1 = "Press ESC to resume";
        string instruction2 = "Press Q to return to menu";
        int instructionFontSize = 16;
        int instruction1Width = FontManager.MeasureText(instruction1, instructionFontSize, FontType.UI);
        int instruction2Width = FontManager.MeasureText(instruction2, instructionFontSize, FontType.UI);
          FontManager.DrawText(instruction1, (screenWidth - instruction1Width) / 2, screenHeight / 2 + 20, instructionFontSize, Color.LightGray, FontType.UI);
        FontManager.DrawText(instruction2, (screenWidth - instruction2Width) / 2, screenHeight / 2 + 45, instructionFontSize, Color.LightGray, FontType.UI);
    }

    public void SetGameState(GameState newState)
    {
        _currentState = newState;
        
        // Set up inventory screen when transitioning to it
        if (newState == GameState.Inventory && _player != null)
        {
            _inventoryScreen.SetInventory(_player.Inventory);
        }
    }
    
    public void EndGame()
    {
        _currentState = GameState.GameOver;
    }
    
    public void Cleanup()
    {
        // Unload fonts when shutting down
        FontManager.UnloadFonts();
    }
    
    private void SpawnEntities()
    {
        _enemies.Clear();
        
        if (_dungeon == null) return;
        
        // Get all walkable positions
        var walkablePositions = _dungeon.GetWalkablePositions();
        
        // Remove player starting position from possible spawn locations
        var playerPos = _dungeon.EntrancePosition;
        walkablePositions.RemoveAll(pos => Vector2.Distance(pos, playerPos) < _dungeon.TileSize * 2);
        
        // Also remove exit position to avoid spawning too close to exit
        walkablePositions.RemoveAll(pos => Vector2.Distance(pos, _dungeon.ExitPosition) < _dungeon.TileSize);
        
        // Spawn 10 skeleton enemies at random locations
        for (int i = 0; i < 10 && walkablePositions.Count > 0; i++)
        {
            int randomIndex = _random.Next(walkablePositions.Count);
            Vector2 spawnPosition = walkablePositions[randomIndex];
            
            // Create skeleton enemy
            var skeleton = new Skeleton(spawnPosition);
            _enemies.Add(skeleton);
            
            // Remove this position so we don't spawn multiple entities in the same spot
            walkablePositions.RemoveAt(randomIndex);
        }
        
        // Spawn items using consolidated method
        SpawnItems(walkablePositions, () => new HealingPotion(10), 10);
        SpawnItems(walkablePositions, () => new OldBoots(), 5);
        SpawnItems(walkablePositions, () => new BrokenSword(), 5);
    }
    
    /// <summary>
    /// Spawns a specified number of items at random walkable positions
    /// </summary>
    /// <typeparam name="T">Type of item to spawn</typeparam>
    /// <param name="walkablePositions">List of available spawn positions</param>
    /// <param name="itemFactory">Factory function to create new items</param>
    /// <param name="count">Number of items to spawn</param>
    private void SpawnItems<T>(List<Vector2> walkablePositions, Func<T> itemFactory, int count) where T : Item
    {
        for (int i = 0; i < count && walkablePositions.Count > 0; i++)
        {
            int randomIndex = _random.Next(walkablePositions.Count);
            Vector2 spawnPosition = walkablePositions[randomIndex];
            
            // Create item using factory and add to dungeon
            var item = itemFactory();
            item.Position = spawnPosition;
            _dungeon!.AddItem(item);
            
            // Remove this position so we don't spawn multiple entities in the same spot
            walkablePositions.RemoveAt(randomIndex);
        }
    }
    
    // Public methods for GameScreen to access
    public void RemoveEnemy(Enemy enemy)
    {
        _enemies.Remove(enemy);
    }
    
    public void UpdateEnemies(float deltaTime)
    {
        foreach (var enemy in _enemies)
        {
            enemy.Update(deltaTime);
        }
    }
    
    public void DrawEnemies(Vector2 cameraOffset = default)
    {
        foreach (var enemy in _enemies)
        {
            enemy.Draw(cameraOffset);
        }
    }
    
    public void CheckForCombatEncounters()
    {
        if (_player == null || _dungeon == null) return;
        
        // Check if player is close to any enemy
        foreach (var enemy in _enemies)
        {
            if (!enemy.IsAlive) continue;
            
            float distance = Vector2.Distance(_player.Position, enemy.Position);
            float combatRange = (_player.Radius + enemy.Radius) * 1.5f; // Combat triggers when close
            
            if (distance <= combatRange)
            {
                // Start combat with this enemy
                _combat.StartCombat(_player, enemy);
                break; // Only fight one enemy at a time
            }
        }
    }

    public void CheckForItemPickups()
    {
        if (_player == null || _dungeon == null) return;

        // Get items near the player
        float pickupRange = _player.Radius + 5f; // Slightly larger than player radius
        var nearbyItems = _dungeon.GetItemsNearPosition(_player.Position, pickupRange);

        // Try to pick up each nearby item
        foreach (var item in nearbyItems.ToList()) // ToList() to avoid modification during iteration
        {
            if (item.OnPickup(_player))
            {
                // Successfully picked up, remove from dungeon
                _dungeon.RemoveItem(item);
                
                // Add floating number for pickup feedback
                _floatingNumbers.AddTextNumber(item.Position, $"+{item.Name}", Color.White, 1.5f, 14);
            }
        }
    }
}
