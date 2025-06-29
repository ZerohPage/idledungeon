using Raylib_cs;
using System.Numerics;

namespace RaylibGame.Classes;

public enum CombatState
{
    NotInCombat,
    InCombat,
    PlayerWins,
    PlayerLoses
}

public class Combat
{
    private Player? _player;
    private Enemy? _currentEnemy;
    private CombatState _state;
    private float _combatTimer;
    private float _combatCooldown = 1.0f; // Time between attacks
    private Random _random;
    private FloatingNumberManager? _floatingNumbers;
    
    public CombatState State => _state;
    public Enemy? CurrentEnemy => _currentEnemy;
    public bool IsInCombat => _state == CombatState.InCombat;
    
    public Combat()
    {
        _state = CombatState.NotInCombat;
        _combatTimer = 0f;
        _random = new Random();
    }
    
    public void SetFloatingNumberManager(FloatingNumberManager floatingNumbers)
    {
        _floatingNumbers = floatingNumbers;
    }
    
    public void StartCombat(Player player, Enemy enemy)
    {
        _player = player;
        _currentEnemy = enemy;
        _state = CombatState.InCombat;
        _combatTimer = 0f;
        
        // Stop auto exploration when combat starts
        if (_player != null)
        {
            _player.IsAutoExploring = false;
        }
    }
    
    public void Update(float deltaTime)
    {
        if (_state != CombatState.InCombat || _player == null || _currentEnemy == null)
            return;
        
        _combatTimer -= deltaTime;
        
        if (_combatTimer <= 0f)
        {
            // Execute combat round
            ExecuteCombatRound();
            _combatTimer = _combatCooldown;
        }
        
        // Check combat end conditions
        if (!_player.IsAlive())
        {
            _state = CombatState.PlayerLoses;
        }
        else if (!_currentEnemy.IsAlive)
        {
            _state = CombatState.PlayerWins;
            // Resume auto exploration when player wins
            _player.IsAutoExploring = true;
        }
    }
    
    private void ExecuteCombatRound()
    {
        if (_player == null || _currentEnemy == null) return;
        
        // Calculate damage
        int playerDamage = CalculatePlayerDamage();
        int enemyDamage = _currentEnemy.AttackDamage;
        
        // Apply damage
        _player.TakeDamage(enemyDamage);
        _currentEnemy.TakeDamage(playerDamage);
        
        // Show floating damage numbers
        if (_floatingNumbers != null)
        {
            // Show damage to player (above player)
            Vector2 playerDamagePos = new Vector2(_player.Position.X, _player.Position.Y - 20);
            _floatingNumbers.AddDamageNumber(playerDamagePos, enemyDamage);
            
            // Show damage to enemy (above enemy)
            Vector2 enemyDamagePos = new Vector2(_currentEnemy.Position.X, _currentEnemy.Position.Y - 20);
            _floatingNumbers.AddDamageNumber(enemyDamagePos, playerDamage);
        }
    }
    
    private int CalculatePlayerDamage()
    {
        // Simple player damage calculation - could be made more complex later
        int baseDamage = 15; // Player does more damage than most enemies
        int variation = _random.Next(-2, 3); // Random variation of -2 to +2
        return Math.Max(1, baseDamage + variation);
    }
    
    public void EndCombat()
    {
        _player = null;
        _currentEnemy = null;
        _state = CombatState.NotInCombat;
        _combatTimer = 0f;
    }
    
    public void Draw()
    {
        if (_state != CombatState.InCombat || _player == null || _currentEnemy == null)
            return;
        
        // Draw combat UI
        int screenWidth = Raylib.GetScreenWidth();
        
        // Combat indicator
        string combatText = "COMBAT!";
        int fontSize = 24;
        int textWidth = Raylib.MeasureText(combatText, fontSize);
        Raylib.DrawText(combatText, (screenWidth - textWidth) / 2, 50, fontSize, Color.Red);
        
        // Show enemy info
        string enemyInfo = $"Fighting: {_currentEnemy.Name} ({_currentEnemy.Health}/{_currentEnemy.MaxHealth} HP)";
        Raylib.DrawText(enemyInfo, 10, 120, 16, Color.White);
        
        // Combat timer indicator
        float timerProgress = 1.0f - (_combatTimer / _combatCooldown);
        int barWidth = 200;
        int barHeight = 8;
        int barX = (screenWidth - barWidth) / 2;
        int barY = 80;
        
        Raylib.DrawRectangle(barX, barY, barWidth, barHeight, Color.DarkGray);
        Raylib.DrawRectangle(barX, barY, (int)(barWidth * timerProgress), barHeight, Color.Orange);
        Raylib.DrawRectangleLines(barX, barY, barWidth, barHeight, Color.White);
    }
}
