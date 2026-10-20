using WLO.Math;

namespace WLI;

public interface WindowElement{
    bool IsAttached{ get; }
    
    Vector2I Size{ get; set; }
    bool Visible{ get; set; }

    void __InternalAttach(IntPtr ParentHWND);
    void __InternalDetach();
    void __InternalResize(Vector2I Size);
}