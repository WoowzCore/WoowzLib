using System.Numerics;
using ImGuiNET;
using WLO.Math;

using API = ImGuiNET.ImGui;

namespace WLO.Interface;

public abstract class ImGUI : WLI.Engine{
    public ImGuiIOPtr IO{ get; private set; }
    
    public bool IsStarted{ get; protected set; }
    
    public virtual void Start(){
        try{
            if(IsStarted){ throw new ExceptionWL("ImGUI уже и так был запущен!"); }
            
            API.CreateContext();
            IO = API.GetIO();

            //IO.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            
            API.StyleColorsDark();
            
            IsStarted = true;
        }catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при создании ImGUI!", e);
        }
    }
    
    public void Stop(){
        try{
            if(!IsStarted){ throw new ExceptionWL("ImGUI даже не был запущен!"); }

            IsStarted = false;
        }catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при остановке ImGUI!", e);
        }
    }
    
    // ----------------------------------------------------------------------

    public void MousePosition(Vector2I Position){
        IO.AddMousePosEvent(Position.X, Position.Y);
    }

    public void MouseButton(int Button, bool Down){
        IO.AddMouseButtonEvent(Button, Down);
    }

    public void MouseScroll(Vector2F Delta){
        IO.AddMouseWheelEvent(Delta.X, Delta.Y);
    }
    
    public void FrameStart(float DT, Vector2I Viewport){
        IO.DeltaTime = DT;
        IO.DisplaySize = new Vector2(Viewport.X, Viewport.Y);
        
        API.NewFrame();
    }

    public void FrameEnd(){
        API.Render();
    }

    public void Render(){
        OnRender(API.GetDrawData());
    }

    protected abstract void OnRender(ImDrawDataPtr DrawData);
}