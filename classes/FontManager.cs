using Raylib_cs;

namespace RaylibGame.Classes;

public static class FontManager
{
    private static Font _defaultFont;
    private static Font _titleFont;
    private static Font _uiFont;
    private static bool _fontsLoaded;
    
    public static Font DefaultFont => _fontsLoaded ? _defaultFont : Raylib.GetFontDefault();
    public static Font TitleFont => _fontsLoaded ? _titleFont : Raylib.GetFontDefault();
    public static Font UIFont => _fontsLoaded ? _uiFont : Raylib.GetFontDefault();
    
    public static void LoadFonts()
    {
        try
        {
            // Try to load custom fonts - use Sketch.ttf for all font types
            _defaultFont = LoadFontSafe("assets/fonts/Sketch.ttf", 16);
            _titleFont = LoadFontSafe("assets/fonts/Sketch.ttf", 40);
            _uiFont = LoadFontSafe("assets/fonts/Sketch.ttf", 14);
            
            _fontsLoaded = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FontManager: Font loading failed: {ex.Message}");
            // If font loading fails, we'll use the default font
            _fontsLoaded = false;
        }
    }
    
    private static Font LoadFontSafe(string fileName, int fontSize)
    {
        // Check if file exists before trying to load
        if (File.Exists(fileName))
        {
            // Load font with higher resolution for better quality when scaling
            int highResFontSize = fontSize * 2; // Load at 2x resolution for better quality
            var font = Raylib.LoadFontEx(fileName, highResFontSize, null, 0);
            
            // Enable bilinear filtering for smooth scaling
            Raylib.SetTextureFilter(font.Texture, TextureFilter.Bilinear);
            
            return font;
        }
        
        // Return default font if file doesn't exist
        return Raylib.GetFontDefault();
    }
    
    public static void UnloadFonts()
    {
        if (_fontsLoaded)
        {
            // Only unload if we actually loaded custom fonts
            if (_defaultFont.BaseSize != 0) Raylib.UnloadFont(_defaultFont);
            if (_titleFont.BaseSize != 0) Raylib.UnloadFont(_titleFont);
            if (_uiFont.BaseSize != 0) Raylib.UnloadFont(_uiFont);
            
            _fontsLoaded = false;
        }
    }
    
    // Helper methods for drawing text with managed fonts
    public static void DrawText(string text, int x, int y, int fontSize, Color color, FontType fontType = FontType.Default)
    {
        Font font = fontType switch
        {
            FontType.Title => TitleFont,
            FontType.UI => UIFont,
            _ => DefaultFont
        };
        
        if (_fontsLoaded && font.BaseSize != 0)
        {
            // Calculate proper scale for high-resolution font
            float scale = (float)fontSize / (font.BaseSize / 2f); // Divide by 2 because we loaded at 2x resolution
            
            // Add slight spacing between characters for better readability
            float spacing = fontSize * 0.05f; // 5% of font size
            
            // Draw black border by drawing text offset in 8 directions
            var position = new System.Numerics.Vector2(x, y);
            
            // Draw border (black text offset by 1 pixel in all directions)
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x - 1, y - 1), fontSize, spacing, Color.Black);
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x, y - 1), fontSize, spacing, Color.Black);
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x + 1, y - 1), fontSize, spacing, Color.Black);
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x - 1, y), fontSize, spacing, Color.Black);
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x + 1, y), fontSize, spacing, Color.Black);
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x - 1, y + 1), fontSize, spacing, Color.Black);
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x, y + 1), fontSize, spacing, Color.Black);
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x + 1, y + 1), fontSize, spacing, Color.Black);
            
            // Draw main text (white on top)
            Raylib.DrawTextEx(font, text, position, fontSize, spacing, Color.White);
        }
        else
        {
            // Fall back to default Raylib text drawing with border
            // Draw border (black text offset by 1 pixel in all directions)
            Raylib.DrawText(text, x - 1, y - 1, fontSize, Color.Black);
            Raylib.DrawText(text, x, y - 1, fontSize, Color.Black);
            Raylib.DrawText(text, x + 1, y - 1, fontSize, Color.Black);
            Raylib.DrawText(text, x - 1, y, fontSize, Color.Black);
            Raylib.DrawText(text, x + 1, y, fontSize, Color.Black);
            Raylib.DrawText(text, x - 1, y + 1, fontSize, Color.Black);
            Raylib.DrawText(text, x, y + 1, fontSize, Color.Black);
            Raylib.DrawText(text, x + 1, y + 1, fontSize, Color.Black);
            
            // Draw main text (white on top)
            Raylib.DrawText(text, x, y, fontSize, Color.White);
        }
    }
    
    public static int MeasureText(string text, int fontSize, FontType fontType = FontType.Default)
    {
        Font font = fontType switch
        {
            FontType.Title => TitleFont,
            FontType.UI => UIFont,
            _ => DefaultFont
        };
        
        if (_fontsLoaded && font.BaseSize != 0)
        {
            // Add slight spacing between characters for better readability
            float spacing = fontSize * 0.05f; // 5% of font size
            var textSize = Raylib.MeasureTextEx(font, text, fontSize, spacing);
            return (int)textSize.X;
        }
        else
        {
            // Fall back to default Raylib text measurement
            return Raylib.MeasureText(text, fontSize);
        }
    }
    
    public static System.Numerics.Vector2 MeasureTextEx(string text, int fontSize, FontType fontType = FontType.Default)
    {
        Font font = fontType switch
        {
            FontType.Title => TitleFont,
            FontType.UI => UIFont,
            _ => DefaultFont
        };
        
        if (_fontsLoaded && font.BaseSize != 0)
        {
            // Add slight spacing between characters for better readability
            float spacing = fontSize * 0.05f; // 5% of font size
            return Raylib.MeasureTextEx(font, text, fontSize, spacing);
        }
        else
        {
            // Fall back to default measurement
            int width = Raylib.MeasureText(text, fontSize);
            return new System.Numerics.Vector2(width, fontSize);
        }
    }
}

public enum FontType
{
    Default,
    Title,
    UI
}
