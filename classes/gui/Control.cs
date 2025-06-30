using Raylib_cs;
using System.Numerics;

namespace RaylibGame.Classes.Gui;

/// <summary>
/// Base class for all GUI controls providing common functionality
/// </summary>
public abstract class Control
{
    /// <summary>
    /// Position of the control on screen
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Size of the control
    /// </summary>
    public Vector2 Size { get; set; }

    /// <summary>
    /// Whether the control is visible and should be drawn
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Whether the control is enabled and can receive input
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Reference to the game manager for accessing game state
    /// </summary>
    protected GameManager GameManager { get; }

    /// <summary>
    /// Font type to use for this control
    /// </summary>
    public FontType FontType { get; set; } = FontType.UI;

    /// <summary>
    /// Whether to draw the control with rounded corners
    /// </summary>
    public bool RoundedCorners { get; set; } = true;

    /// <summary>
    /// Whether to draw an outline around the control
    /// </summary>
    public bool DrawOutline { get; set; } = true;

    /// <summary>
    /// Color of the outline (if DrawOutline is true)
    /// </summary>
    public Color OutlineColor { get; set; } = Color.Black;

    /// <summary>
    /// Thickness of the outline (if DrawOutline is true)
    /// </summary>
    public float OutlineThickness { get; set; } = 2.0f;

    /// <summary>
    /// Radius for rounded corners (if RoundedCorners is true)
    /// </summary>
    public float CornerRadius { get; set; } = 8.0f;

    /// <summary>
    /// Initializes a new instance of the Control class
    /// </summary>
    /// <param name="gameManager">The game manager instance</param>
    /// <param name="position">Initial position of the control</param>
    /// <param name="size">Size of the control</param>
    public Control(GameManager gameManager, Vector2 position, Vector2 size)
    {
        GameManager = gameManager;
        Position = position;
        Size = size;
    }

    /// <summary>
    /// Gets the bounding rectangle of the control
    /// </summary>
    public Rectangle Bounds => new Rectangle(Position.X, Position.Y, Size.X, Size.Y);

    /// <summary>
    /// Checks if a point is within the control's bounds
    /// </summary>
    /// <param name="point">The point to check</param>
    /// <returns>True if the point is within bounds</returns>
    public bool ContainsPoint(Vector2 point)
    {
        return Raylib.CheckCollisionPointRec(point, Bounds);
    }

    /// <summary>
    /// Updates the control's state
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update</param>
    public virtual void Update(float deltaTime)
    {
        // Base implementation - can be overridden by derived classes
    }

    /// <summary>
    /// Renders the control to the screen
    /// </summary>
    public virtual void Draw()
    {
        if (!Visible) return;
        
        // Base implementation - should be overridden by derived classes
    }

    /// <summary>
    /// Called when the control receives focus
    /// </summary>
    public virtual void OnFocus()
    {
        // Base implementation - can be overridden by derived classes
    }

    /// <summary>
    /// Called when the control loses focus
    /// </summary>
    public virtual void OnLoseFocus()
    {
        // Base implementation - can be overridden by derived classes
    }

    /// <summary>
    /// Draws text using the FontManager with the control's font type
    /// </summary>
    /// <param name="text">Text to draw</param>
    /// <param name="position">Position to draw the text</param>
    /// <param name="fontSize">Font size</param>
    /// <param name="color">Text color</param>
    protected void DrawText(string text, Vector2 position, int fontSize, Color color)
    {
        FontManager.DrawText(text, (int)position.X, (int)position.Y, fontSize, color, FontType);
    }

    /// <summary>
    /// Measures text size using the FontManager with the control's font type
    /// </summary>
    /// <param name="text">Text to measure</param>
    /// <param name="fontSize">Font size</param>
    /// <returns>Size of the text</returns>
    protected Vector2 MeasureText(string text, int fontSize)
    {
        return FontManager.MeasureTextEx(text, fontSize, FontType);
    }

    /// <summary>
    /// Draws centered text within the control's bounds
    /// </summary>
    /// <param name="text">Text to draw</param>
    /// <param name="fontSize">Font size</param>
    /// <param name="color">Text color</param>
    protected void DrawCenteredText(string text, int fontSize, Color color)
    {
        Vector2 textSize = MeasureText(text, fontSize);
        Vector2 textPosition = new Vector2(
            Position.X + (Size.X - textSize.X) / 2,
            Position.Y + (Size.Y - textSize.Y) / 2
        );
        DrawText(text, textPosition, fontSize, color);
    }

    /// <summary>
    /// Draws the control's background with optional rounded corners
    /// </summary>
    /// <param name="backgroundColor">Background color to fill</param>
    protected void DrawBackground(Color backgroundColor)
    {
        if (RoundedCorners)
        {
            DrawRoundedRectangle(Bounds, CornerRadius, backgroundColor);
        }
        else
        {
            Raylib.DrawRectangleRec(Bounds, backgroundColor);
        }
    }

    /// <summary>
    /// Draws the control's outline with optional rounded corners
    /// </summary>
    protected void DrawControlOutline()
    {
        if (!DrawOutline) return;

        if (RoundedCorners)
        {
            DrawRoundedRectangleLines(Bounds, CornerRadius, OutlineThickness, OutlineColor);
        }
        else
        {
            Raylib.DrawRectangleLinesEx(Bounds, OutlineThickness, OutlineColor);
        }
    }

    /// <summary>
    /// Draws a rounded rectangle
    /// </summary>
    /// <param name="rect">Rectangle to draw</param>
    /// <param name="roundness">Corner radius</param>
    /// <param name="color">Fill color</param>
    private void DrawRoundedRectangle(Rectangle rect, float roundness, Color color)
    {
        // Use Raylib's rounded rectangle if available, otherwise fall back to regular rectangle
        // Note: DrawRectangleRounded might not be available in all Raylib-cs versions
        try
        {
            Raylib.DrawRectangleRounded(rect, roundness / Math.Min(rect.Width, rect.Height), 16, color);
        }
        catch
        {
            // Fallback to regular rectangle if rounded rectangle is not available
            Raylib.DrawRectangleRec(rect, color);
        }
    }

    /// <summary>
    /// Draws rounded rectangle outline
    /// </summary>
    /// <param name="rect">Rectangle to draw</param>
    /// <param name="roundness">Corner radius</param>
    /// <param name="thickness">Line thickness</param>
    /// <param name="color">Line color</param>
    private void DrawRoundedRectangleLines(Rectangle rect, float roundness, float thickness, Color color)
    {
        // Use Raylib's rounded rectangle lines if available, otherwise fall back to regular rectangle lines
        try
        {
            Raylib.DrawRectangleRoundedLines(rect, roundness / Math.Min(rect.Width, rect.Height), 16, thickness, color);
        }
        catch
        {
            // Fallback to regular rectangle lines if rounded rectangle lines is not available
            Raylib.DrawRectangleLinesEx(rect, thickness, color);
        }
    }
}
