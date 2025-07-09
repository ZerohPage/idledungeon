using Raylib_cs;
using System.Numerics;

namespace RaylibGame.Classes.Gui;

/// <summary>
/// A control that displays information text with background, useful for tooltips and info panels
/// </summary>
public class InfoBox : Control
{
    /// <summary>
    /// The text content to display in the info box
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Font size for the text
    /// </summary>
    public int FontSize { get; set; } = 14;

    /// <summary>
    /// Text color
    /// </summary>
    public Color TextColor { get; set; } = Color.White;

    /// <summary>
    /// Optional color for the first line (useful for item names with rarity colors)
    /// </summary>
    public Color? FirstLineColor { get; set; } = null;

    /// <summary>
    /// Background color of the info box
    /// </summary>
    public Color BackgroundColor { get; set; } = new Color(40, 40, 40, 220); // Semi-transparent dark background

    /// <summary>
    /// Padding around the text content
    /// </summary>
    public Vector2 Padding { get; set; } = new Vector2(8, 6);

    /// <summary>
    /// Whether to auto-size the control to fit the text content
    /// </summary>
    public bool AutoSize { get; set; } = true;

    /// <summary>
    /// Maximum width for text wrapping (0 = no limit)
    /// </summary>
    public float MaxWidth { get; set; } = 0;

    /// <summary>
    /// Whether the info box should fade in/out
    /// </summary>
    public bool EnableFade { get; set; } = true;

    /// <summary>
    /// Current alpha value for fading (0-255)
    /// </summary>
    private byte _currentAlpha = 0;

    /// <summary>
    /// Target alpha value for fading
    /// </summary>
    private byte _targetAlpha = 255;

    /// <summary>
    /// Fade speed (alpha units per second)
    /// </summary>
    public float FadeSpeed { get; set; } = 512f; // Fast fade

    /// <summary>
    /// Initializes a new instance of the InfoBox class
    /// </summary>
    /// <param name="gameManager">The game manager instance</param>
    /// <param name="position">Initial position of the info box</param>
    /// <param name="text">Text content to display</param>
    public InfoBox(GameManager gameManager, Vector2 position, string text = "") 
        : base(gameManager, position, Vector2.Zero)
    {
        Text = text ?? string.Empty;
        
        // Configure styling for tooltip appearance
        RoundedCorners = true;
        CornerRadius = 4.0f;
        DrawOutline = true;
        OutlineColor = new Color(100, 100, 100, 200);
        OutlineThickness = 1.0f;

        // Auto-size to fit content
        if (AutoSize)
        {
            UpdateSize();
        }
    }

    /// <summary>
    /// Sets the text content and optionally updates the size
    /// </summary>
    /// <param name="text">New text content</param>
    public void SetText(string text)
    {
        Text = text ?? string.Empty;
        
        if (AutoSize)
        {
            UpdateSize();
        }
    }

    /// <summary>
    /// Shows the info box with fade-in effect
    /// </summary>
    public void Show()
    {
        Visible = true;
        _targetAlpha = 255;
    }

    /// <summary>
    /// Hides the info box with fade-out effect
    /// </summary>
    public void Hide()
    {
        _targetAlpha = 0;
    }

    /// <summary>
    /// Updates the info box size to fit the current text content
    /// </summary>
    private void UpdateSize()
    {
        if (string.IsNullOrEmpty(Text))
        {
            Size = new Vector2(Padding.X * 2, Padding.Y * 2);
            return;
        }

        Vector2 textSize = FontManager.MeasureTextEx(Text, FontSize, FontType);
        
        // Apply max width constraint if specified
        if (MaxWidth > 0 && textSize.X > MaxWidth - (Padding.X * 2))
        {
            // For now, just limit width - text wrapping would need more complex implementation
            textSize.X = MaxWidth - (Padding.X * 2);
        }

        Size = new Vector2(
            textSize.X + (Padding.X * 2),
            textSize.Y + (Padding.Y * 2)
        );
    }

    /// <summary>
    /// Updates the info box state including fade effects
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update</param>
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (EnableFade)
        {
            // Update fade animation
            if (_currentAlpha != _targetAlpha)
            {
                float fadeStep = FadeSpeed * deltaTime;
                
                if (_currentAlpha < _targetAlpha)
                {
                    _currentAlpha = (byte)Math.Min(255, _currentAlpha + fadeStep);
                }
                else
                {
                    _currentAlpha = (byte)Math.Max(0, _currentAlpha - fadeStep);
                }

                // Hide when fully faded out
                if (_currentAlpha == 0 && _targetAlpha == 0)
                {
                    Visible = false;
                }
            }
        }
        else
        {
            _currentAlpha = _targetAlpha;
        }
    }

    /// <summary>
    /// Renders the info box to the screen
    /// </summary>
    public override void Draw()
    {
        if (!Visible || string.IsNullOrEmpty(Text)) return;

        // Apply alpha to colors based on fade state
        byte alpha = EnableFade ? _currentAlpha : (byte)255;
        
        Color bgColor = new Color(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, 
            (byte)((BackgroundColor.A * alpha) / 255));
        Color textColor = new Color(TextColor.R, TextColor.G, TextColor.B, 
            (byte)((TextColor.A * alpha) / 255));
        Color outlineColor = new Color(OutlineColor.R, OutlineColor.G, OutlineColor.B, 
            (byte)((OutlineColor.A * alpha) / 255));

        // Draw background
        DrawBackground(bgColor);

        // Draw outline with updated color
        var originalOutlineColor = OutlineColor;
        OutlineColor = outlineColor;
        DrawControlOutline();
        OutlineColor = originalOutlineColor;

        // Draw text
        Vector2 textPosition = new Vector2(
            Position.X + Padding.X,
            Position.Y + Padding.Y
        );

        FontManager.DrawText(Text, (int)textPosition.X, (int)textPosition.Y, FontSize, textColor, FontType);
    }

    /// <summary>
    /// Positions the info box relative to a target position, ensuring it stays on screen
    /// </summary>
    /// <param name="targetPosition">Position to position relative to</param>
    /// <param name="offset">Offset from the target position</param>
    /// <param name="screenBounds">Screen boundaries to stay within</param>
    public void PositionNear(Vector2 targetPosition, Vector2 offset, Rectangle screenBounds)
    {
        Vector2 newPosition = targetPosition + offset;

        // Ensure the info box stays within screen bounds
        if (newPosition.X + Size.X > screenBounds.X + screenBounds.Width)
        {
            newPosition.X = screenBounds.X + screenBounds.Width - Size.X;
        }
        if (newPosition.X < screenBounds.X)
        {
            newPosition.X = screenBounds.X;
        }
        if (newPosition.Y + Size.Y > screenBounds.Y + screenBounds.Height)
        {
            newPosition.Y = targetPosition.Y - Size.Y - Math.Abs(offset.Y);
        }
        if (newPosition.Y < screenBounds.Y)
        {
            newPosition.Y = screenBounds.Y;
        }

        Position = newPosition;
    }

    /// <summary>
    /// Creates a tooltip-style info box at the mouse position
    /// </summary>
    /// <param name="gameManager">Game manager instance</param>
    /// <param name="text">Tooltip text</param>
    /// <param name="mousePosition">Current mouse position</param>
    /// <param name="screenBounds">Screen boundaries</param>
    /// <returns>Configured tooltip info box</returns>
    public static InfoBox CreateTooltip(GameManager gameManager, string text, Vector2 mousePosition, Rectangle screenBounds)
    {
        var tooltip = new InfoBox(gameManager, Vector2.Zero, text)
        {
            FontSize = 12,
            BackgroundColor = new Color(20, 20, 20, 240),
            TextColor = Color.White,
            Padding = new Vector2(6, 4),
            EnableFade = true,
            FadeSpeed = 1024f // Very fast fade for tooltips
        };

        // Position near mouse with small offset
        tooltip.PositionNear(mousePosition, new Vector2(10, -tooltip.Size.Y - 5), screenBounds);
        
        return tooltip;
    }
}
