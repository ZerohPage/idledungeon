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
    private Button quitButton;
    private Button guiDesignerButton;

    public IntroScreen(GameManager gameManager) : base(gameManager)
    {
        // Create start button centered on screen
        var (screenWidth, screenHeight) = GetScreenSize();
        var startButtonPosition = new Vector2(
            (screenWidth - 200) / 2, // BigButton is 200px wide
            screenHeight / 2 + 60
        );
        
        startButton = Button.CreateBigButton(gameManager, startButtonPosition, "START GAME");
        startButton.BackgroundColor = Color.DarkBlue;
        startButton.HoverColor = Color.Blue;
        startButton.PressedColor = Color.LightGray;
        startButton.OnClick += () => GameManager.StartNewGame();

        // Create GUI Designer button below start button
        var designerButtonPosition = new Vector2(
            (screenWidth - 200) / 2, // BigButton is 200px wide
            screenHeight / 2 + 140
        );
        
        guiDesignerButton = Button.CreateBigButton(gameManager, designerButtonPosition, "GUI DESIGNER");
        guiDesignerButton.BackgroundColor = Color.DarkGreen;
        guiDesignerButton.HoverColor = Color.Green;
        guiDesignerButton.PressedColor = Color.LightGray;
        guiDesignerButton.OnClick += () => GameManager.SetGameState(GameState.GuiDesigner);

        // Create quit button below designer button
        var quitButtonPosition = new Vector2(
            (screenWidth - 200) / 2, // BigButton is 200px wide
            screenHeight / 2 + 220
        );
        
        quitButton = Button.CreateBigButton(gameManager, quitButtonPosition, "QUIT");
        quitButton.BackgroundColor = Color.Maroon;
        quitButton.HoverColor = Color.Red;
        quitButton.PressedColor = Color.LightGray;
        quitButton.OnClick += () => Environment.Exit(0);
    }

    public override void Update(float deltaTime)
    {
        // Update all buttons
        startButton.Update(deltaTime);
        guiDesignerButton.Update(deltaTime);
        quitButton.Update(deltaTime);

        // Check for input to start the game (keeping keyboard support)
        if (InputManager.IsMenuConfirmPressed)
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
        string instruction = "Press SPACE or ENTER to start, or use buttons below";
        DrawCenteredText(instruction, screenHeight / 2 + 20, 16, Color.Gray, FontType.UI);

        // Draw all buttons
        startButton.Draw();
        guiDesignerButton.Draw();
        quitButton.Draw();
    }
}
