using WLO;
using WLO.Math;

namespace WLI_Render;

// Рендер векторами/полигонами
public interface Hardware : WLI.Render{
    void Start();
    void Stop();

    void FrameStart();
    void FrameStop();
    
    void Clear(Color4B Color);

    void DrawFrameBuffer(FrameBuffer Buffer);

    Vector2I Viewport{ get; set; }
}