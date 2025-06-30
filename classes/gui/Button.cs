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
