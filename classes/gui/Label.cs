using Raylib_cs;
using System.Numerics;

namespace RaylibGame.Classes.Gui;

/// <summary>
/// A text label control for displaying non-interactive text
/// </summary>
public class Label : Control
{
    /// <summary>
    /// Text displayed on the label
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Font size for the label text
    /// </summary>
    public int FontSize { get; set; } = 16;

    /// <summary>
    /// Text color of the label
    /// </summary>
    public Color TextColor { get; set; } = Color.Black;

    /// <summary>
    /// Background color of the label (transparent by default)
    /// </summary>
    public Color BackgroundColor { get; set; } = Color.Blank;

    /// <summary>
    /// Text alignment within the label
    /// </summary>
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;

    /// <summary>
    /// Whether to draw a background behind the text
    /// </summary>
    public bool ShowBackground { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the Label class
    /// </summary>
    /// <param name="gameManager">The game manager instance</param>
    /// <param name="position">Position of the label</param>
    /// <param name="size">Size of the label</param>
    /// <param name="text">Text to display on the label</param>
    public Label(GameManager gameManager, Vector2 position, Vector2 size, string text) 
        : base(gameManager, position, size)
    {
        Text = text;
        DrawOutline = false; // Labels typically don't have outlines
        RoundedCorners = false; // Labels typically don't have rounded corners
    }

    /// <summary>
    /// Creates a small standardized label (120x20)
    /// </summary>
    /// <param name="gameManager">The game manager instance</param>
    /// <param name="position">Position of the label</param>
    /// <param name="text">Text to display on the label</param>
    /// <returns>Configured small label</returns>
    public static Label CreateSmallLabel(GameManager gameManager, Vector2 position, string text)
    {
        return new Label(gameManager, position, new Vector2(120, 20), text)
        {
            FontSize = 12,
            TextColor = Color.DarkGray
        };
    }

    /// <summary>
    /// Creates a medium standardized label (160x25)
    /// </summary>
    /// <param name="gameManager">The game manager instance</param>
    /// <param name="position">Position of the label</param>
    /// <param name="text">Text to display on the label</param>
    /// <returns>Configured medium label</returns>
    public static Label CreateMediumLabel(GameManager gameManager, Vector2 position, string text)
    {
        return new Label(gameManager, position, new Vector2(160, 25), text)
        {
            FontSize = 16,
            TextColor = Color.Black
        };
    }

    /// <summary>
    /// Creates a large standardized label (200x30)
    /// </summary>
    /// <param name="gameManager">The game manager instance</param>
    /// <param name="position">Position of the label</param>
    /// <param name="text">Text to display on the label</param>
    /// <returns>Configured large label</returns>
    public static Label CreateLargeLabel(GameManager gameManager, Vector2 position, string text)
    {
        return new Label(gameManager, position, new Vector2(200, 30), text)
        {
            FontSize = 20,
            TextColor = Color.Black
        };
    }

    /// <summary>
    /// Automatically adjusts the font size to fit the text within the label
    /// </summary>
    /// <param name="padding">Padding to leave around the text (default: 5px)</param>
    public void AutoFitText(int padding = 5)
    {
        if (string.IsNullOrEmpty(Text)) return;

        int maxWidth = (int)Size.X - (padding * 2);
        int maxHeight = (int)Size.Y - (padding * 2);

        // Start with current font size and adjust down if needed
        int testFontSize = FontSize;
        Vector2 textSize;

        do
        {
            textSize = FontManager.MeasureTextEx(Text, testFontSize, FontType);
            if (textSize.X <= maxWidth && textSize.Y <= maxHeight)
            {
                break;
            }
            testFontSize--;
        } while (testFontSize > 6); // Minimum font size of 6 for labels

        FontSize = Math.Max(testFontSize, 6);
    }

    /// <summary>
    /// Updates the label's state
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update</param>
    public override void Update(float deltaTime)
    {
        // Labels typically don't need to update, but we keep this for consistency
        base.Update(deltaTime);
    }

    /// <summary>
    /// Renders the label to the screen
    /// </summary>
    public override void Draw()
    {
        if (!Visible) return;

        // Draw background if enabled
        if (ShowBackground)
        {
            DrawBackground(BackgroundColor);
        }

        // Draw outline if enabled
        if (DrawOutline)
        {
            DrawControlOutline();
        }

        // Draw text based on alignment
        DrawAlignedText();
    }

    /// <summary>
    /// Draws text according to the specified alignment
    /// </summary>
    private void DrawAlignedText()
    {
        if (string.IsNullOrEmpty(Text)) return;

        Vector2 textSize = MeasureText(Text, FontSize);
        Vector2 textPosition;

        switch (Alignment)
        {
            case TextAlignment.Left:
                textPosition = new Vector2(
                    Position.X + 5, // Small left padding
                    Position.Y + (Size.Y - textSize.Y) / 2
                );
                break;

            case TextAlignment.Center:
                textPosition = new Vector2(
                    Position.X + (Size.X - textSize.X) / 2,
                    Position.Y + (Size.Y - textSize.Y) / 2
                );
                break;

            case TextAlignment.Right:
                textPosition = new Vector2(
                    Position.X + Size.X - textSize.X - 5, // Small right padding
                    Position.Y + (Size.Y - textSize.Y) / 2
                );
                break;

            default:
                textPosition = Position;
                break;
        }

        DrawText(Text, textPosition, FontSize, TextColor);
    }
}

/// <summary>
/// Text alignment options for labels
/// </summary>
public enum TextAlignment
{
    Left,
    Center,
    Right
}
