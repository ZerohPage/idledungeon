using Raylib_cs;
using System.Numerics;

namespace RaylibGame.Classes;

// Shadow geometry structure
public struct ShadowGeometry
{
    public Vector2[] Vertices; // 4 vertices for shadow quad

    public ShadowGeometry()
    {
        Vertices = new Vector2[4];
    }
}

// Light information structure
public class LightInfo
{
    public bool Active { get; set; }
    public bool Dirty { get; set; }
    public bool Valid { get; set; }
    
    public Vector2 Position { get; set; }
    public RenderTexture2D Mask { get; set; }
    public float OuterRadius { get; set; }
    public Rectangle Bounds { get; set; }
    
    public ShadowGeometry[] Shadows { get; set; }
    public int ShadowCount { get; set; }
    public Color LightColor { get; set; }
    
    public LightInfo()
    {
        Shadows = new ShadowGeometry[60]; // Max shadows per light
        for (int i = 0; i < Shadows.Length; i++)
        {
            Shadows[i] = new ShadowGeometry();
        }
        LightColor = Color.White;
    }
    
    public void Dispose()
    {
        if (Active)
        {
            Raylib.UnloadRenderTexture(Mask);
        }
    }
}

public class LightingSystem
{
    private const int MaxLights = 16;
    private const int MaxShadowsPerLight = 60;
    
    private LightInfo[] _lights;
    private RenderTexture2D _lightMask;
    private bool _initialized;
    
    public LightingSystem()
    {
        _lights = new LightInfo[MaxLights];
        for (int i = 0; i < MaxLights; i++)
        {
            _lights[i] = new LightInfo();
        }
    }
    
    public void Initialize()
    {
        if (_initialized) return;
        
        // Create global light mask
        _lightMask = Raylib.LoadRenderTexture(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        _initialized = true;
    }
    
    public void Dispose()
    {
        if (!_initialized) return;
        
        Raylib.UnloadRenderTexture(_lightMask);
        
        for (int i = 0; i < MaxLights; i++)
        {
            _lights[i].Dispose();
        }
        
        _initialized = false;
    }
    
    // Setup a new light
    public int SetupLight(float x, float y, float radius, Color color)
    {
        if (!_initialized) Initialize();
        
        for (int i = 0; i < MaxLights; i++)
        {
            if (!_lights[i].Active)
            {
                _lights[i].Active = true;
                _lights[i].Valid = false;
                _lights[i].Mask = Raylib.LoadRenderTexture(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                _lights[i].OuterRadius = radius;
                _lights[i].LightColor = color;
                _lights[i].Bounds = new Rectangle(0, 0, radius * 2, radius * 2);
                
                MoveLight(i, x, y);
                DrawLightMask(i);
                
                return i;
            }
        }
        
        return -1; // No available light slots
    }
    
    // Move a light and mark it as dirty
    public void MoveLight(int slot, float x, float y)
    {
        if (slot < 0 || slot >= MaxLights || !_lights[slot].Active) return;
        
        _lights[slot].Dirty = true;
        _lights[slot].Position = new Vector2(x, y);
        
        // Update cached bounds
        _lights[slot].Bounds = new Rectangle(
            x - _lights[slot].OuterRadius,
            y - _lights[slot].OuterRadius,
            _lights[slot].OuterRadius * 2,
            _lights[slot].OuterRadius * 2
        );
    }
    
    // Remove a light
    public void RemoveLight(int slot)
    {
        if (slot < 0 || slot >= MaxLights || !_lights[slot].Active) return;
        
        _lights[slot].Dispose();
        _lights[slot].Active = false;
    }
    
    // Compute shadow volume for an edge
    private void ComputeShadowVolumeForEdge(int slot, Vector2 sp, Vector2 ep)
    {
        if (_lights[slot].ShadowCount >= MaxShadowsPerLight) return;
        
        float extension = _lights[slot].OuterRadius * 2;
        
        Vector2 spVector = Vector2.Normalize(Vector2.Subtract(sp, _lights[slot].Position));
        Vector2 spProjection = Vector2.Add(sp, Vector2.Multiply(spVector, extension));
        
        Vector2 epVector = Vector2.Normalize(Vector2.Subtract(ep, _lights[slot].Position));
        Vector2 epProjection = Vector2.Add(ep, Vector2.Multiply(epVector, extension));
        
        var shadow = _lights[slot].Shadows[_lights[slot].ShadowCount];
        shadow.Vertices[0] = sp;
        shadow.Vertices[1] = ep;
        shadow.Vertices[2] = epProjection;
        shadow.Vertices[3] = spProjection;
        
        _lights[slot].Shadows[_lights[slot].ShadowCount] = shadow;
        _lights[slot].ShadowCount++;
    }
    
    // Draw light mask for a specific light (simplified version without advanced blending)
    private void DrawLightMask(int slot)
    {
        Raylib.BeginTextureMode(_lights[slot].Mask);
        
        Raylib.ClearBackground(Color.Black);
        
        // Draw light radius if valid
        if (_lights[slot].Valid)
        {
            // Draw a simple gradient circle for the light
            Raylib.DrawCircleGradient(
                (int)_lights[slot].Position.X,
                (int)_lights[slot].Position.Y,
                _lights[slot].OuterRadius,
                _lights[slot].LightColor,
                Color.Black
            );
        }
        
        // Draw shadows as black areas
        for (int i = 0; i < _lights[slot].ShadowCount; i++)
        {
            var vertices = _lights[slot].Shadows[i].Vertices;
            if (vertices.Length >= 4)
            {
                // Draw shadow as a black quad
                Raylib.DrawTriangle(vertices[0], vertices[1], vertices[2], Color.Black);
                Raylib.DrawTriangle(vertices[0], vertices[2], vertices[3], Color.Black);
            }
        }
        
        Raylib.EndTextureMode();
    }
    
    // Update a light's shadows based on obstacles
    public bool UpdateLight(int slot, Rectangle[] obstacles)
    {
        if (slot < 0 || slot >= MaxLights || !_lights[slot].Active || !_lights[slot].Dirty)
            return false;
        
        _lights[slot].Dirty = false;
        _lights[slot].ShadowCount = 0;
        _lights[slot].Valid = false;
        
        foreach (var obstacle in obstacles)
        {
            // Check if light is inside obstacle
            if (Raylib.CheckCollisionPointRec(_lights[slot].Position, obstacle))
                return false;
            
            // Skip if obstacle is outside light bounds
            if (!Raylib.CheckCollisionRecs(_lights[slot].Bounds, obstacle))
                continue;
            
            // Check edges and cast shadow volumes
            
            // Top edge
            Vector2 sp = new Vector2(obstacle.X, obstacle.Y);
            Vector2 ep = new Vector2(obstacle.X + obstacle.Width, obstacle.Y);
            if (_lights[slot].Position.Y > ep.Y)
                ComputeShadowVolumeForEdge(slot, sp, ep);
            
            // Right edge
            sp = ep;
            ep.Y += obstacle.Height;
            if (_lights[slot].Position.X < ep.X)
                ComputeShadowVolumeForEdge(slot, sp, ep);
            
            // Bottom edge
            sp = ep;
            ep.X -= obstacle.Width;
            if (_lights[slot].Position.Y < ep.Y)
                ComputeShadowVolumeForEdge(slot, sp, ep);
            
            // Left edge
            sp = ep;
            ep.Y -= obstacle.Height;
            if (_lights[slot].Position.X > ep.X)
                ComputeShadowVolumeForEdge(slot, sp, ep);
            
            // The obstacle itself (as a shadow)
            if (_lights[slot].ShadowCount < MaxShadowsPerLight)
            {
                var shadow = _lights[slot].Shadows[_lights[slot].ShadowCount];
                shadow.Vertices[0] = new Vector2(obstacle.X, obstacle.Y);
                shadow.Vertices[1] = new Vector2(obstacle.X, obstacle.Y + obstacle.Height);
                shadow.Vertices[2] = new Vector2(obstacle.X + obstacle.Width, obstacle.Y + obstacle.Height);
                shadow.Vertices[3] = new Vector2(obstacle.X + obstacle.Width, obstacle.Y);
                
                _lights[slot].Shadows[_lights[slot].ShadowCount] = shadow;
                _lights[slot].ShadowCount++;
            }
        }
        
        _lights[slot].Valid = true;
        DrawLightMask(slot);
        
        return true;
    }
    
    // Update all lights
    public void UpdateLights(Rectangle[] obstacles)
    {
        if (!_initialized) return;
        
        bool dirtyLights = false;
        
        for (int i = 0; i < MaxLights; i++)
        {
            if (UpdateLight(i, obstacles))
                dirtyLights = true;
        }
        
        // Update master light mask if any lights were dirty
        if (dirtyLights)
        {
            Raylib.BeginTextureMode(_lightMask);
            
            Raylib.ClearBackground(Color.Black);
            
            // Draw all light masks with additive blending
            for (int i = 0; i < MaxLights; i++)
            {
                if (_lights[i].Active)
                {
                    Raylib.DrawTextureRec(
                        _lights[i].Mask.Texture,
                        new Rectangle(0, 0, Raylib.GetScreenWidth(), -Raylib.GetScreenHeight()),
                        Vector2.Zero,
                        _lights[i].LightColor
                    );
                }
            }
            
            Raylib.EndTextureMode();
        }
    }
    
    // Draw the lighting overlay
    public void DrawLighting(float opacity = 1.0f)
    {
        if (!_initialized) return;
        
        // Create a color with the specified opacity
        Color drawColor = new Color((byte)255, (byte)255, (byte)255, (byte)(255 * opacity));
        
        Raylib.DrawTextureRec(
            _lightMask.Texture,
            new Rectangle(0, 0, Raylib.GetScreenWidth(), -Raylib.GetScreenHeight()),
            Vector2.Zero,
            drawColor
        );
    }
    
    // Draw debug information
    public void DrawDebug()
    {
        if (!_initialized) return;
        
        for (int i = 0; i < MaxLights; i++)
        {
            if (!_lights[i].Active) continue;
            
            // Draw light position
            Raylib.DrawCircle(
                (int)_lights[i].Position.X,
                (int)_lights[i].Position.Y,
                10,
                i == 0 ? Color.Yellow : Color.White
            );
            
            // Draw light bounds
            Raylib.DrawRectangleLinesEx(_lights[i].Bounds, 1, Color.Green);
            
            // Draw shadows for first light
            if (i == 0)
            {
                for (int s = 0; s < _lights[i].ShadowCount; s++)
                {
                    var vertices = _lights[i].Shadows[s].Vertices;
                    if (vertices.Length >= 4)
                    {
                        Raylib.DrawTriangle(vertices[0], vertices[1], vertices[2], Color.Purple);
                        Raylib.DrawTriangle(vertices[0], vertices[2], vertices[3], Color.Purple);
                    }
                }
            }
        }
    }
    
    // Get light information
    public LightInfo? GetLight(int slot)
    {
        if (slot < 0 || slot >= MaxLights) return null;
        return _lights[slot].Active ? _lights[slot] : null;
    }
    
    // Get number of active lights
    public int GetActiveLightCount()
    {
        int count = 0;
        for (int i = 0; i < MaxLights; i++)
        {
            if (_lights[i].Active) count++;
        }
        return count;
    }
}