using WoowzLib.Core.WLO;
using WoowzLib.Core.WLO.Math;

namespace WLI_Render;

// Рендер векторами/полигонами
public interface Hardware : WLI.Render{
    void Clear(Color4B Color);

    void DrawFrameBuffer(FrameBuffer Buffer);
}