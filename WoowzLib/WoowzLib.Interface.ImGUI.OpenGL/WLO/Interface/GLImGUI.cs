using System.Numerics;
using Silk.NET.Core.Contexts;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using WLO.Math;
using WLO.Render.Hardware;

namespace WLO.Interface;

public class GLImGUI : WLO.Interface.ImGUI{
    private readonly OpenGL __Owner;

    public ImGuiController API{ get; private set; }
    
    private GLImGUI(Builder Builder){
        __Owner = Builder.TargetRender;

        API = new ImGuiController(__Owner.API, new GLImGUI_IView(this, Builder), new GLImGUI_IInputContext(this, Builder));
    }
    
    // ----------------------------------------------------------------------

    private double __TotalTime;
    
    public void Update(float DeltaTime){
        __TotalTime += DeltaTime;
        API.Update(DeltaTime);
    }
    
    public void Render() => API.Render();
    
    // ----------------------------------------------------------------------

    public class Builder{
        public readonly OpenGL TargetRender;

        public Func<Vector2I>? OnGetSize = null;
        
        public Builder(OpenGL Render){
            TargetRender = Render;
        }

        public GLImGUI Build() => new GLImGUI(this);
    }
    
    private class GLImGUI_IView : IView{
        private GLImGUI __Owner;

        public readonly Func<Vector2I> OnGetSize;
        
        public GLImGUI_IView(GLImGUI ImGUI, Builder Builder){
            __Owner = ImGUI;

            OnGetSize = Builder.OnGetSize ?? (() => new Vector2I(800, 600));
        }
        
        // ----------------------------------------------------------------------
        
        public Vector2D<int> Size{
            get{
                Vector2I Size__ = OnGetSize();
                return new Vector2D<int>(Size__.W, Size__.H);
            }
        }

        public Vector2D<int> FramebufferSize{
            get{
                Vector2I Size__ = OnGetSize();
                return new Vector2D<int>(Size__.W, Size__.H);
            }
        }
        
        // ----------------------------------------------------------------------
        
        public bool ShouldSwapAutomatically{ get; set; }
        public bool IsEventDriven{ get; set; }
        public bool IsContextControlDisabled{ get; set; }

        public double FramesPerSecond{ get; set; }
        public double UpdatesPerSecond{ get; set; }
        public GraphicsAPI API{ get; }
        public bool VSync{ get; set; }
        public VideoMode VideoMode{ get; }
        public int? PreferredDepthBufferBits{ get; }
        public int? PreferredStencilBufferBits{ get; }
        public Vector4D<int>? PreferredBitDepth{ get; }
        public int? Samples{ get; }
        public IGLContext? GLContext{ get; }
        public IVkSurface? VkSurface{ get; }
        public void Dispose(){}
        public INativeWindow? Native{ get; }
        public void Initialize(){}
        public void DoRender(){}
        public void DoUpdate(){}
        public void DoEvents(){}
        public void ContinueEvents(){}
        public void Reset(){}
        public void Focus(){}
        public void Close(){}
        public Vector2D<int> PointToClient(Vector2D<int> point){
            return new Vector2D<int>();
        }
        public Vector2D<int> PointToScreen(Vector2D<int> point){
            return new Vector2D<int>();
        }
        public Vector2D<int> PointToFramebuffer(Vector2D<int> point){
            return new Vector2D<int>();
        }
        public object Invoke(Delegate d, params object[] args){
            return null;
        }
        public void Run(Action onFrame){}
        public IntPtr Handle{ get; }
        public bool IsClosing{ get; }
        public double Time{ get; }
        public bool IsInitialized{ get; }
        public event Action<Vector2D<int>>? Resize;
        public event Action<Vector2D<int>>? FramebufferResize;
        public event Action? Closing;
        public event Action<bool>? FocusChanged;
        public event Action? Load;
        public event Action<double>? Update;
        public event Action<double>? Render;
    }
    
    private class GLImGUI_IInputContext : IInputContext{
        private readonly GLImGUI __Owner;

        private readonly GLImGUI_IKeyboard __Keyboard;
        private readonly GLImGUI_IMouse    __Mouse;
        
        public GLImGUI_IInputContext(GLImGUI ImGUI, Builder Builder){
            __Owner = ImGUI;

            __Keyboard = new GLImGUI_IKeyboard(this);
            __Mouse    = new GLImGUI_IMouse   (this);
            
            
        }
        
        // ----------------------------------------------------------------------

        public IReadOnlyList<IKeyboard> Keyboards => [__Keyboard];
        public IReadOnlyList<IMouse> Mice => [__Mouse];
        
        // ----------------------------------------------------------------------
        
        public void Dispose(){}
        public IntPtr Handle{ get; }
        public IReadOnlyList<IGamepad> Gamepads{ get; }
        public IReadOnlyList<IJoystick> Joysticks{ get; }
        public IReadOnlyList<IInputDevice> OtherDevices{ get; }
        public event Action<IInputDevice, bool>? ConnectionChanged;
    }

    private class GLImGUI_IKeyboard : IKeyboard{
        private readonly GLImGUI_IInputContext __Owner;
        public GLImGUI_IKeyboard(GLImGUI_IInputContext InputContext){ __Owner = InputContext; }
        
        public string Name{ get; }
        public int Index{ get; }
        public bool IsConnected{ get; } = true;
        public bool IsKeyPressed(Key key){ return false; }
        public bool IsScancodePressed(int scancode){ return false; }
        public void BeginInput(){}
        public void EndInput(){}
        public IReadOnlyList<Key> SupportedKeys{ get; } = Enum.GetValues<Key>();
        public string ClipboardText{ get; set; }
        public event Action<IKeyboard, Key, int>? KeyDown;
        public event Action<IKeyboard, Key, int>? KeyUp;
        public event Action<IKeyboard, char>? KeyChar;
    }

    private class GLImGUI_IMouse : IMouse{
        private readonly GLImGUI_IInputContext __Owner;

        public GLImGUI_IMouse(GLImGUI_IInputContext InputContext){ __Owner = InputContext; }
        
        public string Name{ get; }
        public int Index{ get; }
        public bool IsConnected{ get; } = true;
        public bool IsButtonPressed(MouseButton btn){ return false; }
        public IReadOnlyList<MouseButton> SupportedButtons{ get; } = Enum.GetValues<MouseButton>();
        public IReadOnlyList<ScrollWheel> ScrollWheels{ get; } = [default];
        public Vector2 Position{ get; set; }
        public ICursor Cursor{ get; }
        public int DoubleClickTime{ get; set; }
        public int DoubleClickRange{ get; set; }
        public event Action<IMouse, MouseButton>? MouseDown;
        public event Action<IMouse, MouseButton>? MouseUp;
        public event Action<IMouse, MouseButton, Vector2>? Click;
        public event Action<IMouse, MouseButton, Vector2>? DoubleClick;
        public event Action<IMouse, Vector2>? MouseMove;
        public event Action<IMouse, ScrollWheel>? Scroll;
    }
}