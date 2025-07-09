using System.Numerics;
using Raylib_cs;

namespace RaylibGame.Classes;

/// <summary>
/// Manages camera positioning and following behavior for the game world
/// </summary>
public class CameraManager
{
    private Vector2 _position;
    private Vector2 _target;
    private float _smoothing = 8.0f; // Higher value = more responsive following
    private Vector2 _screenCenter;
    private Vector2 _dungeonBounds;
    private bool _enableBoundaryClamp = true;
    
    public Vector2 Position => _position;
    public Vector2 Target => _target;
    public Vector2 Offset => -_position + _screenCenter;
    public float Smoothing 
    { 
        get => _smoothing; 
        set => _smoothing = Math.Max(0.1f, value); // Minimum smoothing to prevent issues
    }
    
    public CameraManager()
    {
        UpdateScreenCenter();
        _position = Vector2.Zero;
        _target = Vector2.Zero;
        _dungeonBounds = Vector2.Zero;
    }
    
    /// <summary>
    /// Sets the target position for the camera to follow
    /// </summary>
    /// <param name="target">World position to follow</param>
    public void SetTarget(Vector2 target)
    {
        _target = target;
    }
    
    /// <summary>
    /// Sets the dungeon boundaries for camera clamping
    /// </summary>
    /// <param name="dungeonWidth">Width of dungeon in pixels</param>
    /// <param name="dungeonHeight">Height of dungeon in pixels</param>
    public void SetDungeonBounds(float dungeonWidth, float dungeonHeight)
    {
        _dungeonBounds = new Vector2(dungeonWidth, dungeonHeight);
    }
    
    /// <summary>
    /// Enable or disable camera boundary clamping
    /// </summary>
    /// <param name="enabled">Whether to clamp camera to dungeon bounds</param>
    public void SetBoundaryClamp(bool enabled)
    {
        _enableBoundaryClamp = enabled;
    }
    
    /// <summary>
    /// Updates camera position with smooth following
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame</param>
    public void Update(float deltaTime)
    {
        // Update screen center in case window was resized
        UpdateScreenCenter();
        
        // Smooth camera following using interpolation
        float lerpFactor = 1.0f - (float)Math.Pow(0.5, _smoothing * deltaTime);
        _position = Vector2.Lerp(_position, _target, lerpFactor);
        
        // Clamp camera to dungeon boundaries if enabled
        if (_enableBoundaryClamp && _dungeonBounds != Vector2.Zero)
        {
            ClampToBounds();
        }
    }
    
    /// <summary>
    /// Instantly snap camera to target position (no smoothing)
    /// </summary>
    public void SnapToTarget()
    {
        _position = _target;
        
        if (_enableBoundaryClamp && _dungeonBounds != Vector2.Zero)
        {
            ClampToBounds();
        }
    }
    
    /// <summary>
    /// Converts world position to screen position
    /// </summary>
    /// <param name="worldPosition">Position in world coordinates</param>
    /// <returns>Position on screen</returns>
    public Vector2 WorldToScreen(Vector2 worldPosition)
    {
        return worldPosition + Offset;
    }
    
    /// <summary>
    /// Converts screen position to world position
    /// </summary>
    /// <param name="screenPosition">Position on screen</param>
    /// <returns>Position in world coordinates</returns>
    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        return screenPosition - Offset;
    }
    
    /// <summary>
    /// Checks if a world position is visible on screen
    /// </summary>
    /// <param name="worldPosition">World position to check</param>
    /// <param name="margin">Extra margin around screen edges</param>
    /// <returns>True if position is visible</returns>
    public bool IsPositionVisible(Vector2 worldPosition, float margin = 0)
    {
        Vector2 screenPos = WorldToScreen(worldPosition);
        return screenPos.X >= -margin && 
               screenPos.X <= Raylib.GetScreenWidth() + margin &&
               screenPos.Y >= -margin && 
               screenPos.Y <= Raylib.GetScreenHeight() + margin;
    }
    
    private void UpdateScreenCenter()
    {
        _screenCenter = new Vector2(Raylib.GetScreenWidth() / 2.0f, Raylib.GetScreenHeight() / 2.0f);
    }
    
    private void ClampToBounds()
    {
        float halfScreenWidth = _screenCenter.X;
        float halfScreenHeight = _screenCenter.Y;
        
        // Clamp camera position to keep the view within dungeon bounds
        _position.X = Math.Max(halfScreenWidth, Math.Min(_dungeonBounds.X - halfScreenWidth, _position.X));
        _position.Y = Math.Max(halfScreenHeight, Math.Min(_dungeonBounds.Y - halfScreenHeight, _position.Y));
        
        // Handle cases where dungeon is smaller than screen
        if (_dungeonBounds.X < Raylib.GetScreenWidth())
        {
            _position.X = _dungeonBounds.X / 2.0f;
        }
        
        if (_dungeonBounds.Y < Raylib.GetScreenHeight())
        {
            _position.Y = _dungeonBounds.Y / 2.0f;
        }
    }
}
