using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.GLFW;
using WLI_Input;
using WLO.Math;

namespace WLO.Window;

public unsafe class GLFW : WLI.Window{
    // убрать этот мусор с глаз моих долой

    #region МУСОР
    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern int SetDIBitsToDevice(IntPtr hdc, int xDest, int yDest, int w, int h, int xSrc, int ySrc, int startScan, int scanLines, void* pixels, void* bmi, uint colorUse);

    [StructLayout(LayoutKind.Sequential)]
    struct BITMAPINFOHEADER{
        public uint   biSize;
        public int    biWidth;
        public int    biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint   biCompression;
        public uint   biSizeImage;
        public int    biXPelsPerMeter;
        public int    biYPelsPerMeter;
        public uint   biClrUsed;
        public uint   biClrImportant;
    }
    #endregion
    
    private WindowHandle*    __Handle;
    private GlfwNativeWindow __Native;

    public GLFW_Keyboard Keyboard{ get; private set; }
    public GLFW_Mouse    Mouse{ get; private set; }
    
    // ----------------------------------------------------------------------

    public GLFW(Vector2I Size, string Title, bool UseOpenGL){ // todo...... БОЛЬШОЙ TODO........
        try{
            WL.GLFW.Start();

            if(UseOpenGL){
                WL.GLFW.API.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGL);
                WL.GLFW.API.WindowHint(WindowHintInt.ContextVersionMajor, 3);
                WL.GLFW.API.WindowHint(WindowHintInt.ContextVersionMinor, 3);
                WL.GLFW.API.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
            }else{
                WL.GLFW.API.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
            }

            __Handle = WL.GLFW.API.CreateWindow(Size.W, Size.H, Title, null, null);

            if(__Handle == null){ throw new ExceptionWL($"WL.GLFW.API.CreateWindow({Size.W}, {Size.H}, \"{Title}\", null, null) вернул null! Произошла ошибка при создании окна GLFW!"); }

            if(UseOpenGL){ WL.GLFW.API.MakeContextCurrent(__Handle); }
            
            __Native = new GlfwNativeWindow(WL.GLFW.API, __Handle);

            Keyboard = new GLFW_Keyboard(this);

            WL.GLFW.API.SetKeyCallback(__Handle, (Self, Key, Code, Action, Mods) => Keyboard.__InternalKeyCallback(Key, Action));
            WL.GLFW.API.SetCharCallback(__Handle, (Self, CodePoint) => Keyboard.__InternalCharCallback((char)CodePoint));
            
            Mouse = new GLFW_Mouse(this);

            WL.GLFW.API.SetCursorPosCallback(__Handle, (Self, X, Y) => Mouse.__InternalPositionCallback(X, Y));
            WL.GLFW.API.SetMouseButtonCallback(__Handle, (Self, Button, Action, Mods) => Mouse.__InternalButtonCallback(Button, Action));
            WL.GLFW.API.SetScrollCallback(__Handle, (Self, X, Y) => Mouse.__InternalScrollCallback(X, Y));
            
            WL.GLFW.API.SwapInterval(0); // todo, off vsync
        }
        catch(Exception e){
            throw new ExceptionWL($"Произошла ошибка при создании GLFW окна!\nnew WLO.Window.GLFW({Size}, \"{Title}\")\nРазмер: {Size}\nНазвание окна: \"{Title}\"", e);
        }
    }

    public GLFW(Vector2I Size, string Title) : this(Size, Title, true){}

    public static WLI.Window Create(Vector2I Size, string Title) => new GLFW(Size, Title);

    public IntPtr GetProcAddress(string ProcessName) => (IntPtr)WL.GLFW.API.GetProcAddress(ProcessName);

    public void SwapBuffers(){
        if(__Handle != null){ WL.GLFW.API.SwapBuffers(__Handle); }
    }
    
    public bool Close(){
        if(__Handle != null){
            WL.GLFW.API.SetWindowShouldClose(__Handle, true);
            WL.GLFW.API.DestroyWindow(__Handle);
            __Handle = null;
            
            WL.GLFW.MaybeStop();

            return true;
        }
        return false;
    }

    public bool IsClosed => __Handle == null || WL.GLFW.API.WindowShouldClose(__Handle);

    public Vector2I Size{
        get{
            if(__Handle == null){ return new Vector2I(0, 0); }
            WL.GLFW.API.GetWindowSize(__Handle, out int W, out int H);
            return new Vector2I(W, H);
        }
        set{
            if(__Handle != null){
                WL.GLFW.API.SetWindowSize(__Handle, value.W, value.H);
            }
        }
    }

    public float Aspect{
        get{
            Vector2I Size__ = Size;
            return (float)Size__.W / Size__.H;
        }
    }

    public Vector2I Position{
        get{
            if(__Handle == null){ return new Vector2I(0, 0); }
            WL.GLFW.API.GetWindowPos(__Handle, out int X, out int Y);
            return new Vector2I(X, Y);
        }
        set{
            if(__Handle != null){
                WL.GLFW.API.SetWindowPos(__Handle, value.X, value.Y);
            }
        }
    }

    private string __Title = String.Empty;
    public string Title{
        get => __Title;
        set{
            if(__Handle != null){
                __Title = value;
                WL.GLFW.API.SetWindowTitle(__Handle, value);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PollEvents(){ // todo, переделать, вынести в WL.GLFW
        Keyboard.__UpdateStates();
        WL.GLFW.API.PollEvents();
    }

    // todo, это если что типо медленный pollevents
    public void PollEvents2(){
        Mouse.__UpdateStates();
    }

    public void Present(FrameBuffer Buffer){
        // todo, obsolete
        if(__Handle == null){ return; }
        
        if(!__Native.Win32.HasValue){ return; }

        IntPtr HWND = __Native.Win32.Value.Hwnd;
        IntPtr HDC = GetDC(HWND);

        BITMAPINFOHEADER BMI = new BITMAPINFOHEADER{
            biSize = (uint)sizeof(BITMAPINFOHEADER),
            biWidth = Buffer.Size.W,
            biHeight = Buffer.Size.H,
            biPlanes = 1,
            biBitCount = 32,
            biCompression = 0
        };

        fixed(Color4B* Ptr = Buffer.Pixels){
            SetDIBitsToDevice(
                HDC, 0, 0, Buffer.Size.W, Buffer.Size.H,
                0, 0, 0, Buffer.Size.H,
                Ptr, &BMI, 0
            );
        }

        ReleaseDC(HWND, HDC);
    }
    
    // ----------------------------------------------------------------------
    
    public class GLFW_Keyboard : WLI_Input.Keyboard{
        private GLFW __Owner;
        
        public event Action<Keyboard.Key, bool>? OnKey;
        public event Action<char>? OnChar;
        
        public GLFW_Keyboard(GLFW Window){ __Owner = Window; }
        
        private readonly bool[] __Current  = new bool[512];
        private readonly bool[] __Previous = new bool[512];

        public void __UpdateStates() => Array.Copy(__Current, __Previous, 512);

        public void __InternalKeyCallback(Keys Key, InputAction Action){
            WLI_Input.Keyboard.Key WLKey = GLFWKeyToWLKey(Key);
            if(WLKey == WLI_Input.Keyboard.Key.Unknown){ return; }

            bool Down = Action != InputAction.Release;
            __Current[(int)WLKey] = Down;
            
            OnKey?.Invoke(WLKey, Down);
        }

        public void __InternalCharCallback(char Char){
            OnChar?.Invoke(Char);
        }

        public bool IsKeyDown(Keyboard.Key Key) => __Current[(int)Key];
        public bool IsKeyPressed(Keyboard.Key Key) => __Current[(int)Key] && !__Previous[(int)Key];

        public WLI_Input.Keyboard.Key GLFWKeyToWLKey(Keys Key) => Key switch{
            Keys.A => WLI_Input.Keyboard.Key.A,
            Keys.B => WLI_Input.Keyboard.Key.B,
            Keys.C => WLI_Input.Keyboard.Key.C,
            Keys.D => WLI_Input.Keyboard.Key.D,
            Keys.E => WLI_Input.Keyboard.Key.E,
            Keys.F => WLI_Input.Keyboard.Key.F,
            Keys.G => WLI_Input.Keyboard.Key.G,
            Keys.H => WLI_Input.Keyboard.Key.H,
            Keys.I => WLI_Input.Keyboard.Key.I,
            Keys.J => WLI_Input.Keyboard.Key.J,
            Keys.K => WLI_Input.Keyboard.Key.K,
            Keys.L => WLI_Input.Keyboard.Key.L,
            Keys.M => WLI_Input.Keyboard.Key.M,
            Keys.N => WLI_Input.Keyboard.Key.N,
            Keys.O => WLI_Input.Keyboard.Key.O,
            Keys.P => WLI_Input.Keyboard.Key.P,
            Keys.Q => WLI_Input.Keyboard.Key.Q,
            Keys.R => WLI_Input.Keyboard.Key.R,
            Keys.S => WLI_Input.Keyboard.Key.S,
            Keys.T => WLI_Input.Keyboard.Key.T,
            Keys.U => WLI_Input.Keyboard.Key.U,
            Keys.V => WLI_Input.Keyboard.Key.V,
            Keys.W => WLI_Input.Keyboard.Key.W,
            Keys.X => WLI_Input.Keyboard.Key.X,
            Keys.Y => WLI_Input.Keyboard.Key.Y,
            Keys.Z => WLI_Input.Keyboard.Key.Z,

            Keys.Number0 => WLI_Input.Keyboard.Key.D0,
            Keys.Number1 => WLI_Input.Keyboard.Key.D1,
            Keys.Number2 => WLI_Input.Keyboard.Key.D2,
            Keys.Number3 => WLI_Input.Keyboard.Key.D3,
            Keys.Number4 => WLI_Input.Keyboard.Key.D4,
            Keys.Number5 => WLI_Input.Keyboard.Key.D5,
            Keys.Number6 => WLI_Input.Keyboard.Key.D6,
            Keys.Number7 => WLI_Input.Keyboard.Key.D7,
            Keys.Number8 => WLI_Input.Keyboard.Key.D8,
            Keys.Number9 => WLI_Input.Keyboard.Key.D9,

            Keys.F1  => WLI_Input.Keyboard.Key.F1,
            Keys.F2  => WLI_Input.Keyboard.Key.F2,
            Keys.F3  => WLI_Input.Keyboard.Key.F3,
            Keys.F4  => WLI_Input.Keyboard.Key.F4,
            Keys.F5  => WLI_Input.Keyboard.Key.F5,
            Keys.F6  => WLI_Input.Keyboard.Key.F6,
            Keys.F7  => WLI_Input.Keyboard.Key.F7,
            Keys.F8  => WLI_Input.Keyboard.Key.F8,
            Keys.F9  => WLI_Input.Keyboard.Key.F9,
            Keys.F10 => WLI_Input.Keyboard.Key.F10,
            Keys.F11 => WLI_Input.Keyboard.Key.F11,
            Keys.F12 => WLI_Input.Keyboard.Key.F12,
            Keys.F13 => WLI_Input.Keyboard.Key.F13,
            Keys.F14 => WLI_Input.Keyboard.Key.F14,
            Keys.F15 => WLI_Input.Keyboard.Key.F15,
            Keys.F16 => WLI_Input.Keyboard.Key.F16,
            Keys.F17 => WLI_Input.Keyboard.Key.F17,
            Keys.F18 => WLI_Input.Keyboard.Key.F18,
            Keys.F19 => WLI_Input.Keyboard.Key.F19,
            Keys.F20 => WLI_Input.Keyboard.Key.F20,
            Keys.F21 => WLI_Input.Keyboard.Key.F21,
            Keys.F22 => WLI_Input.Keyboard.Key.F22,
            Keys.F23 => WLI_Input.Keyboard.Key.F23,
            Keys.F24 => WLI_Input.Keyboard.Key.F24,

            Keys.Escape      => WLI_Input.Keyboard.Key.Escape,
            Keys.Enter       => WLI_Input.Keyboard.Key.Enter,
            Keys.Space       => WLI_Input.Keyboard.Key.Space,
            Keys.Tab         => WLI_Input.Keyboard.Key.Tab,
            Keys.Backspace   => WLI_Input.Keyboard.Key.Backspace,
            Keys.Insert      => WLI_Input.Keyboard.Key.Insert,
            Keys.Delete      => WLI_Input.Keyboard.Key.Delete,
            Keys.PageUp      => WLI_Input.Keyboard.Key.PageUp,
            Keys.PageDown    => WLI_Input.Keyboard.Key.PageDown,
            Keys.Home        => WLI_Input.Keyboard.Key.Home,
            Keys.End         => WLI_Input.Keyboard.Key.End,
            Keys.CapsLock    => WLI_Input.Keyboard.Key.CapsLock,
            Keys.ScrollLock  => WLI_Input.Keyboard.Key.ScrollLock,
            Keys.NumLock     => WLI_Input.Keyboard.Key.NumLock,
            Keys.PrintScreen => WLI_Input.Keyboard.Key.PrintScreen,
            Keys.Pause       => WLI_Input.Keyboard.Key.Pause,

            Keys.Left  => WLI_Input.Keyboard.Key.Left,
            Keys.Right => WLI_Input.Keyboard.Key.Right,
            Keys.Up    => WLI_Input.Keyboard.Key.Up,
            Keys.Down  => WLI_Input.Keyboard.Key.Down,

            Keys.ShiftLeft    => WLI_Input.Keyboard.Key.ShiftL,
            Keys.ShiftRight   => WLI_Input.Keyboard.Key.ShiftR,
            Keys.ControlLeft  => WLI_Input.Keyboard.Key.ControlL,
            Keys.ControlRight => WLI_Input.Keyboard.Key.ControlR,
            Keys.AltLeft      => WLI_Input.Keyboard.Key.AltL,
            Keys.AltRight     => WLI_Input.Keyboard.Key.AltR,
            Keys.SuperLeft    => WLI_Input.Keyboard.Key.SuperL,
            Keys.SuperRight   => WLI_Input.Keyboard.Key.SuperR,
            Keys.Menu         => WLI_Input.Keyboard.Key.Menu,

            Keys.GraveAccent  => WLI_Input.Keyboard.Key.Grave,
            Keys.Minus        => WLI_Input.Keyboard.Key.Minus,
            Keys.Equal        => WLI_Input.Keyboard.Key.Equal,
            Keys.LeftBracket  => WLI_Input.Keyboard.Key.BracketL,
            Keys.RightBracket => WLI_Input.Keyboard.Key.BracketR,
            Keys.BackSlash    => WLI_Input.Keyboard.Key.Backslash,
            Keys.Semicolon    => WLI_Input.Keyboard.Key.Semicolon,
            Keys.Apostrophe   => WLI_Input.Keyboard.Key.Apostrophe,
            Keys.Comma        => WLI_Input.Keyboard.Key.Comma,
            Keys.Period       => WLI_Input.Keyboard.Key.Period,
            Keys.Slash        => WLI_Input.Keyboard.Key.Slash,

            Keys.Keypad0        => WLI_Input.Keyboard.Key.Num0,
            Keys.Keypad1        => WLI_Input.Keyboard.Key.Num1,
            Keys.Keypad2        => WLI_Input.Keyboard.Key.Num2,
            Keys.Keypad3        => WLI_Input.Keyboard.Key.Num3,
            Keys.Keypad4        => WLI_Input.Keyboard.Key.Num4,
            Keys.Keypad5        => WLI_Input.Keyboard.Key.Num5,
            Keys.Keypad6        => WLI_Input.Keyboard.Key.Num6,
            Keys.Keypad7        => WLI_Input.Keyboard.Key.Num7,
            Keys.Keypad8        => WLI_Input.Keyboard.Key.Num8,
            Keys.Keypad9        => WLI_Input.Keyboard.Key.Num9,
            Keys.KeypadDivide   => WLI_Input.Keyboard.Key.NumDivide,
            Keys.KeypadMultiply => WLI_Input.Keyboard.Key.NumMultiply,
            Keys.KeypadSubtract => WLI_Input.Keyboard.Key.NumSubtract,
            Keys.KeypadAdd      => WLI_Input.Keyboard.Key.NumAdd,
            Keys.KeypadEnter    => WLI_Input.Keyboard.Key.NumEnter,
            Keys.KeypadDecimal  => WLI_Input.Keyboard.Key.NumDecimal,
            
            var _ => WLI_Input.Keyboard.Key.Unknown
        };
    }

    public class GLFW_Mouse : WLI_Input.Mouse{
        private GLFW __Owner;
        
        public event Action<Mouse.Button, bool>? OnButton;
        public event Action<Vector2I, Vector2I>? OnMove;
        public event Action<Vector2F>? OnScroll;
        
        public GLFW_Mouse(GLFW Window){ __Owner = Window; }

        private          Vector2I __PrevPosition;
        private          Vector2F __ScrollAccumulator;
        private readonly bool[]   __Buttons = new bool[8];

        public void __UpdateStates(){
            __PrevPosition = Position;

            ScrollDelta = __ScrollAccumulator;
            __ScrollAccumulator = new Vector2F(0, 0);
        }

        public bool IsButtonDown(Mouse.Button Button) => __Buttons[(int)Button];

        public void __InternalPositionCallback(double X, double Y){
            Vector2I NewPosition = new Vector2I((int)X, (int)Y);
            Vector2I Delta = NewPosition - Position;
            Position = NewPosition;
            
            OnMove?.Invoke(NewPosition, Delta);
        }

        public void __InternalButtonCallback(MouseButton Button, InputAction Action){
            if((int)Button < __Buttons.Length){
                WLI_Input.Mouse.Button WLButton = GLFWButtonToWLButton(Button);
                
                bool Down = Action != InputAction.Release;
                __Buttons[(int)WLButton] = Down;
                
                OnButton?.Invoke(WLButton, Down);
            }
        }

        public void __InternalScrollCallback(double X, double Y){
            Vector2F Delta = new Vector2F((float)X, (float)Y);
            __ScrollAccumulator += Delta;
            
            OnScroll?.Invoke(Delta);
        }

        public Vector2I Position{ get; private set; }
        public Vector2I Delta => Position - __PrevPosition;
        public Vector2F ScrollDelta{ get; private set; }

        public WLI_Input.Mouse.Button GLFWButtonToWLButton(MouseButton Key) => Key switch{
            MouseButton.Left    => WLI_Input.Mouse.Button.Left,
            MouseButton.Right   => WLI_Input.Mouse.Button.Right,
            MouseButton.Middle  => WLI_Input.Mouse.Button.Middle,
            MouseButton.Button4 => WLI_Input.Mouse.Button.Button4,
            MouseButton.Button5 => WLI_Input.Mouse.Button.Button5,
            
            var _ => WLI_Input.Mouse.Button.Unknown
        };
    }
}