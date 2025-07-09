using Raylib_cs;
using System.Numerics;

namespace RaylibGame.Classes;

public class Enemy
{
    protected Vector2 _position;
    protected float _radius;
    protected Color _color;
    
    // Basic enemy stats
    public int Health { get; protected set; }
    public int MaxHealth { get; protected set; }
    public int AttackDamage { get; protected set; }
    public string Name { get; protected set; }
    
    // Properties
    public Vector2 Position => _position;
    public float Radius => _radius;
    public bool IsAlive => Health > 0;
    
    public Enemy(Vector2 startPosition, string name = "Enemy")
    {
        _position = startPosition;
        _radius = 6.0f; // Slightly smaller than player
        _color = Color.Red;
        
        // Default stats
        Name = name;
        MaxHealth = 50;
        Health = MaxHealth;
        AttackDamage = 10;
    }
    
    public virtual void Update(float deltaTime)
    {
        // Basic update - override in derived classes for specific behavior
    }
    
    public virtual void Draw(Vector2 cameraOffset = default)
    {
        if (!IsAlive) return;
        
        // Apply camera offset to position
        Vector2 screenPosition = _position + cameraOffset;
        
        // Draw enemy circle
        Raylib.DrawCircleV(screenPosition, _radius, _color);
        
        // Draw enemy outline
        Raylib.DrawCircleLinesV(screenPosition, _radius, Color.White);
    }
    
    public virtual void TakeDamage(int damage)
    {
        Health = Math.Max(0, Health - damage);
        
        if (Health <= 0)
        {
            OnDeath();
        }
    }
    
    protected virtual void OnDeath()
    {
        _color = Color.DarkGray; // Make dead enemies gray
    }
    
    public virtual void SetPosition(Vector2 position)
    {
        _position = position;
    }
}
