using Raylib_cs;
using RaylibGame.Classes;

namespace RaylibGame;

class Program
{
    static void Main()
    {
        // Initialization
        const int screenWidth = 1920;
        const int screenHeight = 1080;

        Raylib.InitWindow(screenWidth, screenHeight, "Dungeon Game");
        Raylib.SetTargetFPS(60);
        Raylib.SetExitKey(KeyboardKey.Null); // Disable automatic exit on ESC key

        // Create game manager
        var gameManager = new GameManager();
        
        // Initialize the game manager (loads fonts, etc.)
        gameManager.Initialize();

        // Main game loop
        while (!Raylib.WindowShouldClose())
        {
            // Update
            gameManager.Update();

            // Draw
            Raylib.BeginDrawing();
            gameManager.Draw();
            Raylib.EndDrawing();
        }

        // De-Initialization
        gameManager.Cleanup();
        Raylib.CloseWindow();
    }
}
