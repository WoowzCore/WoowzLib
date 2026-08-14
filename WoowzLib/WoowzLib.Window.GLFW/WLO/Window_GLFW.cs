using WLO;

namespace WoowzLib.Window.GLFW.WLO;

public class Window_GLFW : WLI.Window{
    public void Create(int W, int H, string Title){
        throw new NotImplementedException();
    }
    public void Close(){
        throw new NotImplementedException();
    }
    public bool IsClosed{ get; }
    public Vector2I Size{ get; }
    public void PollEvents(){
        throw new NotImplementedException();
    }
    public void SwapBuffers(){
        throw new NotImplementedException();
    }
}