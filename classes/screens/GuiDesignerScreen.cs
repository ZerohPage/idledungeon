using Raylib_cs;
using System.Numerics;
using RaylibGame.Classes.Gui;

namespace RaylibGame.Classes;

/// <summary>
/// Screen for designing and testing GUI layouts and components
/// </summary>
public class GuiDesignerScreen : Screen
{
    private List<Control> _designControls;
    private Control? _selectedControl;
    private Vector2 _mousePosition;
    private Vector2 _dragOffset;
    private bool _isDragging;
    private bool _showGrid;
    private bool _showRulers;
    private int _gridSize = 20;
    
    // Sample controls for testing
    private Button _testButton1 = null!;
    private Button _testButton2 = null!;
    private Button _testButton3 = null!;
    private Button _backButton = null!;
    private Button _addButtonControl = null!;
    private Button _addLabelControl = null!;
    private Button _toggleGridButton = null!;
    private Button _toggleRulersButton = null!;

    public GuiDesignerScreen(GameManager gameManager) : base(gameManager)
    {
        _designControls = new List<Control>();
        _showGrid = true;
        _showRulers = true;
        
        InitializeDesignerControls();
        CreateSampleControls();
    }

    private void InitializeDesignerControls()
    {
        // Designer toolbar buttons
        _backButton = new Button(GameManager,
            new Vector2(10, 10),
            new Vector2(80, 30),
            "Back");
        _backButton.AutoFitText(8);
        _backButton.OnClick += () => GameManager.SetGameState(GameState.Intro);

        _addButtonControl = new Button(GameManager,
            new Vector2(100, 10),
            new Vector2(100, 30),
            "Add Button");
        _addButtonControl.AutoFitText(8);
        _addButtonControl.OnClick += AddNewButton;

        _addLabelControl = new Button(GameManager,
            new Vector2(210, 10),
            new Vector2(90, 30),
            "Add Label");
        _addLabelControl.AutoFitText(8);
        _addLabelControl.OnClick += AddNewLabel;

        _toggleGridButton = new Button(GameManager,
            new Vector2(310, 10),
            new Vector2(100, 30),
            "Toggle Grid");
        _toggleGridButton.AutoFitText(8);
        _toggleGridButton.OnClick += () => _showGrid = !_showGrid;

        _toggleRulersButton = new Button(GameManager,
            new Vector2(420, 10),
            new Vector2(100, 30),
            "Toggle Rulers");
        _toggleRulersButton.AutoFitText(8);
        _toggleRulersButton.OnClick += () => _showRulers = !_showRulers;
    }

    private void CreateSampleControls()
    {
        // Create some sample controls for testing
        _testButton1 = new Button(GameManager,
            new Vector2(200, 100),
            new Vector2(120, 40),
            "Test Button 1");
        _testButton1.AutoFitText(8); // Auto-fit text with 8px padding
        
        _testButton2 = new Button(GameManager,
            new Vector2(350, 150),
            new Vector2(100, 35),
            "Button 2");
        _testButton2.AutoFitText(8);
            
        _testButton3 = new Button(GameManager,
            new Vector2(150, 250),
            new Vector2(140, 45),
            "Large Button");
        _testButton3.AutoFitText(8);

        // Add sample labels
        var sampleSmallLabel = Label.CreateSmallLabel(GameManager,
            new Vector2(200, 200),
            "Small Label");
        sampleSmallLabel.AutoFitText();

        var sampleMediumLabel = Label.CreateMediumLabel(GameManager,
            new Vector2(350, 220),
            "Medium Label");
        sampleMediumLabel.AutoFitText();

        var sampleLargeLabel = Label.CreateLargeLabel(GameManager,
            new Vector2(150, 320),
            "Large Label Text");
        sampleLargeLabel.AutoFitText();

        _designControls.Add(_testButton1);
        _designControls.Add(_testButton2);
        _designControls.Add(_testButton3);
        _designControls.Add(sampleSmallLabel);
        _designControls.Add(sampleMediumLabel);
        _designControls.Add(sampleLargeLabel);
    }

    private void AddNewButton()
    {
        var newButton = new Button(GameManager,
            new Vector2(100 + _designControls.Count * 20, 100 + _designControls.Count * 20),
            new Vector2(100, 30),
            $"Button {_designControls.Count + 1}");
        
        // Auto-fit the text to the button size
        newButton.AutoFitText(8);
        
        _designControls.Add(newButton);
    }

    private void AddNewLabel()
    {
        var newLabel = Label.CreateMediumLabel(GameManager,
            new Vector2(120 + _designControls.Count * 20, 120 + _designControls.Count * 20),
            $"Label {_designControls.Count + 1}");
        
        // Auto-fit the text to the label size
        newLabel.AutoFitText();
        
        _designControls.Add(newLabel);
    }

    public override void Update(float deltaTime)
    {
        _mousePosition = Raylib.GetMousePosition();

        // Update designer toolbar
        _backButton.Update(deltaTime);
        _addButtonControl.Update(deltaTime);
        _addLabelControl.Update(deltaTime);
        _toggleGridButton.Update(deltaTime);
        _toggleRulersButton.Update(deltaTime);

        // Handle control selection and dragging
        HandleControlInteraction();

        // Update all design controls
        foreach (var control in _designControls)
        {
            control.Update(deltaTime);
        }

        // Handle keyboard shortcuts
        HandleKeyboardShortcuts();
    }

    private void HandleControlInteraction()
    {
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            _selectedControl = GetControlAtPosition(_mousePosition);
            if (_selectedControl != null)
            {
                _isDragging = true;
                _dragOffset = _mousePosition - _selectedControl.Position;
            }
            else
            {
                _selectedControl = null;
                _isDragging = false;
            }
        }

        if (Raylib.IsMouseButtonReleased(MouseButton.Left))
        {
            _isDragging = false;
        }

        // Drag selected control
        if (_isDragging && _selectedControl != null)
        {
            Vector2 newPosition = _mousePosition - _dragOffset;
            
            // Snap to grid if enabled
            if (_showGrid && Raylib.IsKeyDown(KeyboardKey.LeftControl))
            {
                newPosition.X = (float)(Math.Round(newPosition.X / _gridSize) * _gridSize);
                newPosition.Y = (float)(Math.Round(newPosition.Y / _gridSize) * _gridSize);
            }
            
            _selectedControl.Position = newPosition;
        }
    }

    private Control? GetControlAtPosition(Vector2 position)
    {
        // Check from back to front (last added is on top)
        for (int i = _designControls.Count - 1; i >= 0; i--)
        {
            var control = _designControls[i];
            var bounds = new Rectangle(
                control.Position.X,
                control.Position.Y,
                control.Size.X,
                control.Size.Y);
                
            if (Raylib.CheckCollisionPointRec(position, bounds))
            {
                return control;
            }
        }
        return null;
    }

    private void HandleKeyboardShortcuts()
    {
        // Delete selected control
        if (_selectedControl != null && Raylib.IsKeyPressed(KeyboardKey.Delete))
        {
            _designControls.Remove(_selectedControl);
            _selectedControl = null;
        }

        // Toggle grid with G key
        if (Raylib.IsKeyPressed(KeyboardKey.G))
        {
            _showGrid = !_showGrid;
        }

        // Toggle rulers with R key
        if (Raylib.IsKeyPressed(KeyboardKey.R))
        {
            _showRulers = !_showRulers;
        }

        // Auto-fit text with F key
        if (_selectedControl != null && Raylib.IsKeyPressed(KeyboardKey.F))
        {
            if (_selectedControl is Button button)
            {
                button.AutoFitText(8);
            }
            else if (_selectedControl is Label label)
            {
                label.AutoFitText();
            }
        }

        // Duplicate control with Ctrl+D
        if (_selectedControl != null && Raylib.IsKeyDown(KeyboardKey.LeftControl) && Raylib.IsKeyPressed(KeyboardKey.D))
        {
            if (_selectedControl is Button button)
            {
                var duplicate = new Button(GameManager,
                    _selectedControl.Position + new Vector2(20, 20),
                    _selectedControl.Size,
                    button.Text + " Copy");
                duplicate.AutoFitText(8); // Auto-fit the duplicated button text
                _designControls.Add(duplicate);
                _selectedControl = duplicate;
            }
            else if (_selectedControl is Label label)
            {
                var duplicate = new Label(GameManager,
                    _selectedControl.Position + new Vector2(20, 20),
                    _selectedControl.Size,
                    label.Text + " Copy");
                duplicate.FontSize = label.FontSize;
                duplicate.TextColor = label.TextColor;
                duplicate.Alignment = label.Alignment;
                duplicate.AutoFitText(); // Auto-fit the duplicated label text
                _designControls.Add(duplicate);
                _selectedControl = duplicate;
            }
        }
    }

    public override void Draw()
    {
        Raylib.ClearBackground(Color.DarkGray);

        // Draw grid
        if (_showGrid)
        {
            DrawGrid();
        }

        // Draw rulers
        if (_showRulers)
        {
            DrawRulers();
        }

        // Draw all design controls
        foreach (var control in _designControls)
        {
            control.Draw();
        }

        // Highlight selected control
        if (_selectedControl != null)
        {
            DrawSelectionHighlight(_selectedControl);
        }

        // Draw designer toolbar
        DrawDesignerToolbar();

        // Draw info panel
        DrawInfoPanel();

        // Draw mouse coordinates
        DrawMouseInfo();
    }

    private void DrawGrid()
    {
        var screenWidth = Raylib.GetScreenWidth();
        var screenHeight = Raylib.GetScreenHeight();
        var gridColor = Color.Gray;
        gridColor.A = 50; // Semi-transparent

        // Vertical lines
        for (int x = 0; x < screenWidth; x += _gridSize)
        {
            Raylib.DrawLine(x, 0, x, screenHeight, gridColor);
        }

        // Horizontal lines
        for (int y = 0; y < screenHeight; y += _gridSize)
        {
            Raylib.DrawLine(0, y, screenWidth, y, gridColor);
        }
    }

    private void DrawRulers()
    {
        var screenWidth = Raylib.GetScreenWidth();
        var screenHeight = Raylib.GetScreenHeight();
        var rulerColor = Color.LightGray;

        // Top ruler
        Raylib.DrawRectangle(0, 0, screenWidth, 20, rulerColor);
        for (int x = 0; x < screenWidth; x += 50)
        {
            Raylib.DrawLine(x, 0, x, 20, Color.Black);
            FontManager.DrawText(x.ToString(), x + 2, 2, 10, Color.Black, FontType.UI);
        }

        // Left ruler
        Raylib.DrawRectangle(0, 0, 20, screenHeight, rulerColor);
        for (int y = 0; y < screenHeight; y += 50)
        {
            Raylib.DrawLine(0, y, 20, y, Color.Black);
            if (y > 20) // Don't overlap with top ruler
            {
                FontManager.DrawText(y.ToString(), 2, y + 2, 10, Color.Black, FontType.UI);
            }
        }
    }

    private void DrawSelectionHighlight(Control control)
    {
        var bounds = new Rectangle(
            control.Position.X - 2,
            control.Position.Y - 2,
            control.Size.X + 4,
            control.Size.Y + 4);
            
        Raylib.DrawRectangleLinesEx(bounds, 2, Color.Yellow);

        // Draw resize handles
        var handleSize = 6;
        var handles = new[]
        {
            new Vector2(bounds.X, bounds.Y), // Top-left
            new Vector2(bounds.X + bounds.Width, bounds.Y), // Top-right
            new Vector2(bounds.X, bounds.Y + bounds.Height), // Bottom-left
            new Vector2(bounds.X + bounds.Width, bounds.Y + bounds.Height) // Bottom-right
        };

        foreach (var handle in handles)
        {
            Raylib.DrawRectangleV(
                new Vector2(handle.X - handleSize / 2, handle.Y - handleSize / 2),
                new Vector2(handleSize, handleSize),
                Color.Yellow);
        }
    }

    private void DrawDesignerToolbar()
    {
        // Toolbar background
        Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), 50, Color.Black);
        
        // Draw toolbar buttons
        _backButton.Draw();
        _addButtonControl.Draw();
        _addLabelControl.Draw();
        _toggleGridButton.Draw();
        _toggleRulersButton.Draw();
    }

    private void DrawInfoPanel()
    {
        var panelX = Raylib.GetScreenWidth() - 250;
        var panelY = 60;
        var panelWidth = 240;
        var panelHeight = 200;

        Raylib.DrawRectangle(panelX, panelY, panelWidth, panelHeight, Color.Black);
        Raylib.DrawRectangleLines(panelX, panelY, panelWidth, panelHeight, Color.White);

        int yOffset = panelY + 10;
        FontManager.DrawText("GUI Designer Info", panelX + 10, yOffset, 14, Color.White, FontType.UI);
        yOffset += 25;

        FontManager.DrawText($"Controls: {_designControls.Count}", panelX + 10, yOffset, 12, Color.LightGray, FontType.UI);
        yOffset += 20;

        if (_selectedControl != null)
        {
            FontManager.DrawText("Selected Control:", panelX + 10, yOffset, 12, Color.Yellow, FontType.UI);
            yOffset += 15;
            FontManager.DrawText($"Pos: ({_selectedControl.Position.X:F0}, {_selectedControl.Position.Y:F0})", panelX + 10, yOffset, 10, Color.White, FontType.UI);
            yOffset += 15;
            FontManager.DrawText($"Size: ({_selectedControl.Size.X:F0}, {_selectedControl.Size.Y:F0})", panelX + 10, yOffset, 10, Color.White, FontType.UI);
            yOffset += 15;
            
            if (_selectedControl is Button button)
            {
                FontManager.DrawText($"Button: {button.Text}", panelX + 10, yOffset, 10, Color.White, FontType.UI);
                yOffset += 15;
                FontManager.DrawText($"Font Size: {button.FontSize}", panelX + 10, yOffset, 10, Color.White, FontType.UI);
            }
            else if (_selectedControl is Label label)
            {
                FontManager.DrawText($"Label: {label.Text}", panelX + 10, yOffset, 10, Color.White, FontType.UI);
                yOffset += 15;
                FontManager.DrawText($"Font Size: {label.FontSize}", panelX + 10, yOffset, 10, Color.White, FontType.UI);
                yOffset += 15;
                FontManager.DrawText($"Alignment: {label.Alignment}", panelX + 10, yOffset, 10, Color.White, FontType.UI);
            }
        }
        else
        {
            FontManager.DrawText("No control selected", panelX + 10, yOffset, 12, Color.Gray, FontType.UI);
        }

        yOffset += 30;
        FontManager.DrawText("Shortcuts:", panelX + 10, yOffset, 12, Color.SkyBlue, FontType.UI);
        yOffset += 15;
        FontManager.DrawText("G - Toggle Grid", panelX + 10, yOffset, 10, Color.White, FontType.UI);
        yOffset += 12;
        FontManager.DrawText("R - Toggle Rulers", panelX + 10, yOffset, 10, Color.White, FontType.UI);
        yOffset += 12;
        FontManager.DrawText("F - Auto-fit Text", panelX + 10, yOffset, 10, Color.White, FontType.UI);
        yOffset += 12;
        FontManager.DrawText("Del - Delete Selected", panelX + 10, yOffset, 10, Color.White, FontType.UI);
        yOffset += 12;
        FontManager.DrawText("Ctrl+D - Duplicate", panelX + 10, yOffset, 10, Color.White, FontType.UI);
        yOffset += 12;
        FontManager.DrawText("Ctrl+Drag - Snap to Grid", panelX + 10, yOffset, 10, Color.White, FontType.UI);
    }

    private void DrawMouseInfo()
    {
        var mouseText = $"Mouse: ({_mousePosition.X:F0}, {_mousePosition.Y:F0})";
        FontManager.DrawText(mouseText, 10, Raylib.GetScreenHeight() - 25, 12, Color.White, FontType.UI);
    }
}
