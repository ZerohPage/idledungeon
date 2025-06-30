using Raylib_cs;

namespace RaylibGame.Classes;

/// <summary>
/// Game Over screen shown when the player dies
/// </summary>
public class GameOverScreen : Screen
{
    public GameOverScreen(GameManager gameManager) : base(gameManager)
    {
    }

    public override void Update(float deltaTime)
    {
        // Check for restart or return to intro
        if (Raylib.IsKeyPressed(KeyboardKey.R))
        {
            GameManager.StartNewGame();
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            GameManager.SetGameState(GameState.Intro);
        }
    }

    public override void Draw()
    {
        Raylib.ClearBackground(new Color(100, 0, 0, 255));

        var (screenWidth, screenHeight) = GetScreenSize();

        // Game Over text
        string gameOverText = "GAME OVER";
        DrawCenteredText(gameOverText, screenHeight / 2 - 40, 40, Color.White, FontType.Title);

        // Instructions
        string instruction1 = "Press R to restart";
        string instruction2 = "Press Q to return to intro";
        
        DrawCenteredText(instruction1, screenHeight / 2 + 20, 16, Color.LightGray, FontType.UI);
        DrawCenteredText(instruction2, screenHeight / 2 + 45, 16, Color.LightGray, FontType.UI);
    }
}
