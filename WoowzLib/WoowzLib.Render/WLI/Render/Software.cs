using WLO;
using WLO.Math;

namespace WLI_Render;

// Рендер пикселями
public interface Software : WLI.Render{
    void DrawPixel(Image Buffer, Vector2I Position, Color4B Color);
    void DrawRect(Image Buffer, Rect2I Rect, Color4B Color);
    void DrawLine(Image Buffer, Vector2I Start, Vector2I End, Color4B Color);
    void Clear(Image Buffer, Color4B Color);
}