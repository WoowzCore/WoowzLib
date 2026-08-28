using WLO;
using WLO.Math;

namespace WLI;

public interface Window{
    static abstract Window Create(Vector2I Size, string Title);
    bool Close();

    bool IsClosed{ get; }

    void PollEvents();

    void Present(Image Buffer);
    
    Vector2I Size{ get; set; }
    Vector2I Position{ get; set; }
    string Title{ get; set; }
    
    float Aspect{ get; }
}