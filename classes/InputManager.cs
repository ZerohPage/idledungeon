using Raylib_cs;
using System.Numerics;

namespace RaylibGame.Classes;

/// <summary>
/// Static input manager that handles all input for the game
/// </summary>
public static class InputManager
{
    // Movement input state
    public static Vector2 MovementDirection { get; private set; }
    public static bool HasMovementInput { get; private set; }
    
    // Toggle keys
    public static bool IsAutoExploreTogglePressed { get; private set; }
    public static bool IsReachablePositionsTogglePressed { get; private set; }
    public static bool IsInventoryTogglePressed { get; private set; }
    public static bool IsDebugTogglePressed { get; private set; }
    public static bool IsRaycastTogglePressed { get; private set; }
    public static bool IsPausePressed { get; private set; }
    
    // Exploration speed controls
    public static bool IsSpeedUpPressed { get; private set; }
    public static bool IsSpeedDownPressed { get; private set; }
    
    // Menu/UI navigation
    public static bool IsMenuConfirmPressed { get; private set; }
    public static bool IsMenuCancelPressed { get; private set; }
    
    // Inventory navigation
    public static Vector2 InventoryNavigation { get; private set; }
    public static bool IsInventoryUsePressed { get; private set; }
    public static bool IsInventoryClosePressed { get; private set; }
    
    /// <summary>
    /// Updates all input states. Call this once per frame.
    /// </summary>
    public static void Update()
    {
        UpdateMovementInput();
        UpdateToggleKeys();
        UpdateMenuInput();
        UpdateInventoryInput();
    }
    
    private static void UpdateMovementInput()
    {
        Vector2 inputDirection = Vector2.Zero;
        
        // WASD and Arrow Keys for movement
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
        
        MovementDirection = inputDirection;
        HasMovementInput = inputDirection != Vector2.Zero;
    }
    
    private static void UpdateToggleKeys()
    {
        IsAutoExploreTogglePressed = Raylib.IsKeyPressed(KeyboardKey.Tab);
        IsReachablePositionsTogglePressed = Raylib.IsKeyPressed(KeyboardKey.R);
        IsInventoryTogglePressed = Raylib.IsKeyPressed(KeyboardKey.I);
        IsDebugTogglePressed = Raylib.IsKeyPressed(KeyboardKey.F3); // Changed from R to F3 to avoid conflict
        IsRaycastTogglePressed = Raylib.IsKeyPressed(KeyboardKey.F2); // F2 to toggle 3D view
        IsPausePressed = Raylib.IsKeyPressed(KeyboardKey.Escape);
        
        // Speed controls - support both regular and keypad plus/minus
        IsSpeedUpPressed = Raylib.IsKeyPressed(KeyboardKey.Equal) ||      // + key (shift + =)
                          Raylib.IsKeyPressed(KeyboardKey.KpAdd);          // Keypad +
        IsSpeedDownPressed = Raylib.IsKeyPressed(KeyboardKey.Minus) ||    // - key  
                            Raylib.IsKeyPressed(KeyboardKey.KpSubtract);   // Keypad -
    }
    
    private static void UpdateMenuInput()
    {
        IsMenuConfirmPressed = Raylib.IsKeyPressed(KeyboardKey.Space) || 
                              Raylib.IsKeyPressed(KeyboardKey.Enter);
        IsMenuCancelPressed = Raylib.IsKeyPressed(KeyboardKey.Escape);
    }
    
    private static void UpdateInventoryInput()
    {
        Vector2 navDirection = Vector2.Zero;
        
        // Arrow keys for inventory navigation
        if (Raylib.IsKeyPressed(KeyboardKey.Left))
        {
            navDirection.X = -1.0f;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Right))
        {
            navDirection.X = 1.0f;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Up))
        {
            navDirection.Y = -1.0f;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Down))
        {
            navDirection.Y = 1.0f;
        }
        
        InventoryNavigation = navDirection;
        IsInventoryUsePressed = Raylib.IsKeyPressed(KeyboardKey.Enter) || 
                               Raylib.IsKeyPressed(KeyboardKey.Space);
        IsInventoryClosePressed = Raylib.IsKeyPressed(KeyboardKey.I) || 
                                 Raylib.IsKeyPressed(KeyboardKey.Escape);
    }
    
    /// <summary>
    /// Gets the current mouse position
    /// </summary>
    public static Vector2 GetMousePosition()
    {
        return Raylib.GetMousePosition();
    }
    
    /// <summary>
    /// Checks if a mouse button was pressed this frame
    /// </summary>
    public static bool IsMouseButtonPressed(MouseButton button)
    {
        return Raylib.IsMouseButtonPressed(button);
    }
    
    /// <summary>
    /// Checks if a mouse button is currently being held down
    /// </summary>
    public static bool IsMouseButtonDown(MouseButton button)
    {
        return Raylib.IsMouseButtonDown(button);
    }
    
    /// <summary>
    /// Checks if a specific key was pressed this frame
    /// </summary>
    public static bool IsKeyPressed(KeyboardKey key)
    {
        return Raylib.IsKeyPressed(key);
    }
    
    /// <summary>
    /// Checks if a specific key is currently being held down
    /// </summary>
    public static bool IsKeyDown(KeyboardKey key)
    {
        return Raylib.IsKeyDown(key);
    }
    
    /// <summary>
    /// Resets all input states (useful for screen transitions)
    /// </summary>
    public static void Reset()
    {
        MovementDirection = Vector2.Zero;
        HasMovementInput = false;
        IsAutoExploreTogglePressed = false;
        IsReachablePositionsTogglePressed = false;
        IsInventoryTogglePressed = false;
        IsDebugTogglePressed = false;
        IsRaycastTogglePressed = false;
        IsPausePressed = false;
        IsMenuConfirmPressed = false;
        IsMenuCancelPressed = false;
        InventoryNavigation = Vector2.Zero;
        IsInventoryUsePressed = false;
        IsInventoryClosePressed = false;
        IsSpeedUpPressed = false;
        IsSpeedDownPressed = false;
    }
}
