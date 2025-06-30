using Raylib_cs;

namespace RaylibGame.Classes;

/// <summary>
/// Base class for all game screens. Provides a standard interface for screen lifecycle management.
/// </summary>
public abstract class Screen
{
    /// <summary>
    /// Reference to the game manager for state transitions and shared data access
    /// </summary>
    protected GameManager GameManager { get; private set; }
    
    /// <summary>
    /// Whether this screen is currently active
    /// </summary>
    public bool IsActive { get; private set; }
    
    /// <summary>
    /// Constructor that requires a GameManager reference
    /// </summary>
    /// <param name="gameManager">The main game manager instance</param>
    protected Screen(GameManager gameManager)
    {
        GameManager = gameManager;
        IsActive = false;
    }
    
    /// <summary>
    /// Called when the screen becomes active. Use this for initialization.
    /// </summary>
    public virtual void OnEnter()
    {
        IsActive = true;
    }
    
    /// <summary>
    /// Called when the screen is no longer active. Use this for cleanup.
    /// </summary>
    public virtual void OnExit()
    {
        IsActive = false;
    }
    
    /// <summary>
    /// Called every frame to update the screen logic
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame</param>
    public abstract void Update(float deltaTime);
    
    /// <summary>
    /// Called every frame to render the screen
    /// </summary>
    public abstract void Draw();
    
    /// <summary>
    /// Helper method to get screen dimensions
    /// </summary>
    protected (int width, int height) GetScreenSize()
    {
        return (Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
    }
    
    /// <summary>
    /// Helper method to center text horizontally
    /// </summary>
    protected int GetCenteredTextX(string text, int fontSize, FontType fontType = FontType.Default)
    {
        int textWidth = FontManager.MeasureText(text, fontSize, fontType);
        return (Raylib.GetScreenWidth() - textWidth) / 2;
    }
    
    /// <summary>
    /// Helper method to center text both horizontally and vertically
    /// </summary>
    protected (int x, int y) GetCenteredTextPosition(string text, int fontSize, FontType fontType = FontType.Default)
    {
        int textWidth = FontManager.MeasureText(text, fontSize, fontType);
        var textSize = FontManager.MeasureTextEx(text, fontSize, fontType);
        
        int x = (Raylib.GetScreenWidth() - textWidth) / 2;
        int y = (Raylib.GetScreenHeight() - (int)textSize.Y) / 2;
        
        return (x, y);
    }
    
    /// <summary>
    /// Helper method to draw text centered horizontally at a specific Y position
    /// </summary>
    protected void DrawCenteredText(string text, int y, int fontSize, Color color, FontType fontType = FontType.Default)
    {
        int x = GetCenteredTextX(text, fontSize, fontType);
        FontManager.DrawText(text, x, y, fontSize, color, fontType);
    }
    
    /// <summary>
    /// Helper method to draw text centered both horizontally and vertically
    /// </summary>
    protected void DrawCenteredText(string text, int fontSize, Color color, FontType fontType = FontType.Default)
    {
        var (x, y) = GetCenteredTextPosition(text, fontSize, fontType);
        FontManager.DrawText(text, x, y, fontSize, color, fontType);
    }
    
    /// <summary>
    /// Helper method to draw a semi-transparent overlay over the entire screen
    /// </summary>
    protected void DrawOverlay(Color color)
    {
        var (width, height) = GetScreenSize();
        Raylib.DrawRectangle(0, 0, width, height, color);
    }
}
