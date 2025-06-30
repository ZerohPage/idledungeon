using Raylib_cs;
using RaylibGame.Classes.Gui;
using System.Numerics;

namespace RaylibGame.Classes;

/// <summary>
/// Introduction screen shown at the start of the game
/// </summary>
public class IntroScreen : Screen
{
    private Button startButton;

    public IntroScreen(GameManager gameManager) : base(gameManager)
    {
        // Create start button centered on screen
        var (screenWidth, screenHeight) = GetScreenSize();
        var buttonSize = new Vector2(200, 50);
        var buttonPosition = new Vector2(
            (screenWidth - buttonSize.X) / 2, 
            screenHeight / 2 + 60
        );
        
        startButton = new Button(gameManager, buttonPosition, buttonSize, "START GAME");
        startButton.BackgroundColor = Color.DarkBlue;
        startButton.HoverColor = Color.Blue;
        startButton.PressedColor = Color.LightGray;
        startButton.OnClick += () => GameManager.StartNewGame();
    }

    public override void Update(float deltaTime)
    {
        // Update the start button
        startButton.Update(deltaTime);

        // Check for input to start the game (keeping keyboard support)
        if (Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            GameManager.StartNewGame();
        }
    }

    public override void Draw()
    {
        Raylib.ClearBackground(Color.Green);

        var (screenWidth, screenHeight) = GetScreenSize();

        // Draw title
        string title = "DUNGEON GAME";
        DrawCenteredText(title, screenHeight / 2 - 60, 40, Color.White, FontType.Title);

        // Draw instruction
        string instruction = "Press SPACE or ENTER to start, or click the button below";
        DrawCenteredText(instruction, screenHeight / 2 + 20, 16, Color.Gray, FontType.UI);

        // Draw the start button
        startButton.Draw();
    }
}
