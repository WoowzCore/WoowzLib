using WLO;
using WLO.Math;

namespace WLI_Render;

// Рендер пикселями
public interface Software : WLI.Render{
    void DrawPixel(FrameBuffer Buffer, Vector2I Position, Color4B Color);
    void DrawRect(FrameBuffer Buffer, Rect2I Rect, Color4B Color);
    void DrawLine(FrameBuffer Buffer, Vector2I Start, Vector2I End, Color4B Color);
    void Clear(FrameBuffer Buffer, Color4B Color);
}