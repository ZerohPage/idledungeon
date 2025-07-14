using Raylib_cs;
using System.Numerics;

namespace RaylibGame.Classes;

public class Player
{
    private Vector2 _gridPosition; // Grid coordinates
    private Vector2 _worldPosition; // Pixel coordinates for drawing
    private float _moveTimer; // Timer for smooth movement animation
    private float _moveDuration = 0.2f; // Time to move between grid cells (smooth movement)
    private Vector2 _moveStartPos; // Starting position for smooth movement
    private Vector2 _moveEndPos; // Ending position for smooth movement
    private bool _isMoving;
    private float _radius;
    private Color _color;
    private Dungeon? _currentDungeon;
    private bool _isAutoExploring;
    private float _autoMoveTimer;
    private float _autoMoveCooldown; // Time between auto moves - now dynamic
    private AutoExplorer _autoExplorer;
    private bool _showReachablePositions = false; // Toggle for visualization
    
    // Speed control settings
    private float _baseAutoMoveCooldown = 0.25f; // Base speed
    private float _minAutoMoveCooldown = 0.01f;  // Fastest speed (20 moves per second)
    private float _maxAutoMoveCooldown = 2.0f;   // Slowest speed (0.5 moves per second)
    private float _speedStep = 0.05f;            // How much to change speed per key press
    
    public Vector2 Position => _worldPosition;
    public Vector2 GridPosition => _gridPosition;
    public float Radius => _radius;
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    public bool IsAutoExploring { get => _isAutoExploring; set => _isAutoExploring = value; }
    public float ExplorationSpeed => 1.0f / _autoMoveCooldown; // Moves per second
    public Vector2 CurrentDirection => _autoExplorer.CurrentDirection;
    public InventoryManager Inventory { get; private set; }
    
    public Player(Vector2 startPosition)
    {
        // Convert start position to grid coordinates
        _gridPosition = new Vector2((int)(startPosition.X / 20), (int)(startPosition.Y / 20));
        _worldPosition = new Vector2(_gridPosition.X * 20 + 10, _gridPosition.Y * 20 + 10);
        _moveTimer = 0f;
        _moveStartPos = _worldPosition;
        _moveEndPos = _worldPosition;
        _isMoving = false;
        _radius = 8.0f;
        _color = Color.Blue;
        MaxHealth = 200;
        Health = MaxHealth;
        _isAutoExploring = true;
        _autoMoveTimer = 0f;
        _autoMoveCooldown = _baseAutoMoveCooldown; // Initialize with base speed
        _autoExplorer = new AutoExplorer();
        Inventory = new InventoryManager(20); // 20 slot inventory
    }
    
    public void SetDungeon(Dungeon dungeon)
    {
        _currentDungeon = dungeon;
        
        // Set player position to dungeon entrance if available
        if (dungeon.EntrancePosition != Vector2.Zero)
        {
            _gridPosition = new Vector2((int)(dungeon.EntrancePosition.X / dungeon.TileSize), 
                                      (int)(dungeon.EntrancePosition.Y / dungeon.TileSize));
            _worldPosition = new Vector2(_gridPosition.X * dungeon.TileSize + dungeon.TileSize / 2, 
                                       _gridPosition.Y * dungeon.TileSize + dungeon.TileSize / 2);
            
            // Reset exploration state for new dungeon
            _autoExplorer.Reset();
        }
    }
    
    public void Update(float deltaTime)
    {
        // Check for toggle between manual and auto exploration
        if (InputManager.IsAutoExploreTogglePressed)
        {
            _isAutoExploring = !_isAutoExploring;
        }
        
        // Check for toggle to show reachable positions visualization
        if (InputManager.IsReachablePositionsTogglePressed)
        {
            _showReachablePositions = !_showReachablePositions;
        }
        
        // Handle exploration speed controls
        HandleSpeedControls();

        // Handle smooth movement animation
        UpdateMovementAnimation(deltaTime);

        // Only process new moves if not currently moving
        if (!_isMoving)
        {
            if (_isAutoExploring)
            {
                HandleAutoExploration(deltaTime);
            }
            else
            {
                HandleInput();
            }
        }
    }

    private void UpdateMovementAnimation(float deltaTime)
    {
        if (_isMoving)
        {
            _moveTimer += deltaTime;
            float progress = Math.Min(_moveTimer / _moveDuration, 1.0f);
            
            // Smooth interpolation
            _worldPosition = Vector2.Lerp(_moveStartPos, _moveEndPos, progress);
            
            if (progress >= 1.0f)
            {
                _isMoving = false;
                _moveTimer = 0f;
                _worldPosition = _moveEndPos;
            }
        }
    }

    private void HandleAutoExploration(float deltaTime)
    {
        if (_currentDungeon == null) return;
        
        // Update smooth rotation for the auto explorer
        _autoExplorer.UpdateRotation(deltaTime);
        
        // Check if exploration is complete and turn off auto-exploration
        if (_autoExplorer.GetExplorationProgress() >= 100)
        {
            _isAutoExploring = false;
            return;
        }
        
        _autoMoveTimer -= deltaTime;
        
        if (_autoMoveTimer <= 0f)
        {
            Vector2 nextMove = _autoExplorer.GetNextMove(_gridPosition, _currentDungeon);
            
            if (nextMove != Vector2.Zero)
            {
                //Console.WriteLine($"[Player] Attempting move from ({_gridPosition.X}, {_gridPosition.Y}) with direction ({nextMove.X}, {nextMove.Y})");
                if (TryMoveToGridPosition(_gridPosition + nextMove))
                {
                    //Console.WriteLine($"[Player] Move successful! Now at ({_gridPosition.X}, {_gridPosition.Y})");
                    // Successfully moved in current direction
                    _autoExplorer.OnMoveSuccessful();
                }
                else
                {
                    //Console.WriteLine($"[Player] Move blocked! Still at ({_gridPosition.X}, {_gridPosition.Y})");
                    // Blocked, need new direction
                    _autoExplorer.OnMoveBlocked();
                }
            }
            else
            {
                //Console.WriteLine($"[Player] No valid move returned from AutoExplorer");
                // No valid move found, need new direction
                _autoExplorer.OnMoveBlocked();
            }
            
            _autoMoveTimer = _autoMoveCooldown;
        }
    }

    private void HandleInput()
    {
        // Use InputManager for movement
        if (InputManager.HasMovementInput)
        {
            Vector2 targetPos = _gridPosition + InputManager.MovementDirection;
            TryMoveToGridPosition(targetPos);
        }
    }

    private bool TryMoveToGridPosition(Vector2 gridPos)
    {
        if (_currentDungeon == null || _isMoving) 
        {
            //Console.WriteLine($"[Player] TryMove failed: dungeon={_currentDungeon != null}, moving={_isMoving}");
            return false;
        }
        
        bool isValid = IsValidGridPosition(gridPos);
        bool isWalkable = isValid ? _currentDungeon.IsWalkable((int)gridPos.X, (int)gridPos.Y) : false;
        
        //Console.WriteLine($"[Player] TryMove to ({gridPos.X}, {gridPos.Y}): Valid={isValid}, Walkable={isWalkable}");
        
        if (isValid && isWalkable)
        {
            _gridPosition = gridPos;
            
            // Start smooth movement animation
            _moveStartPos = _worldPosition;
            _moveEndPos = new Vector2(gridPos.X * _currentDungeon.TileSize + _currentDungeon.TileSize / 2,
                                    gridPos.Y * _currentDungeon.TileSize + _currentDungeon.TileSize / 2);
            _isMoving = true;
            _moveTimer = 0f;
            
            return true;
        }
        
        return false;
    }

    private bool IsValidGridPosition(Vector2 gridPos)
    {
        if (_currentDungeon == null) return false;
        
        return gridPos.X >= 0 && gridPos.X < _currentDungeon.Width &&
               gridPos.Y >= 0 && gridPos.Y < _currentDungeon.Height;
    }
    
    public void Draw(Vector2 cameraOffset = default)
    {
        // Apply camera offset to all world positions
        Vector2 screenPosition = _worldPosition + cameraOffset;
        
        // Draw player circle
        Raylib.DrawCircleV(screenPosition, _radius, _color);
        
        // Draw player outline
        Raylib.DrawCircleLinesV(screenPosition, _radius, Color.White);
        
        // Draw target position when auto-exploring (show current direction)
        if (_isAutoExploring && _autoExplorer.CurrentDirection != Vector2.Zero)
        {
            // Show where we're heading (3 steps ahead)
            Vector2 futurePos = _gridPosition + _autoExplorer.CurrentDirection * 3;
            Vector2 targetWorldPos = new Vector2(futurePos.X * (_currentDungeon?.TileSize ?? 20) + (_currentDungeon?.TileSize ?? 20) / 2,
                                               futurePos.Y * (_currentDungeon?.TileSize ?? 20) + (_currentDungeon?.TileSize ?? 20) / 2);
            
            // Draw directional arrow (apply camera offset)
            Vector2 arrowStart = screenPosition;
            Vector2 arrowEnd = screenPosition + _autoExplorer.CurrentDirection * 30; // 30 pixels in direction
            
            Raylib.DrawLineV(arrowStart, arrowEnd, Color.Orange);
            
            // Draw arrow head
            Vector2 arrowTip = arrowEnd;
            Vector2 perpendicular = new Vector2(-_autoExplorer.CurrentDirection.Y, _autoExplorer.CurrentDirection.X) * 5;
            Raylib.DrawLineV(arrowTip, arrowTip - _autoExplorer.CurrentDirection * 8 + perpendicular, Color.Orange);
            Raylib.DrawLineV(arrowTip, arrowTip - _autoExplorer.CurrentDirection * 8 - perpendicular, Color.Orange);
        }
        
        // Draw grid position indicator for debugging
        if (_currentDungeon != null)
        {
            Vector2 gridCenter = new Vector2(_gridPosition.X * _currentDungeon.TileSize + _currentDungeon.TileSize / 2,
                                           _gridPosition.Y * _currentDungeon.TileSize + _currentDungeon.TileSize / 2) + cameraOffset;
            Raylib.DrawRectangleLinesEx(new Rectangle(
                _gridPosition.X * _currentDungeon.TileSize + cameraOffset.X, 
                _gridPosition.Y * _currentDungeon.TileSize + cameraOffset.Y,
                _currentDungeon.TileSize, 
                _currentDungeon.TileSize), 1, Color.Yellow);
            
            // Draw red dots for reachable positions (from pre-scan)
            if (_isAutoExploring && _showReachablePositions)
            {
                var reachablePositions = _autoExplorer.GetReachablePositions();
                foreach (var pos in reachablePositions)
                {
                    Vector2 worldPos = new Vector2(pos.X * _currentDungeon.TileSize + _currentDungeon.TileSize / 2,
                                                 pos.Y * _currentDungeon.TileSize + _currentDungeon.TileSize / 2) + cameraOffset;
                    // Semi-transparent red dots
                    Raylib.DrawCircleV(worldPos, 2.0f, new Color(255, 0, 0, 128));
                }
            }
        }
    }
    
    public void DrawHealthBar(Vector2 position)
    {
        const int barWidth = 100;
        const int barHeight = 8;
        
        // Draw background
        Raylib.DrawRectangle((int)position.X, (int)position.Y, barWidth, barHeight, Color.DarkGray);
        
        // Draw health
        float healthPercentage = (float)Health / MaxHealth;
        int healthWidth = (int)(barWidth * healthPercentage);
        
        Color healthColor = healthPercentage > 0.6f ? Color.Green :
                           healthPercentage > 0.3f ? Color.Yellow : Color.Red;
        
        Raylib.DrawRectangle((int)position.X, (int)position.Y, healthWidth, barHeight, healthColor);
        
        // Draw border
        Raylib.DrawRectangleLines((int)position.X, (int)position.Y, barWidth, barHeight, Color.White);
        
        // Draw health text
        string healthText = $"{Health}/{MaxHealth}";
        Raylib.DrawText(healthText, (int)position.X + barWidth + 10, (int)position.Y - 2, 12, Color.White);
    }
    
    public void TakeDamage(int damage)
    {
        Health = Math.Max(0, Health - damage);
    }
    
    public void Heal(int amount)
    {
        Health = Math.Min(MaxHealth, Health + amount);
    }
    
    public bool IsAlive()
    {
        return Health > 0;
    }
    
    public void SetPosition(Vector2 position)
    {
        if (_currentDungeon != null)
        {
            _gridPosition = new Vector2((int)(position.X / _currentDungeon.TileSize), 
                                      (int)(position.Y / _currentDungeon.TileSize));
            _worldPosition = new Vector2(_gridPosition.X * _currentDungeon.TileSize + _currentDungeon.TileSize / 2,
                                       _gridPosition.Y * _currentDungeon.TileSize + _currentDungeon.TileSize / 2);
        }
        else
        {
            _gridPosition = new Vector2((int)(position.X / 20), (int)(position.Y / 20));
            _worldPosition = new Vector2(_gridPosition.X * 20 + 10, _gridPosition.Y * 20 + 10);
        }
    }
    
    public Vector2 GetTilePosition()
    {
        return _gridPosition;
    }

    private void HandleSpeedControls()
    {
        if (InputManager.IsSpeedUpPressed)
        {
            // Decrease cooldown to speed up (clamp to minimum)
            _autoMoveCooldown = Math.Max(_minAutoMoveCooldown, _autoMoveCooldown - _speedStep);
        }
        
        if (InputManager.IsSpeedDownPressed)
        {
            // Increase cooldown to slow down (clamp to maximum)  
            _autoMoveCooldown = Math.Min(_maxAutoMoveCooldown, _autoMoveCooldown + _speedStep);
        }
    }
}
