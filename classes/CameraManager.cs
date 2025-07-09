using System.Numerics;
using Raylib_cs;

namespace RaylibGame.Classes;

/// <summary>
/// Static singleton camera manager using Raylib's Camera2D system
/// </summary>
public static class CameraManager
{
    private static Camera2D _camera;
    private static Vector2 _target;
    private static float _smoothing = 8.0f; // Higher value = more responsive following
    private static Vector2 _dungeonBounds;
    private static bool _enableBoundaryClamp = true;
    private static bool _initialized = false;
    
    public static Camera2D Camera => _camera;
    public static Vector2 Position => _camera.Target;
    public static Vector2 Target => _target;
    public static float Smoothing 
    { 
        get => _smoothing; 
        set => _smoothing = Math.Max(0.1f, value); // Minimum smoothing to prevent issues
    }
    
    /// <summary>
    /// Initialize the camera manager (call this once at game start)
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;
        
        _camera = new Camera2D
        {
            Target = Vector2.Zero,
            Offset = new Vector2(Raylib.GetScreenWidth() / 2.0f, Raylib.GetScreenHeight() / 2.0f),
            Rotation = 0.0f,
            Zoom = 1.0f
        };
        
        _target = Vector2.Zero;
        _dungeonBounds = Vector2.Zero;
        _initialized = true;
    }
    
    /// <summary>
    /// Sets the target position for the camera to follow
    /// </summary>
    /// <param name="target">World position to follow</param>
    public static void SetTarget(Vector2 target)
    {
        _target = target;
    }
    
    /// <summary>
    /// Sets the dungeon boundaries for camera clamping
    /// </summary>
    /// <param name="dungeonWidth">Width of dungeon in pixels</param>
    /// <param name="dungeonHeight">Height of dungeon in pixels</param>
    public static void SetDungeonBounds(float dungeonWidth, float dungeonHeight)
    {
        _dungeonBounds = new Vector2(dungeonWidth, dungeonHeight);
    }
    
    /// <summary>
    /// Enable or disable camera boundary clamping
    /// </summary>
    /// <param name="enabled">Whether to clamp camera to dungeon bounds</param>
    public static void SetBoundaryClamp(bool enabled)
    {
        _enableBoundaryClamp = enabled;
    }
    
    /// <summary>
    /// Updates camera position with smooth following
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame</param>
    public static void Update(float deltaTime)
    {
        if (!_initialized) Initialize();
        
        // Update camera offset in case window was resized
        _camera.Offset = new Vector2(Raylib.GetScreenWidth() / 2.0f, Raylib.GetScreenHeight() / 2.0f);
        
        // Smooth camera following using interpolation
        float lerpFactor = 1.0f - (float)Math.Pow(0.5, _smoothing * deltaTime);
        _camera.Target = Vector2.Lerp(_camera.Target, _target, lerpFactor);
        
        // Clamp camera to dungeon boundaries if enabled
        if (_enableBoundaryClamp && _dungeonBounds != Vector2.Zero)
        {
            ClampToBounds();
        }
    }
    
    /// <summary>
    /// Instantly snap camera to target position (no smoothing)
    /// </summary>
    public static void SnapToTarget()
    {
        if (!_initialized) Initialize();
        
        _camera.Target = _target;
        
        if (_enableBoundaryClamp && _dungeonBounds != Vector2.Zero)
        {
            ClampToBounds();
        }
    }
    
    /// <summary>
    /// Converts world position to screen position using Raylib's built-in method
    /// </summary>
    /// <param name="worldPosition">Position in world coordinates</param>
    /// <returns>Position on screen</returns>
    public static Vector2 WorldToScreen(Vector2 worldPosition)
    {
        if (!_initialized) Initialize();
        return Raylib.GetWorldToScreen2D(worldPosition, _camera);
    }
    
    /// <summary>
    /// Converts screen position to world position using Raylib's built-in method
    /// </summary>
    /// <param name="screenPosition">Position on screen</param>
    /// <returns>Position in world coordinates</returns>
    public static Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        if (!_initialized) Initialize();
        return Raylib.GetScreenToWorld2D(screenPosition, _camera);
    }
    
    /// <summary>
    /// Checks if a world position is visible on screen
    /// </summary>
    /// <param name="worldPosition">World position to check</param>
    /// <param name="margin">Extra margin around screen edges</param>
    /// <returns>True if position is visible</returns>
    public static bool IsPositionVisible(Vector2 worldPosition, float margin = 0)
    {
        if (!_initialized) Initialize();
        
        Vector2 screenPos = WorldToScreen(worldPosition);
        return screenPos.X >= -margin && 
               screenPos.X <= Raylib.GetScreenWidth() + margin &&
               screenPos.Y >= -margin && 
               screenPos.Y <= Raylib.GetScreenHeight() + margin;
    }
    
    /// <summary>
    /// Begin camera mode for world rendering
    /// </summary>
    public static void BeginMode()
    {
        if (!_initialized) Initialize();
        Raylib.BeginMode2D(_camera);
    }
    
    /// <summary>
    /// End camera mode
    /// </summary>
    public static void EndMode()
    {
        Raylib.EndMode2D();
    }
    
    private static void ClampToBounds()
    {
        float halfScreenWidth = _camera.Offset.X;
        float halfScreenHeight = _camera.Offset.Y;
        
        // Clamp camera target to keep the view within dungeon bounds
        var target = _camera.Target;
        target.X = Math.Max(halfScreenWidth, Math.Min(_dungeonBounds.X - halfScreenWidth, target.X));
        target.Y = Math.Max(halfScreenHeight, Math.Min(_dungeonBounds.Y - halfScreenHeight, target.Y));
        
        // Handle cases where dungeon is smaller than screen
        if (_dungeonBounds.X < Raylib.GetScreenWidth())
        {
            target.X = _dungeonBounds.X / 2.0f;
        }
        
        if (_dungeonBounds.Y < Raylib.GetScreenHeight())
        {
            target.Y = _dungeonBounds.Y / 2.0f;
        }
        
        _camera.Target = target;
    }
}
