using Raylib_cs;

namespace RaylibGame.Classes;

public class FontManager
{
    private Font _defaultFont;
    private Font _titleFont;
    private Font _uiFont;
    private bool _fontsLoaded;
    
    public Font DefaultFont => _fontsLoaded ? _defaultFont : Raylib.GetFontDefault();
    public Font TitleFont => _fontsLoaded ? _titleFont : Raylib.GetFontDefault();
    public Font UIFont => _fontsLoaded ? _uiFont : Raylib.GetFontDefault();
    
    public FontManager()
    {
        _fontsLoaded = false;
    }
    
    public void LoadFonts()
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
    
    private Font LoadFontSafe(string fileName, int fontSize)
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
    
    public void UnloadFonts()
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
    public void DrawText(string text, int x, int y, int fontSize, Color color, FontType fontType = FontType.Default)
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
            
            Raylib.DrawTextEx(font, text, new System.Numerics.Vector2(x, y), fontSize, spacing, color);
        }
        else
        {
            // Fall back to default Raylib text drawing
            Raylib.DrawText(text, x, y, fontSize, color);
        }
    }
    
    public int MeasureText(string text, int fontSize, FontType fontType = FontType.Default)
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
    
    public System.Numerics.Vector2 MeasureTextEx(string text, int fontSize, FontType fontType = FontType.Default)
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
