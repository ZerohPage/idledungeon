using System.Numerics;
using Raylib_cs;

namespace RaylibGame.Classes.Gui;

/// <summary>
/// Renders a pseudo 3D view using raycasting from the player's perspective
/// Shows what the explorer is seeing in a 300x300 pixel overlay
/// </summary>
public class RaycastRenderer
{
    private const int RENDER_WIDTH = 300;
    private const int RENDER_HEIGHT = 300;
    private const int NUM_RAYS = 150; // Number of rays to cast (half of width for performance)
    private const float FOV = 60.0f; // Field of view in degrees
    private const float MAX_DISTANCE = 5.0f; // Maximum ray distance in grid units (5 squares ahead)
    
    private RenderTexture2D _renderTexture;
    private Vector2 _position;
    private bool _isVisible;
    private Dungeon? _dungeon;
    
    // Debug info
    private Vector2 _lastPlayerPos;
    private Vector2 _lastPlayerDir;
    
    // Fog effect properties
    private float _fogAnimationTime;
    private const float FOG_START_DISTANCE = MAX_DISTANCE; // Start fog effect exactly at max distance
    private readonly Random _fogRandom = new Random();
    
    // Colors for different wall distances (darker = further)
    private readonly Color[] _wallColors = new Color[]
    {
        new Color(200, 200, 200, 255), // Close
        new Color(160, 160, 160, 255),
        new Color(120, 120, 120, 255),
        new Color(80, 80, 80, 255),    // Medium
        new Color(60, 60, 60, 255),
        new Color(40, 40, 40, 255),    // Far
        new Color(20, 20, 20, 255)     // Very far
    };
    
    public bool IsVisible 
    { 
        get => _isVisible; 
        set => _isVisible = value; 
    }
    
    public RaycastRenderer()
    {
        // Position in top-right corner with some padding
        _position = new Vector2(Raylib.GetScreenWidth() - RENDER_WIDTH - 20, 20);
        _isVisible = true;
        
        // Create render texture for the 3D view
        _renderTexture = Raylib.LoadRenderTexture(RENDER_WIDTH, RENDER_HEIGHT);
    }
    
    /// <summary>
    /// Updates the renderer position (in case of window resize)
    /// </summary>
    public void UpdatePosition()
    {
        _position = new Vector2(Raylib.GetScreenWidth() - RENDER_WIDTH - 20, 20);
    }
    
    /// <summary>
    /// Sets the dungeon reference for raycasting
    /// </summary>
    public void SetDungeon(Dungeon dungeon)
    {
        _dungeon = dungeon;
    }
    
    /// <summary>
    /// Updates the fog animation (call this every frame)
    /// </summary>
    public void UpdateFogAnimation(float deltaTime)
    {
        _fogAnimationTime += deltaTime * 2.0f; // Speed up the animation
    }
    
    /// <summary>
    /// Renders the pseudo 3D view from the player's perspective
    /// </summary>
    public void Render(Vector2 playerGridPosition, Vector2 playerDirection)
    {
        if (!_isVisible || _dungeon == null) return;
        
        // Store debug info
        _lastPlayerPos = playerGridPosition;
        _lastPlayerDir = playerDirection;
        
        // Begin rendering to texture
        Raylib.BeginTextureMode(_renderTexture);
        Raylib.ClearBackground(Color.Black);
        
        // Adjust player position to center of tile (add 0.5 to both coordinates)
        Vector2 playerPos = new Vector2(playerGridPosition.X + 0.5f, playerGridPosition.Y + 0.5f);
        
        // Calculate player facing direction
        float playerAngle = GetDirectionAngle(playerDirection);
        float halfFov = FOV * 0.5f * (float)(Math.PI / 180.0); // Convert to radians
        
        // Cast rays for each column
        for (int x = 0; x < NUM_RAYS; x++)
        {
            // Calculate ray angle
            float rayAngle = playerAngle - halfFov + (halfFov * 2.0f * x / NUM_RAYS);
            
            // Cast ray and get hit distance
            float hitDistance = CastRay(playerPos, rayAngle);
            
            // Fix fisheye effect by using perpendicular distance
            float angleDiff = rayAngle - playerAngle;
            float correctedDistance = hitDistance * (float)Math.Cos(angleDiff);
            
            // Calculate wall height based on corrected distance
            float wallHeight = RENDER_HEIGHT / Math.Max(correctedDistance, 0.1f) * 2.0f;
            wallHeight = Math.Min(wallHeight, RENDER_HEIGHT);
            
            // Calculate wall top and bottom
            float wallTop = (RENDER_HEIGHT - wallHeight) * 0.5f;
            float wallBottom = wallTop + wallHeight;
            
            // Get wall color based on corrected distance
            Color wallColor = GetWallColor(correctedDistance);
            
            // Draw vertical line for this ray (stretch across multiple pixels for better coverage)
            int pixelWidth = RENDER_WIDTH / NUM_RAYS;
            for (int px = 0; px < pixelWidth; px++)
            {
                int drawX = x * pixelWidth + px;
                if (drawX < RENDER_WIDTH)
                {
                    // If we hit the max distance (no wall found), draw fog instead of a wall
                    if (hitDistance >= MAX_DISTANCE)
                    {
                        Color fogColor = GetFogColor(correctedDistance, drawX, 0);
                        if (fogColor.A > 0)
                        {
                            // Draw fog over the entire column height for areas we can't see
                            Raylib.DrawLine(drawX, 0, drawX, RENDER_HEIGHT, fogColor);
                        }
                    }
                    else
                    {
                        // Draw the wall normally
                        Raylib.DrawLine(drawX, (int)wallTop, drawX, (int)wallBottom, wallColor);
                    }
                }
            }
        }
        
        // End rendering to texture
        Raylib.EndTextureMode();
    }
    
    /// <summary>
    /// Draws the rendered 3D view to the screen
    /// </summary>
    public void Draw()
    {
        if (!_isVisible) return;
        
        // Draw the render texture to screen (flipped vertically because render textures are flipped)
        Rectangle sourceRect = new Rectangle(0, 0, RENDER_WIDTH, -RENDER_HEIGHT);
        Rectangle destRect = new Rectangle(_position.X, _position.Y, RENDER_WIDTH, RENDER_HEIGHT);
        
        // Draw background panel
        Raylib.DrawRectangle((int)_position.X - 5, (int)_position.Y - 5, 
                           RENDER_WIDTH + 10, RENDER_HEIGHT + 10, Color.DarkGray);
        
        // Draw the 3D view
        Raylib.DrawTexturePro(_renderTexture.Texture, sourceRect, destRect, Vector2.Zero, 0.0f, Color.White);
        
        // Draw border
        Raylib.DrawRectangleLines((int)_position.X, (int)_position.Y, 
                                RENDER_WIDTH, RENDER_HEIGHT, Color.White);
        
        // Draw title and debug info
        FontManager.DrawText("3D View", (int)_position.X + 5, (int)_position.Y - 20, 16, Color.White, FontType.UI);
        
        // Debug info for troubleshooting
        string debugText = $"Pos: ({_lastPlayerPos.X:F1}, {_lastPlayerPos.Y:F1}) Dir: ({_lastPlayerDir.X:F1}, {_lastPlayerDir.Y:F1})";
        FontManager.DrawText(debugText, (int)_position.X + 5, (int)_position.Y + RENDER_HEIGHT + 5, 12, Color.Yellow, FontType.UI);
    }
    
    /// <summary>
    /// Casts a ray from the player position in the given direction
    /// </summary>
    private float CastRay(Vector2 startPos, float angle)
    {
        if (_dungeon == null) return MAX_DISTANCE;
        
        Vector2 rayDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        float stepSize = 0.05f; // Smaller step size for better accuracy
        Vector2 currentPos = startPos;
        
        for (float distance = 0; distance < MAX_DISTANCE; distance += stepSize)
        {
            // Check if we're outside dungeon bounds
            if (currentPos.X < 0 || currentPos.X >= _dungeon.Width ||
                currentPos.Y < 0 || currentPos.Y >= _dungeon.Height)
            {
                return distance;
            }
            
            // Check if we hit a wall
            if (!_dungeon.IsWalkable((int)currentPos.X, (int)currentPos.Y))
            {
                return distance;
            }
            
            // Move along the ray
            currentPos += rayDir * stepSize;
        }
        
        return MAX_DISTANCE;
    }
    
    /// <summary>
    /// Gets the wall color based on distance (darker for further walls)
    /// </summary>
    private Color GetWallColor(float distance)
    {
        int colorIndex = (int)(distance / MAX_DISTANCE * (_wallColors.Length - 1));
        colorIndex = Math.Clamp(colorIndex, 0, _wallColors.Length - 1);
        return _wallColors[colorIndex];
    }
    
    /// <summary>
    /// Converts a direction vector to an angle in radians
    /// </summary>
    private float GetDirectionAngle(Vector2 direction)
    {
        if (direction == Vector2.Zero)
            return 0.0f; // Default facing direction (right)
        
        return (float)Math.Atan2(direction.Y, direction.X);
    }
    
    /// <summary>
    /// Toggles the visibility of the 3D view
    /// </summary>
    public void ToggleVisibility()
    {
        _isVisible = !_isVisible;
    }
    
    /// <summary>
    /// Cleanup resources
    /// </summary>
    public void Dispose()
    {
        Raylib.UnloadRenderTexture(_renderTexture);
    }
    
    /// <summary>
    /// Generates a shimmering fog color for areas beyond the view limit
    /// </summary>
    private Color GetFogColor(float distance, int x, int y)
    {
        // Only show fog for areas at the maximum view distance (unseen areas)
        if (distance < MAX_DISTANCE) return Color.Blank;
        
        // Create shimmer effect using sine waves with different frequencies
        float shimmer1 = (float)Math.Sin(_fogAnimationTime * 2.0f + x * 0.05f) * 0.5f + 0.5f;
        float shimmer2 = (float)Math.Sin(_fogAnimationTime * 1.5f + x * 0.03f) * 0.5f + 0.5f;
        
        // Combine shimmers for subtle movement
        float combinedShimmer = (shimmer1 + shimmer2) / 2.0f;
        
        // Create a subtle fog intensity for the unseen areas
        int baseGray = 25 + (int)(combinedShimmer * 15); // Gray value between 25-40
        int alpha = (int)(80 + combinedShimmer * 30); // Alpha between 80-110
        
        return new Color(baseGray, baseGray + 3, baseGray + 6, alpha); // Slightly bluish tint
    }
}
