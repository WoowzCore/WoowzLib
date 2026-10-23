using WLO.Math;

namespace WLI;

public interface Window{
    static abstract Window Create(Vector2I Size, string Title);
    
    bool Close();
    bool IsClosed{ get; }
    
    Vector2I Position{ get; set; }
    
    Vector2I Size{ get; set; }
    float Aspect{ get; }
    
    string Title{ get; set; }
    
    void PollEvents();

    void Attach(WindowElement Element);

    event Action? OnClose;
}