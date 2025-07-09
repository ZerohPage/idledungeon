using Raylib_cs;
using System.Numerics;

namespace RaylibGame.Classes;

public class Skeleton : Enemy
{
    public Skeleton(Vector2 startPosition) : base(startPosition, "Skeleton")
    {
        // Skeleton-specific stats
        MaxHealth = 30;
        Health = MaxHealth;
        AttackDamage = 8;
        _radius = 5.0f;
        _color = Color.Beige;
    }
    
    public override void Draw(Vector2 cameraOffset = default)
    {
        if (!IsAlive) return;
        
        // Apply camera offset to position
        Vector2 screenPosition = _position + cameraOffset;
        
        // Draw skeleton as a beige circle with bone-like appearance
        Raylib.DrawCircleV(screenPosition, _radius, _color);
        
        // Draw darker outline
        Raylib.DrawCircleLinesV(screenPosition, _radius, Color.DarkBrown);
        
        // Draw simple "bone" cross pattern
        float crossSize = _radius * 0.6f;
        Raylib.DrawLineV(
            new Vector2(screenPosition.X - crossSize, screenPosition.Y),
            new Vector2(screenPosition.X + crossSize, screenPosition.Y),
            Color.White
        );
        Raylib.DrawLineV(
            new Vector2(screenPosition.X, screenPosition.Y - crossSize),
            new Vector2(screenPosition.X, screenPosition.Y + crossSize),
            Color.White
        );
    }
}
