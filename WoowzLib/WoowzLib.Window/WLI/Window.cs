using WLO;
using WLO.Math;

namespace WLI;

public interface Window{
    static abstract Window Create(Vector2I Size, string Title);
    bool Close();

    bool IsClosed{ get; }
    Vector2I Size{ get; }

    void PollEvents();

    void Present(FrameBuffer Buffer);
}