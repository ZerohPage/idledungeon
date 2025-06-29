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

        // Create game manager
        var gameManager = new GameManager();

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
        Raylib.CloseWindow();
    }
}
