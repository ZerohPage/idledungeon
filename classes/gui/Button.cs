using Raylib_cs;
using System;
using System.Numerics;

namespace RaylibGame.Classes.Gui;

/// <summary>
/// A clickable button control that can display text and respond to mouse clicks
/// </summary>
public class Button : Control
{
    /// <summary>
    /// Text displayed on the button
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Font size for the button text
    /// </summary>
    public int FontSize { get; set; } = 20;

    /// <summary>
    /// Background color of the button
    /// </summary>
    public Color BackgroundColor { get; set; } = Color.DarkGray;

    /// <summary>
    /// Text color of the button
    /// </summary>
    public Color TextColor { get; set; } = Color.White;

    /// <summary>
    /// Background color when button is hovered
    /// </summary>
    public Color HoverColor { get; set; } = Color.Gray;

    /// <summary>
    /// Background color when button is pressed
    /// </summary>
    public Color PressedColor { get; set; } = Color.LightGray;

    /// <summary>
    /// Event fired when the button is clicked
    /// </summary>
    public event Action? OnClick;

    /// <summary>
    /// Whether the mouse is currently hovering over the button
    /// </summary>
    public bool IsHovered { get; private set; }

    /// <summary>
    /// Whether the button is currently being pressed
    /// </summary>
    public bool IsPressed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the Button class
    /// </summary>
    /// <param name="gameManager">The game manager instance</param>
    /// <param name="position">Position of the button</param>
    /// <param name="size">Size of the button</param>
    /// <param name="text">Text to display on the button</param>
    public Button(GameManager gameManager, Vector2 position, Vector2 size, string text) 
        : base(gameManager, position, size)
    {
        Text = text;
    }

    /// <summary>
    /// Creates a small standardized button (120x40)
    /// </summary>
    /// <param name="gameManager">The game manager instance</param>
    /// <param name="position">Position of the button</param>
    /// <param name="text">Text to display on the button</param>
    /// <returns>Configured small button</returns>
    public static Button CreateSmallButton(GameManager gameManager, Vector2 position, string text)
    {
        return new Button(gameManager, position, new Vector2(120, 40), text)
        {
            FontSize = 12,
            CornerRadius = 6.0f
        };
    }

    /// <summary>
    /// Creates a medium standardized button (160x50)
    /// </summary>
    /// <param name="gameManager">The game manager instance</param>
    /// <param name="position">Position of the button</param>
    /// <param name="text">Text to display on the button</param>
    /// <returns>Configured medium button</returns>
    public static Button CreateMediumButton(GameManager gameManager, Vector2 position, string text)
    {
        return new Button(gameManager, position, new Vector2(160, 50), text)
        {
            FontSize = 16,
            CornerRadius = 8.0f
        };
    }

    /// <summary>
    /// Creates a large standardized button (200x60)
    /// </summary>
    /// <param name="gameManager">The game manager instance</param>
    /// <param name="position">Position of the button</param>
    /// <param name="text">Text to display on the button</param>
    /// <returns>Configured large button</returns>
    public static Button CreateBigButton(GameManager gameManager, Vector2 position, string text)
    {
        return new Button(gameManager, position, new Vector2(200, 60), text)
        {
            FontSize = 20,
            CornerRadius = 10.0f
        };
    }

    /// <summary>
    /// Automatically adjusts the font size to fit the text within the button
    /// </summary>
    /// <param name="padding">Padding to leave around the text (default: 10px)</param>
    public void AutoFitText(int padding = 10)
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
        } while (testFontSize > 8); // Minimum font size of 8

        FontSize = Math.Max(testFontSize, 8);
    }

    /// <summary>
    /// Updates the button's state and handles input
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update</param>
    public override void Update(float deltaTime)
    {
        if (!Enabled) return;

        Vector2 mousePosition = Raylib.GetMousePosition();
        IsHovered = ContainsPoint(mousePosition);

        if (IsHovered)
        {
            // Check for mouse button press and release
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                IsPressed = true;
            }
            else if (Raylib.IsMouseButtonReleased(MouseButton.Left) && IsPressed)
            {
                IsPressed = false;
                OnClick?.Invoke();
            }
        }
        else
        {
            IsPressed = false;
        }

        // Reset pressed state if mouse button is released anywhere
        if (Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            IsPressed = false;
        }
    }

    /// <summary>
    /// Renders the button to the screen
    /// </summary>
    public override void Draw()
    {
        if (!Visible) return;

        // Determine button color based on state
        Color currentColor = BackgroundColor;
        if (!Enabled)
        {
            currentColor = Color.DarkGray;
        }
        else if (IsPressed)
        {
            currentColor = PressedColor;
        }
        else if (IsHovered)
        {
            currentColor = HoverColor;
        }

        // Draw button background
        DrawBackground(currentColor);

        // Draw button outline
        DrawControlOutline();

        // Draw button text using inherited font functionality
        Color textColor = Enabled ? TextColor : Color.DarkGray;
        DrawCenteredText(Text, FontSize, textColor);
    }
}
