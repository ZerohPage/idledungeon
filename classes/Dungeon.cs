using Raylib_cs;
using System.Numerics;
using RaylibGame.Classes.Items;

namespace RaylibGame.Classes;

public enum TileType
{
    Wall,
    Floor,
    Door,
    Entrance,
    Exit
}

public struct Tile
{
    public TileType Type { get; set; }
    public bool IsExplored { get; set; }
    public bool IsWalkable => Type == TileType.Floor || Type == TileType.Door || Type == TileType.Entrance || Type == TileType.Exit;
    public Color Color => Type switch
    {
        TileType.Wall => Color.DarkGray,
        TileType.Floor => Color.LightGray,
        TileType.Door => Color.Brown,
        TileType.Entrance => Color.Green,
        TileType.Exit => Color.Red,
        _ => Color.Black
    };
}

public struct Room
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    
    public int CenterX => X + Width / 2;
    public int CenterY => Y + Height / 2;
    
    public bool Intersects(Room other)
    {
        return X < other.X + other.Width &&
               X + Width > other.X &&
               Y < other.Y + other.Height &&
               Y + Height > other.Y;
    }
}

public class Dungeon
{
    private readonly Random _random;
    private Tile[,] _tiles;
    private List<Room> _rooms;
    private List<Item> _items;
    
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int TileSize { get; set; } = 20;
    public int ViewRadius { get; set; } = 3; // How far the player can see
    public Vector2 EntrancePosition { get; private set; }
    public Vector2 ExitPosition { get; private set; }
    public IReadOnlyList<Item> Items => _items;
    
    public Dungeon(int width, int height, int? seed = null)
    {
        Width = width;
        Height = height;
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        _tiles = new Tile[width, height];
        _rooms = new List<Room>();
        _items = new List<Item>();
        
        Generate();
    }
    
    public TileType GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return TileType.Wall;
        
        return _tiles[x, y].Type;
    }
    
    public bool IsWalkable(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return false;
        
        return _tiles[x, y].IsWalkable;
    }
    
    public bool IsWalkable(Vector2 position)
    {
        int x = (int)(position.X / TileSize);
        int y = (int)(position.Y / TileSize);
        return IsWalkable(x, y);
    }
    
    public void UpdateExploredAreas(Vector2 playerPosition)
    {
        int playerTileX = (int)(playerPosition.X / TileSize);
        int playerTileY = (int)(playerPosition.Y / TileSize);
        
        // Mark the exact tile the player is standing on as explored
        if (playerTileX >= 0 && playerTileX < Width && playerTileY >= 0 && playerTileY < Height)
        {
            _tiles[playerTileX, playerTileY].IsExplored = true;
            
            // Also reveal adjacent walls so the player can see the dungeon structure
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int checkX = playerTileX + dx;
                    int checkY = playerTileY + dy;
                    
                    if (checkX >= 0 && checkX < Width && checkY >= 0 && checkY < Height)
                    {
                        // Reveal walls adjacent to explored floor tiles
                        if (_tiles[checkX, checkY].Type == TileType.Wall)
                        {
                            _tiles[checkX, checkY].IsExplored = true;
                        }
                    }
                }
            }
        }
    }
    
    private void Generate()
    {
        // Initialize all tiles as walls
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                _tiles[x, y] = new Tile { Type = TileType.Wall };
            }
        }
        
        // Generate rooms
        GenerateRooms();
        
        // Connect rooms with corridors
        ConnectRooms();
        
        // Place entrance and exit
        PlaceEntranceAndExit();
    }
    
    private void GenerateRooms()
    {
        const int maxAttempts = 300; // Increased attempts for larger dungeon
        const int minRoomSize = 4;
        const int maxRoomSize = 12; // Slightly larger max room size
        const int maxRooms = 25; // Increased from 8 to 25 rooms for 120x80 dungeon
        
        for (int attempt = 0; attempt < maxAttempts && _rooms.Count < maxRooms; attempt++)
        {
            var room = new Room
            {
                Width = _random.Next(minRoomSize, maxRoomSize + 1),
                Height = _random.Next(minRoomSize, maxRoomSize + 1)
            };
            
            room.X = _random.Next(1, Width - room.Width - 1);
            room.Y = _random.Next(1, Height - room.Height - 1);
            
            // Check if room intersects with existing rooms
            bool intersects = _rooms.Any(existingRoom => room.Intersects(existingRoom));
            
            if (!intersects)
            {
                _rooms.Add(room);
                CarveRoom(room);
            }
        }
    }
    
    private void CarveRoom(Room room)
    {
        for (int x = room.X; x < room.X + room.Width; x++)
        {
            for (int y = room.Y; y < room.Y + room.Height; y++)
            {
                _tiles[x, y] = new Tile { Type = TileType.Floor };
            }
        }
    }
    
    private void ConnectRooms()
    {
        for (int i = 0; i < _rooms.Count - 1; i++)
        {
            var roomA = _rooms[i];
            var roomB = _rooms[i + 1];
            
            if (_random.Next(2) == 0)
            {
                // Horizontal then vertical corridor
                CreateHorizontalCorridor(roomA.CenterX, roomB.CenterX, roomA.CenterY);
                CreateVerticalCorridor(roomB.CenterX, roomA.CenterY, roomB.CenterY);
            }
            else
            {
                // Vertical then horizontal corridor
                CreateVerticalCorridor(roomA.CenterX, roomA.CenterY, roomB.CenterY);
                CreateHorizontalCorridor(roomA.CenterX, roomB.CenterX, roomB.CenterY);
            }
        }
    }
    
    private void CreateHorizontalCorridor(int x1, int x2, int y)
    {
        int startX = Math.Min(x1, x2);
        int endX = Math.Max(x1, x2);
        
        for (int x = startX; x <= endX; x++)
        {
            if (y >= 0 && y < Height && x >= 0 && x < Width)
            {
                _tiles[x, y] = new Tile { Type = TileType.Floor };
            }
        }
    }
    
    private void CreateVerticalCorridor(int x, int y1, int y2)
    {
        int startY = Math.Min(y1, y2);
        int endY = Math.Max(y1, y2);
        
        for (int y = startY; y <= endY; y++)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                _tiles[x, y] = new Tile { Type = TileType.Floor };
            }
        }
    }
    
    private void PlaceEntranceAndExit()
    {
        if (_rooms.Count < 2) return;
        
        // Place entrance in first room
        var entranceRoom = _rooms[0];
        EntrancePosition = new Vector2(entranceRoom.CenterX * TileSize, entranceRoom.CenterY * TileSize);
        _tiles[entranceRoom.CenterX, entranceRoom.CenterY] = new Tile { Type = TileType.Entrance };
        
        // Place exit in last room
        var exitRoom = _rooms[_rooms.Count - 1];
        ExitPosition = new Vector2(exitRoom.CenterX * TileSize, exitRoom.CenterY * TileSize);
        _tiles[exitRoom.CenterX, exitRoom.CenterY] = new Tile { Type = TileType.Exit };
    }
    
    public void Draw()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var tile = _tiles[x, y];
                var position = new Vector2(x * TileSize, y * TileSize);
                
                if (tile.IsExplored)
                {
                    // Draw explored tiles (camera transformation handled by Raylib)
                    Raylib.DrawRectangleV(position, new Vector2(TileSize, TileSize), tile.Color);
                    
                    // Draw tile borders for better visibility
                    if (tile.Type == TileType.Wall)
                    {
                        Raylib.DrawRectangleLinesEx(
                            new Rectangle(position.X, position.Y, TileSize, TileSize),
                            1, Color.Black);
                    }
                }
                else
                {
                    // Draw fog of war (unexplored areas)
                    Raylib.DrawRectangleV(position, new Vector2(TileSize, TileSize), Color.Black);
                }
            }
        }

        // Draw items in explored areas
        foreach (var item in _items)
        {
            if (item.IsVisible && IsPositionExplored(item.Position))
            {
                item.Draw();
            }
        }
    }
    //test
    
    public void DrawMinimap(Vector2 position, int minimapSize, Vector2 playerPosition)
    {
        float scale = (float)minimapSize / Math.Max(Width, Height);
        int scaledWidth = (int)(Width * scale);
        int scaledHeight = (int)(Height * scale);

        // Draw minimap background
        Raylib.DrawRectangle((int)position.X - 2, (int)position.Y - 2, scaledWidth + 4, scaledHeight + 4, Color.Black);

        // Draw minimap tiles
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var tile = _tiles[x, y];
                if (tile.IsExplored && tile.Type != TileType.Wall)
                {
                    var tilePos = new Vector2(
                        position.X + x * scale,
                        position.Y + y * scale
                    );

                    Color tileColor = tile.Type switch
                    {
                        TileType.Floor => Color.White,
                        TileType.Entrance => Color.Green,
                        TileType.Exit => Color.Red,
                        _ => Color.Gray
                    };

                    Raylib.DrawRectangle((int)tilePos.X, (int)tilePos.Y,
                                       Math.Max(1, (int)scale), Math.Max(1, (int)scale), tileColor);
                }
            }
        }

        // Draw player position on minimap
        Vector2 playerTile = new Vector2(playerPosition.X / TileSize, playerPosition.Y / TileSize);
        Vector2 playerMinimapPos = new Vector2(
            position.X + playerTile.X * scale,
            position.Y + playerTile.Y * scale
        );

        Raylib.DrawCircle((int)playerMinimapPos.X, (int)playerMinimapPos.Y, 2, Color.Blue);
    }
    
    public List<Vector2> GetWalkablePositions()
    {
        var positions = new List<Vector2>();
        
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_tiles[x, y].IsWalkable)
                {
                    positions.Add(new Vector2(x * TileSize + TileSize / 2, y * TileSize + TileSize / 2));
                }
            }
        }
        
        return positions;
    }
    
    public Vector2 GetRandomWalkablePosition()
    {
        var walkablePositions = GetWalkablePositions();
        return walkablePositions.Count > 0 ? walkablePositions[_random.Next(walkablePositions.Count)] : Vector2.Zero;
    }

    /// <summary>
    /// Updates all items in the dungeon
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update</param>
    public void UpdateItems(float deltaTime)
    {
        foreach (var item in _items)
        {
            item.Update(deltaTime);
        }
    }

    /// <summary>
    /// Checks if a world position is in an explored area
    /// </summary>
    /// <param name="worldPosition">Position in world coordinates</param>
    /// <returns>True if the position is explored</returns>
    public bool IsPositionExplored(Vector2 worldPosition)
    {
        int tileX = (int)(worldPosition.X / TileSize);
        int tileY = (int)(worldPosition.Y / TileSize);
        
        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height)
            return false;
            
        return _tiles[tileX, tileY].IsExplored;
    }

    /// <summary>
    /// Gets items near a specific position
    /// </summary>
    /// <param name="position">Position to check around</param>
    /// <param name="radius">Search radius</param>
    /// <returns>List of items within radius</returns>
    public List<Item> GetItemsNearPosition(Vector2 position, float radius)
    {
        return _items.Where(item => 
            item.IsVisible && 
            Vector2.Distance(item.Position, position) <= radius)
            .ToList();
    }

    /// <summary>
    /// Removes an item from the dungeon
    /// </summary>
    /// <param name="item">Item to remove</param>
    public void RemoveItem(Item item)
    {
        _items.Remove(item);
    }

    /// <summary>
    /// Adds an item to the dungeon
    /// </summary>
    /// <param name="item">Item to add</param>
    public void AddItem(Item item)
    {
        _items.Add(item);
    }
}
