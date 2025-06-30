using Raylib_cs;

namespace RaylibGame.Classes;

/// <summary>
/// Introduction screen shown at the start of the game
/// </summary>
public class IntroScreen : Screen
{
    public IntroScreen(GameManager gameManager) : base(gameManager)
    {
    }

    public override void Update(float deltaTime)
    {
        // Check for input to start the game
        if (Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            GameManager.StartNewGame();
        }
    }

    public override void Draw()
    {
        Raylib.ClearBackground(Color.Black);

        var (screenWidth, screenHeight) = GetScreenSize();

        // Draw title
        string title = "DUNGEON GAME";
        DrawCenteredText(title, screenHeight / 2 - 60, 40, Color.White, FontType.Title);

        // Draw instruction
        string instruction = "Press SPACE or ENTER to start";
        DrawCenteredText(instruction, screenHeight / 2 + 20, 20, Color.Gray, FontType.UI);
    }
}
