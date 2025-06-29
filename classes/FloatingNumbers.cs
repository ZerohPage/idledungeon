using Raylib_cs;
using System.Numerics;

namespace RaylibGame.Classes;

public class FloatingNumber
{
    private Vector2 _position;
    private Vector2 _velocity;
    private string _text;
    private Color _color;
    private float _lifetime;
    private float _maxLifetime;
    private int _fontSize;
    
    public bool IsExpired => _lifetime <= 0f;
    
    public FloatingNumber(Vector2 startPosition, string text, Color color, float lifetime = 2.0f, int fontSize = 16)
    {
        _position = startPosition;
        _text = text;
        _color = color;
        _lifetime = lifetime;
        _maxLifetime = lifetime;
        _fontSize = fontSize;
        
        // Random upward velocity with slight horizontal drift
        Random random = new Random();
        _velocity = new Vector2(
            (float)(random.NextDouble() - 0.5) * 20f, // -10 to +10 horizontal drift
            -30f - (float)random.NextDouble() * 20f    // -30 to -50 upward velocity
        );
    }
    
    public void Update(float deltaTime)
    {
        if (IsExpired) return;
        
        // Update position
        _position += _velocity * deltaTime;
        
        // Apply gravity (slow down upward movement)
        _velocity.Y += 60f * deltaTime;
        
        // Reduce horizontal velocity over time
        _velocity.X *= 0.95f;
        
        // Reduce lifetime
        _lifetime -= deltaTime;
    }
    
    public void Draw()
    {
        if (IsExpired) return;
        
        // Calculate alpha based on remaining lifetime
        float alpha = _lifetime / _maxLifetime;
        Color drawColor = new Color(_color.R, _color.G, _color.B, (int)(255 * alpha));
        
        // Draw text with outline for better visibility
        Raylib.DrawText(_text, (int)_position.X + 1, (int)_position.Y + 1, _fontSize, Color.Black);
        Raylib.DrawText(_text, (int)_position.X, (int)_position.Y, _fontSize, drawColor);
    }
}

public class FloatingNumberManager
{
    private List<FloatingNumber> _floatingNumbers;
    
    public FloatingNumberManager()
    {
        _floatingNumbers = new List<FloatingNumber>();
    }
    
    public void AddDamageNumber(Vector2 position, int damage, bool isCritical = false)
    {
        Color color = isCritical ? Color.Orange : Color.Red;
        int fontSize = isCritical ? 20 : 16;
        string text = isCritical ? $"{damage}!" : damage.ToString();
        
        // Add some random offset so multiple numbers don't overlap
        Random random = new Random();
        Vector2 offsetPosition = position + new Vector2(
            (float)(random.NextDouble() - 0.5) * 20f,
            (float)(random.NextDouble() - 0.5) * 10f
        );
        
        _floatingNumbers.Add(new FloatingNumber(offsetPosition, text, color, 2.0f, fontSize));
    }
    
    public void AddHealNumber(Vector2 position, int healing)
    {
        Color color = Color.Green;
        string text = $"+{healing}";
        
        // Add some random offset
        Random random = new Random();
        Vector2 offsetPosition = position + new Vector2(
            (float)(random.NextDouble() - 0.5) * 20f,
            (float)(random.NextDouble() - 0.5) * 10f
        );
        
        _floatingNumbers.Add(new FloatingNumber(offsetPosition, text, color, 2.0f, 16));
    }
    
    public void AddTextNumber(Vector2 position, string text, Color color, float lifetime = 2.0f, int fontSize = 16)
    {
        // Add some random offset
        Random random = new Random();
        Vector2 offsetPosition = position + new Vector2(
            (float)(random.NextDouble() - 0.5) * 20f,
            (float)(random.NextDouble() - 0.5) * 10f
        );
        
        _floatingNumbers.Add(new FloatingNumber(offsetPosition, text, color, lifetime, fontSize));
    }
    
    public void Update(float deltaTime)
    {
        // Update all floating numbers
        foreach (var number in _floatingNumbers)
        {
            number.Update(deltaTime);
        }
        
        // Remove expired numbers
        _floatingNumbers.RemoveAll(n => n.IsExpired);
    }
    
    public void Draw()
    {
        foreach (var number in _floatingNumbers)
        {
            number.Draw();
        }
    }
    
    public void Clear()
    {
        _floatingNumbers.Clear();
    }
}
