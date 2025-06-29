using Raylib_cs;
using System.Numerics;

namespace RaylibGame.Classes;

public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver
}

public class GameManager
{
    private GameState _currentState;
    private Dungeon? _dungeon;
    private Player? _player;
    
    public GameState CurrentState => _currentState;
    public Dungeon? CurrentDungeon => _dungeon;
    public Player? CurrentPlayer => _player;
    
    public GameManager()
    {
        _currentState = GameState.Menu;
    }
    
    public void Initialize()
    {
        // Initialize game systems
        StartNewGame();
    }
    
    public void StartNewGame()
    {
        // Create a new dungeon
        _dungeon = new Dungeon(60, 40);
        
        // Create player and set position to dungeon entrance
        _player = new Player(Vector2.Zero);

        _player.SetDungeon(_dungeon);
        
        _currentState = GameState.Playing;
    }
    
    public void Update()
    {
        switch (_currentState)
        {
            case GameState.Menu:
                UpdateMenu();
                break;
            case GameState.Playing:
                UpdateGameplay();
                break;
            case GameState.Paused:
                UpdatePaused();
                break;
            case GameState.GameOver:
                UpdateGameOver();
                break;
        }
    }
    
    public void Draw()
    {
        switch (_currentState)
        {
            case GameState.Menu:
                DrawMenu();
                break;
            case GameState.Playing:
                DrawGameplay();
                break;
            case GameState.Paused:
                DrawPaused();
                break;
            case GameState.GameOver:
                DrawGameOver();
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
    }
    
    private void UpdatePaused()
    {
        // Check for unpause
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            _currentState = GameState.Playing;
        }
        
        // Check for return to menu
        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            _currentState = GameState.Menu;
        }
    }
    
    private void UpdateGameOver()
    {
        // Check for restart or return to menu
        if (Raylib.IsKeyPressed(KeyboardKey.R))
        {
            StartNewGame();
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            _currentState = GameState.Menu;
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
        int titleWidth = Raylib.MeasureText(title, titleFontSize);
        Raylib.DrawText(title, (screenWidth - titleWidth) / 2, screenHeight / 2 - 60, titleFontSize, Color.White);
        
        // Instructions
        string instruction = "Press SPACE or ENTER to start";
        int instructionFontSize = 20;
        int instructionWidth = Raylib.MeasureText(instruction, instructionFontSize);
        Raylib.DrawText(instruction, (screenWidth - instructionWidth) / 2, screenHeight / 2 + 20, instructionFontSize, Color.Gray);
    }
    
    private void DrawGameplay()
    {
        Raylib.ClearBackground(Color.Black);
        
        // Draw the dungeon if it exists
        _dungeon?.Draw();
        
        // Draw the player if it exists
        _player?.Draw();
        
        // Draw UI
        Raylib.DrawText("Dungeon Game", 10, 10, 20, Color.White);
        Raylib.DrawText("Use WASD to move, ESC to pause", 10, 35, 16, Color.LightGray);
        
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
        int pauseWidth = Raylib.MeasureText(pauseText, pauseFontSize);
        Raylib.DrawText(pauseText, (screenWidth - pauseWidth) / 2, screenHeight / 2 - 40, pauseFontSize, Color.White);
        
        // Instructions
        string instruction1 = "Press ESC to resume";
        string instruction2 = "Press Q to return to menu";
        int instructionFontSize = 16;
        int instruction1Width = Raylib.MeasureText(instruction1, instructionFontSize);
        int instruction2Width = Raylib.MeasureText(instruction2, instructionFontSize);
        
        Raylib.DrawText(instruction1, (screenWidth - instruction1Width) / 2, screenHeight / 2 + 20, instructionFontSize, Color.LightGray);
        Raylib.DrawText(instruction2, (screenWidth - instruction2Width) / 2, screenHeight / 2 + 45, instructionFontSize, Color.LightGray);
    }
    
    private void DrawGameOver()
    {
        Raylib.ClearBackground(new Color(100, 0, 0, 255));
        
        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();
        
        // Game Over text
        string gameOverText = "GAME OVER";
        int gameOverFontSize = 40;
        int gameOverWidth = Raylib.MeasureText(gameOverText, gameOverFontSize);
        Raylib.DrawText(gameOverText, (screenWidth - gameOverWidth) / 2, screenHeight / 2 - 40, gameOverFontSize, Color.White);
        
        // Instructions
        string instruction1 = "Press R to restart";
        string instruction2 = "Press Q to return to menu";
        int instructionFontSize = 16;
        int instruction1Width = Raylib.MeasureText(instruction1, instructionFontSize);
        int instruction2Width = Raylib.MeasureText(instruction2, instructionFontSize);
        
        Raylib.DrawText(instruction1, (screenWidth - instruction1Width) / 2, screenHeight / 2 + 20, instructionFontSize, Color.LightGray);
        Raylib.DrawText(instruction2, (screenWidth - instruction2Width) / 2, screenHeight / 2 + 45, instructionFontSize, Color.LightGray);
    }
    
    public void SetGameState(GameState newState)
    {
        _currentState = newState;
    }
    
    public void EndGame()
    {
        _currentState = GameState.GameOver;
    }
}
