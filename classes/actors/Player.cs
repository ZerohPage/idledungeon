using Raylib_cs;
using System.Numerics;

namespace RaylibGame.Classes;

public class Player
{
    private Vector2 _gridPosition; // Grid coordinates
    private Vector2 _worldPosition; // Pixel coordinates for drawing
    private float _moveTimer; // Timer for smooth movement animation
    private float _moveDuration = 0.01f; // Time to move between grid cells (150% faster for testing)
    private Vector2 _moveStartPos; // Starting position for smooth movement
    private Vector2 _moveEndPos; // Ending position for smooth movement
    private bool _isMoving;
    private float _radius;
    private Color _color;
    private Dungeon? _currentDungeon;
    private bool _isAutoExploring;
    private float _autoMoveTimer;
    private float _autoMoveCooldown = 0.01f; // Time between auto moves (150% faster for testing)
    private AutoExplorer _autoExplorer;
    private bool _showReachablePositions = false; // Toggle for visualization
    
    public Vector2 Position => _worldPosition;
    public Vector2 GridPosition => _gridPosition;
    public float Radius => _radius;
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    public bool IsAutoExploring { get => _isAutoExploring; set => _isAutoExploring = value; }
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
        if (Raylib.IsKeyPressed(KeyboardKey.Tab))
        {
            _isAutoExploring = !_isAutoExploring;
        }
        
        // Check for toggle to show reachable positions visualization
        if (Raylib.IsKeyPressed(KeyboardKey.R))
        {
            _showReachablePositions = !_showReachablePositions;
        }

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
        Vector2 inputDirection = Vector2.Zero;
        
        // Handle movement input
        if (Raylib.IsKeyPressed(KeyboardKey.W) || Raylib.IsKeyPressed(KeyboardKey.Up))
        {
            inputDirection.Y = -1.0f;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.S) || Raylib.IsKeyPressed(KeyboardKey.Down))
        {
            inputDirection.Y = 1.0f;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.A) || Raylib.IsKeyPressed(KeyboardKey.Left))
        {
            inputDirection.X = -1.0f;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.D) || Raylib.IsKeyPressed(KeyboardKey.Right))
        {
            inputDirection.X = 1.0f;
        }
        
        // Only move in one direction at a time for grid movement
        if (inputDirection != Vector2.Zero)
        {
            Vector2 targetPos = _gridPosition + inputDirection;
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
    
    public void Draw()
    {
        // Draw player circle
        Raylib.DrawCircleV(_worldPosition, _radius, _color);
        
        // Draw player outline
        Raylib.DrawCircleLinesV(_worldPosition, _radius, Color.White);
        
        // Draw target position when auto-exploring (show current direction)
        if (_isAutoExploring && _autoExplorer.CurrentDirection != Vector2.Zero)
        {
            // Show where we're heading (3 steps ahead)
            Vector2 futurePos = _gridPosition + _autoExplorer.CurrentDirection * 3;
            Vector2 targetWorldPos = new Vector2(futurePos.X * (_currentDungeon?.TileSize ?? 20) + (_currentDungeon?.TileSize ?? 20) / 2,
                                               futurePos.Y * (_currentDungeon?.TileSize ?? 20) + (_currentDungeon?.TileSize ?? 20) / 2);
            
            // Draw directional arrow
            Vector2 arrowStart = _worldPosition;
            Vector2 arrowEnd = _worldPosition + _autoExplorer.CurrentDirection * 30; // 30 pixels in direction
            
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
                                           _gridPosition.Y * _currentDungeon.TileSize + _currentDungeon.TileSize / 2);
            Raylib.DrawRectangleLinesEx(new Rectangle(
                _gridPosition.X * _currentDungeon.TileSize, 
                _gridPosition.Y * _currentDungeon.TileSize,
                _currentDungeon.TileSize, 
                _currentDungeon.TileSize), 1, Color.Yellow);
            
            // Draw red dots for reachable positions (from pre-scan)
            if (_isAutoExploring && _showReachablePositions)
            {
                var reachablePositions = _autoExplorer.GetReachablePositions();
                foreach (var pos in reachablePositions)
                {
                    Vector2 worldPos = new Vector2(pos.X * _currentDungeon.TileSize + _currentDungeon.TileSize / 2,
                                                 pos.Y * _currentDungeon.TileSize + _currentDungeon.TileSize / 2);
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
}
