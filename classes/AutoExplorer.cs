using System.Numerics;

namespace RaylibGame.Classes;

public class AutoExplorer
{
    private Vector2 _currentDirection;
    private int _stepsInDirection;
    private int _maxStepsInDirection = 8;
    private bool _needsNewDirection;
    private Random _random;
    
    // Smooth rotation properties
    private float _currentFacingAngle = 0.0f; // Current facing angle in radians
    private float _targetFacingAngle = 0.0f;  // Target angle to rotate towards
    private float _rotationSpeed = 8.0f;     // How fast to rotate (radians per second)
    
    private HashSet<Vector2> _visitedPositions;
    private HashSet<Vector2> _reachablePositions;
    private bool _inHuntMode;
    private Vector2 _huntTarget;
    private Queue<Vector2> _huntPath;
    
    private Queue<Vector2> _recentPositions;
    private int _maxRecentPositions = 6;
    private int _stuckThreshold = 4;
    private int _huntAttempts = 0;
    private int _maxHuntAttempts = 3;
    private bool _explorationComplete = false;
    
    public Vector2 CurrentDirection => GetCurrentFacingDirection();
    
    public AutoExplorer()
    {
        _random = new Random();
        _visitedPositions = new HashSet<Vector2>();
        _reachablePositions = new HashSet<Vector2>();
        _recentPositions = new Queue<Vector2>();
        _huntPath = new Queue<Vector2>();
        Reset();
    }
    
    public void Reset()
    {
        _currentDirection = Vector2.Zero;
        _currentFacingAngle = 0.0f;
        _targetFacingAngle = 0.0f;
        _stepsInDirection = 0;
        _needsNewDirection = true;
        _visitedPositions.Clear();
        _reachablePositions.Clear();
        _recentPositions.Clear();
        _inHuntMode = false;
        _huntTarget = Vector2.Zero;
        _huntPath.Clear();
        _huntAttempts = 0;
        _explorationComplete = false;
    }
    
    public Vector2 GetNextMove(Vector2 currentGridPosition, Dungeon dungeon)
    {
        if (dungeon == null) return Vector2.Zero;

        // Pre-scan the dungeon on first call to identify all reachable positions
        if (_reachablePositions.Count == 0)
        {
            ScanReachableAreas(currentGridPosition, dungeon);
        }

        _visitedPositions.Add(currentGridPosition);
        
        _recentPositions.Enqueue(currentGridPosition);
        if (_recentPositions.Count > _maxRecentPositions)
        {
            _recentPositions.Dequeue();
        }
        
        bool isStuck = IsStuckInLoop(currentGridPosition);
        if (isStuck && !_inHuntMode)
        {
            return StartHuntPhase(currentGridPosition, dungeon);
        }
        
        if (_inHuntMode && _huntAttempts > _maxHuntAttempts)
        {
            _inHuntMode = false;
            _huntTarget = Vector2.Zero;
            _huntAttempts = 0;
            _explorationComplete = IsExplorationComplete();
            _needsNewDirection = true;
        }

        // Check if exploration is actually complete based on reachable positions
        if (!_explorationComplete && IsExplorationComplete())
        {
            _explorationComplete = true;
        }

        if (_explorationComplete)
        {
            return ChooseRandomDirection(currentGridPosition, dungeon);
        }

        if (_inHuntMode && _huntTarget != Vector2.Zero)
        {
            if (currentGridPosition == _huntTarget)
            {
                _inHuntMode = false;
                _huntTarget = Vector2.Zero;
                _huntPath.Clear();
                _huntAttempts = 0;
                _needsNewDirection = true;
            }
            else
            {
                // If we have a path, follow it; otherwise try to create a new path
                if (_huntPath.Count == 0)
                {
                    var path = FindPathToTarget(currentGridPosition, _huntTarget, dungeon);
                    if (path.Count > 0)
                    {
                        // Add path steps to queue (skip first step which is current position)
                        foreach (var step in path.Skip(1))
                        {
                            _huntPath.Enqueue(step);
                        }
                    }
                    else
                    {
                        // No path found, try a different target
                        return StartHuntPhase(currentGridPosition, dungeon);
                    }
                }
                
                if (_huntPath.Count > 0)
                {
                    var nextStep = _huntPath.Peek();
                    Vector2 direction = nextStep - currentGridPosition;
                    
                    if (IsValidGridPosition(nextStep, dungeon) && dungeon.IsWalkable((int)nextStep.X, (int)nextStep.Y))
                    {
                        _huntPath.Dequeue(); // Remove the step we're about to take
                        
                        if (nextStep == _huntTarget)
                        {
                            _inHuntMode = false;
                            _huntTarget = Vector2.Zero;
                            _huntPath.Clear();
                            _huntAttempts = 0;
                            _needsNewDirection = true;
                        }
                        SetTargetDirection(direction);
                        return direction;
                    }
                    else
                    {
                        // Path is blocked, clear it and try again
                        _huntPath.Clear();
                        return StartHuntPhase(currentGridPosition, dungeon);
                    }
                }
                else
                {
                    // No valid path, try different target
                    return StartHuntPhase(currentGridPosition, dungeon);
                }
            }
        }

        if (!_inHuntMode)
        {
            if (_needsNewDirection || _stepsInDirection >= _maxStepsInDirection)
            {
                Vector2 newDirection = ChooseRandomDirection(currentGridPosition, dungeon);
                SetTargetDirection(newDirection);
                _stepsInDirection = 0;
                _needsNewDirection = false;
            }

            if (_currentDirection != Vector2.Zero)
            {
                Vector2 nextPos = currentGridPosition + _currentDirection;
                if (IsValidGridPosition(nextPos, dungeon) && dungeon.IsWalkable((int)nextPos.X, (int)nextPos.Y))
                {
                    SetTargetDirection(_currentDirection);
                    return _currentDirection;
                }
                else
                {
                    return StartHuntPhase(currentGridPosition, dungeon);
                }
            }
            else
            {
                return StartHuntPhase(currentGridPosition, dungeon);
            }
        }

        return Vector2.Zero;
    }
    
    public void OnMoveSuccessful()
    {
        _stepsInDirection++;
    }
    
    public void OnMoveBlocked()
    {
        _needsNewDirection = true;
        _stepsInDirection = 0;
    }

    private Vector2 StartHuntPhase(Vector2 currentGridPosition, Dungeon dungeon)
    {
        _inHuntMode = true;
        _huntPath.Clear(); // Clear any existing path
        
        if (_huntAttempts >= _maxHuntAttempts)
        {
            // First try nearest, then furthest as last resort
            Vector2 lastResortTarget = FindNearestUnexploredArea(currentGridPosition, dungeon);
            if (lastResortTarget == Vector2.Zero)
            {
                lastResortTarget = FindFurthestUnexploredArea(currentGridPosition, dungeon);
            }
            
            if (lastResortTarget != Vector2.Zero)
            {
                var path = FindPathToTarget(currentGridPosition, lastResortTarget, dungeon);
                if (path.Count > 0)
                {
                    _huntTarget = lastResortTarget;
                    _huntAttempts = 0;
                    
                    // Add path to queue (skip current position)
                    foreach (var step in path.Skip(1))
                    {
                        _huntPath.Enqueue(step);
                    }
                    
                    if (_huntPath.Count > 0)
                    {
                        var nextStep = _huntPath.Dequeue();
                        Vector2 direction = nextStep - currentGridPosition;
                        return direction;
                    }
                }
            }
            
            _explorationComplete = IsExplorationComplete();
            _inHuntMode = false;
            _huntTarget = Vector2.Zero;
            _huntPath.Clear();
            _huntAttempts = 0;
            return Vector2.Zero;
        }

        _huntAttempts++;
        _huntTarget = FindNearestUnexploredArea(currentGridPosition, dungeon);
        
        if (_huntTarget != Vector2.Zero)
        {
            var path = FindPathToTarget(currentGridPosition, _huntTarget, dungeon);
            if (path.Count > 0)
            {
                // Add path to queue (skip current position)
                foreach (var step in path.Skip(1))
                {
                    _huntPath.Enqueue(step);
                }
                
                if (_huntPath.Count > 0)
                {
                    var nextStep = _huntPath.Dequeue();
                    Vector2 direction = nextStep - currentGridPosition;
                    return direction;
                }
            }
        }
        else
        {
            _explorationComplete = IsExplorationComplete();
            _huntAttempts = 0;
        }
        
        _inHuntMode = false;
        _huntTarget = Vector2.Zero;
        _huntPath.Clear();
        _needsNewDirection = true;
        return Vector2.Zero;
    }

    private void ScanReachableAreas(Vector2 startPosition, Dungeon dungeon)
    {
        var queue = new Queue<Vector2>();
        queue.Enqueue(startPosition);
        _reachablePositions.Add(startPosition);

        var directions = new Vector2[] { Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY };
        
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var dir in directions)
            {
                var next = current + dir;
                
                if (!_reachablePositions.Contains(next) && 
                    IsValidGridPosition(next, dungeon) &&
                    dungeon.IsWalkable((int)next.X, (int)next.Y))
                {
                    _reachablePositions.Add(next);
                    queue.Enqueue(next);
                }
            }
        }
    }

    private Vector2 FindNearestUnexploredArea(Vector2 currentGridPosition, Dungeon dungeon, int maxRadius = -1)
    {
        if (maxRadius == -1)
            maxRadius = Math.Max(dungeon.Width, dungeon.Height);
        
        for (int radius = 1; radius <= maxRadius; radius++)
        {
            for (int x = (int)currentGridPosition.X - radius; x <= (int)currentGridPosition.X + radius; x++)
            {
                for (int y = (int)currentGridPosition.Y - radius; y <= (int)currentGridPosition.Y + radius; y++)
                {
                    if (Math.Abs(x - currentGridPosition.X) != radius && Math.Abs(y - currentGridPosition.Y) != radius)
                        continue;
                        
                    Vector2 checkPos = new Vector2(x, y);
                    
                    // Use pre-scanned reachable positions instead of expensive reachability check
                    if (_reachablePositions.Contains(checkPos) && 
                        !_visitedPositions.Contains(checkPos))
                    {
                        return checkPos;
                    }
                }
            }
        }
        
        return Vector2.Zero;
    }

    private Vector2 FindFurthestUnexploredArea(Vector2 currentGridPosition, Dungeon dungeon)
    {
        Vector2 furthestTarget = Vector2.Zero;
        float maxDistance = 0;
        
        // Look for the furthest unexplored area - sometimes breaking out of local areas requires going far
        foreach (var pos in _reachablePositions)
        {
            if (!_visitedPositions.Contains(pos))
            {
                float distance = Vector2.Distance(currentGridPosition, pos);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    furthestTarget = pos;
                }
            }
        }
        
        return furthestTarget;
    }

    private Vector2 ChooseRandomDirection(Vector2 currentGridPosition, Dungeon dungeon)
    {
        Vector2[] allDirections = {
            new Vector2(0, -1), // Up
            new Vector2(1, 0),  // Right
            new Vector2(0, 1),  // Down
            new Vector2(-1, 0)  // Left
        };

        List<Vector2> validDirections = new List<Vector2>();
        List<Vector2> unvisitedDirections = new List<Vector2>();

        foreach (var dir in allDirections)
        {
            Vector2 testPos = currentGridPosition + dir;
            
            if (IsValidGridPosition(testPos, dungeon))
            {
                bool isWalkable = dungeon.IsWalkable((int)testPos.X, (int)testPos.Y);
                bool isVisited = _visitedPositions.Contains(testPos);
                
                if (isWalkable)
                {
                    validDirections.Add(dir);
                    
                    if (!isVisited)
                    {
                        unvisitedDirections.Add(dir);
                    }
                }
            }
        }

        if (unvisitedDirections.Count > 0)
        {
            return unvisitedDirections[_random.Next(unvisitedDirections.Count)];
        }

        if (validDirections.Count > 2 && _visitedPositions.Count > 20)
        {
            return Vector2.Zero;
        }

        if (validDirections.Count > 0)
        {
            return validDirections[_random.Next(validDirections.Count)];
        }

        return Vector2.Zero;
    }

    private bool IsStuckInLoop(Vector2 currentPosition)
    {
        if (_recentPositions.Count < _stuckThreshold) return false;
        
        int occurrences = _recentPositions.Count(pos => pos == currentPosition);
        
        return occurrences >= _stuckThreshold;
    }

    private bool IsValidGridPosition(Vector2 gridPos, Dungeon dungeon)
    {
        return gridPos.X >= 0 && gridPos.X < dungeon.Width &&
               gridPos.Y >= 0 && gridPos.Y < dungeon.Height;
    }

    private bool IsExplorationComplete()
    {
        // Exploration is complete when all reachable positions have been visited
        return _visitedPositions.Count >= _reachablePositions.Count;
    }

    public int GetExplorationProgress()
    {
        if (_reachablePositions.Count == 0) return 0;
        return (int)((float)_visitedPositions.Count / _reachablePositions.Count * 100);
    }

    public HashSet<Vector2> GetReachablePositions()
    {
        return _reachablePositions;
    }

    private List<Vector2> FindPathToTarget(Vector2 from, Vector2 to, Dungeon dungeon)
    {
        // Simple A* pathfinding implementation
        var openSet = new PriorityQueue<Vector2, float>();
        var cameFrom = new Dictionary<Vector2, Vector2>();
        var gScore = new Dictionary<Vector2, float>();
        var fScore = new Dictionary<Vector2, float>();
        
        gScore[from] = 0;
        fScore[from] = Vector2.Distance(from, to);
        openSet.Enqueue(from, fScore[from]);
        
        var directions = new Vector2[] { Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY };
        
        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            
            if (current == to)
            {
                // Reconstruct path
                var path = new List<Vector2>();
                var step = to;
                while (cameFrom.ContainsKey(step))
                {
                    path.Add(step);
                    step = cameFrom[step];
                }
                path.Reverse();
                return path;
            }
            
            foreach (var dir in directions)
            {
                var neighbor = current + dir;
                
                if (!IsValidGridPosition(neighbor, dungeon) || 
                    !dungeon.IsWalkable((int)neighbor.X, (int)neighbor.Y))
                    continue;
                
                float tentativeGScore = gScore[current] + 1;
                
                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Vector2.Distance(neighbor, to);
                    
                    if (!openSet.UnorderedItems.Any(item => item.Element.Equals(neighbor)))
                    {
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }
        }
        
        return new List<Vector2>(); // No path found
    }

    /// <summary>
    /// Updates the smooth rotation towards the target angle
    /// </summary>
    public void UpdateRotation(float deltaTime)
    {
        // Smoothly interpolate current angle towards target angle
        float angleDifference = NormalizeAngle(_targetFacingAngle - _currentFacingAngle);
        
        // If we're close enough, snap to target
        if (Math.Abs(angleDifference) < 0.1f)
        {
            _currentFacingAngle = _targetFacingAngle;
        }
        else
        {
            // Rotate towards target at rotation speed
            float rotationStep = _rotationSpeed * deltaTime;
            if (angleDifference > 0)
            {
                _currentFacingAngle += Math.Min(rotationStep, angleDifference);
            }
            else
            {
                _currentFacingAngle -= Math.Min(rotationStep, -angleDifference);
            }
        }
        
        // Keep angle in valid range
        _currentFacingAngle = NormalizeAngle(_currentFacingAngle);
    }
    
    /// <summary>
    /// Gets the current facing direction as a normalized vector
    /// </summary>
    private Vector2 GetCurrentFacingDirection()
    {
        return new Vector2((float)Math.Cos(_currentFacingAngle), (float)Math.Sin(_currentFacingAngle));
    }
    
    /// <summary>
    /// Sets the target direction and updates the target angle for smooth rotation
    /// </summary>
    private void SetTargetDirection(Vector2 direction)
    {
        if (direction != Vector2.Zero)
        {
            _currentDirection = direction;
            _targetFacingAngle = (float)Math.Atan2(direction.Y, direction.X);
        }
    }
    
    /// <summary>
    /// Normalizes an angle to be between -PI and PI
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        while (angle > Math.PI) angle -= 2.0f * (float)Math.PI;
        while (angle < -Math.PI) angle += 2.0f * (float)Math.PI;
        return angle;
    }
}
