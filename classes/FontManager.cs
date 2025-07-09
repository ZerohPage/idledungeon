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
            // Try to load custom fonts - use Sketch.ttf for all font types at normal resolution
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
            // Load font at higher resolution for better anti-aliasing when scaled
            int highResFontSize = fontSize * 4; // Load at 4x resolution for smoother scaling
            var font = Raylib.LoadFontEx(fileName, highResFontSize, null, 0);
            
            // Enable trilinear filtering for best quality scaling
            Raylib.SetTextureFilter(font.Texture, TextureFilter.Trilinear);
            
            // Generate mipmaps for the font texture for better scaling quality
            Raylib.GenTextureMipmaps(ref font.Texture);
            
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
            // Double the requested font size for better readability
            int actualFontSize = fontSize * 2;
            
            // Calculate proper scale from high-resolution font (loaded at 4x)
            float scale = (float)actualFontSize / font.BaseSize;
            
            // Add slight spacing between characters for better readability (based on original fontSize)
            float spacing = fontSize * 0.1f; // 10% of original font size for proper proportion
            
            // Draw black border by drawing text offset in 8 directions
            var position = new System.Numerics.Vector2(x, y);
            
            // Draw border (black text offset by 1 pixel in all directions)
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x - 1, y - 1), actualFontSize, spacing, Color.Black);
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x, y - 1), actualFontSize, spacing, Color.Black);
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x + 1, y - 1), actualFontSize, spacing, Color.Black);
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x - 1, y), actualFontSize, spacing, Color.Black);
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x + 1, y), actualFontSize, spacing, Color.Black);
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x - 1, y + 1), actualFontSize, spacing, Color.Black);
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x, y + 1), actualFontSize, spacing, Color.Black);
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x + 1, y + 1), actualFontSize, spacing, Color.Black);
            
            // Draw main text with high quality scaling
            Raylib.DrawTextEx(font, text, position, actualFontSize, spacing, color);
        }
        else
        {
            // Fall back to default Raylib text drawing with border (doubled size)
            int actualFontSize = fontSize * 2;
            
            // Draw border (black text offset by 1 pixel in all directions)
            Raylib.DrawText(text, x - 1, y - 1, actualFontSize, Color.Black);
            Raylib.DrawText(text, x, y - 1, actualFontSize, Color.Black);
            Raylib.DrawText(text, x + 1, y - 1, actualFontSize, Color.Black);
            Raylib.DrawText(text, x - 1, y, actualFontSize, Color.Black);
            Raylib.DrawText(text, x + 1, y, actualFontSize, Color.Black);
            Raylib.DrawText(text, x - 1, y + 1, actualFontSize, Color.Black);
            Raylib.DrawText(text, x, y + 1, actualFontSize, Color.Black);
            Raylib.DrawText(text, x + 1, y + 1, actualFontSize, Color.Black);
            
            // Draw main text
            Raylib.DrawText(text, x, y, actualFontSize, color);
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
            // Double the requested font size for better readability
            int actualFontSize = fontSize * 2;
            
            // Add slight spacing between characters for better readability (based on original fontSize)
            float spacing = fontSize * 0.1f; // 10% of original font size for proper proportion
            var textSize = Raylib.MeasureTextEx(font, text, actualFontSize, spacing);
            return (int)textSize.X;
        }
        else
        {
            // Fall back to default Raylib text measurement (doubled size)
            return Raylib.MeasureText(text, fontSize * 2);
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
            // Double the requested font size for better readability
            int actualFontSize = fontSize * 2;
            
            // Add slight spacing between characters for better readability (based on original fontSize)
            float spacing = fontSize * 0.1f; // 10% of original font size for proper proportion
            return Raylib.MeasureTextEx(font, text, actualFontSize, spacing);
        }
        else
        {
            // Fall back to default measurement (doubled size)
            int width = Raylib.MeasureText(text, fontSize * 2);
            return new System.Numerics.Vector2(width, fontSize * 2);
        }
    }
}

public enum FontType
{
    Default,
    Title,
    UI
}
