using WLO;
using WoowzLib.Core.WLO;

namespace WLI;

public interface Window{
    static abstract Window Create(Vector2I Size, string Title);
    void Close();

    bool IsClosed{ get; }
    Vector2I Size{ get; }

    void PollEvents();

    void Present(FrameBuffer Buffer);
}