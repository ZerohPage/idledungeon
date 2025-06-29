using Raylib_cs;
using System.Numerics;

namespace RaylibGame.Classes;

public enum GameState
{
    Intro,
    Playing,
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
    
    public GameState CurrentState => _currentState;
    public Dungeon? CurrentDungeon => _dungeon;
    public Player? CurrentPlayer => _player;
    public FloatingNumberManager FloatingNumbers => _floatingNumbers;
    
    public GameManager()
    {
        _currentState = GameState.Intro;
        _enemies = new List<Enemy>();
        _random = new Random();
        _combat = new Combat();
        _floatingNumbers = new FloatingNumberManager();
        _introScreen = new IntroScreen(this);
        _gameOverScreen = new GameOverScreen(this);
        
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
        // Create a new dungeon
        _dungeon = new Dungeon(60, 40);
        
        // Create player and set position to dungeon entrance
        _player = new Player(Vector2.Zero);
        _player.SetDungeon(_dungeon);
        
        // Spawn enemies at random walkable locations
        SpawnEnemies();
        
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
                UpdateGameplay();
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
                DrawGameplay();
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
    
    private void UpdateGameplay()
    {
        // Check for pause
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            _currentState = GameState.Paused;
        }
        
        // Update combat system
        _combat.Update(Raylib.GetFrameTime());
        
        // Update floating numbers
        _floatingNumbers.Update(Raylib.GetFrameTime());
        
        // Check combat results
        if (_combat.State == CombatState.PlayerLoses)
        {
            _currentState = GameState.GameOver;
            _combat.EndCombat();
            return;
        }
        else if (_combat.State == CombatState.PlayerWins)
        {
            // Remove defeated enemy
            if (_combat.CurrentEnemy != null)
            {
                _enemies.Remove(_combat.CurrentEnemy);
            }
            _combat.EndCombat();
        }
        
        // Update player
        if (_player != null)
        {
            _player.Update(Raylib.GetFrameTime());
            
            // Update explored areas based on player position
            if (_dungeon != null)
            {
                _dungeon.UpdateExploredAreas(_player.Position);
            }
            
            // Check if player is dead
            if (!_player.IsAlive())
            {
                _currentState = GameState.GameOver;
            }
        }
        
        // Update enemies
        foreach (var enemy in _enemies)
        {
            enemy.Update(Raylib.GetFrameTime());
        }
        
        // Check for combat encounters (only if not already in combat)
        if (!_combat.IsInCombat && _player != null)
        {
            CheckForCombatEncounters();
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
    
    private void DrawGameplay()
    {
        Raylib.ClearBackground(Color.Black);
        
        // Draw the dungeon if it exists
        _dungeon?.Draw();
        
        // Draw enemies
        foreach (var enemy in _enemies)
        {
            enemy.Draw();
        }
        
        // Draw the player if it exists
        _player?.Draw();
        
        // Draw combat UI if in combat
        _combat.Draw();
        
        // Draw floating damage numbers
        _floatingNumbers.Draw();
        
        // Draw UI
        FontManager.DrawText("Dungeon Game", 10, 10, 20, Color.White, FontType.UI);
        FontManager.DrawText("Use WASD to move, ESC to pause", 10, 35, 16, Color.LightGray, FontType.UI);
        
        // Draw player health bar
        if (_player != null)
        {
            _player.DrawHealthBar(new Vector2(10, 70));
        }
        
        Raylib.DrawFPS(10, 100);
    }
    
    private void DrawPaused()
    {
        // Draw the game state first (grayed out)
        DrawGameplay();
        
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
    
    private void SpawnEnemies()
    {
        _enemies.Clear();
        
        if (_dungeon == null) return;
        
        // Get all walkable positions
        var walkablePositions = _dungeon.GetWalkablePositions();
        
        // Remove player starting position from possible spawn locations
        var playerPos = _dungeon.EntrancePosition;
        walkablePositions.RemoveAll(pos => Vector2.Distance(pos, playerPos) < _dungeon.TileSize * 2);
        
        // Spawn 10 skeleton enemies at random locations
        for (int i = 0; i < 10 && walkablePositions.Count > 0; i++)
        {
            int randomIndex = _random.Next(walkablePositions.Count);
            Vector2 spawnPosition = walkablePositions[randomIndex];
            
            // Create skeleton enemy
            var skeleton = new Skeleton(spawnPosition);
            _enemies.Add(skeleton);
            
            // Remove this position so we don't spawn multiple enemies in the same spot
            walkablePositions.RemoveAt(randomIndex);
        }
    }
    
    private void CheckForCombatEncounters()
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
}
