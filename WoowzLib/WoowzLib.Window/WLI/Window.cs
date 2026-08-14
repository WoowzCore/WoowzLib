using WLO;

namespace WLI;

public interface Window{
    void Create(int W, int H, string Title);
    
    void Close();

    bool IsClosed{ get; }
    Vector2I Size{ get; }

    void PollEvents();
    void SwapBuffers();
}