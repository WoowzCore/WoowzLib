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

            if(UseOpenGL){
                WL.GLFW.API.MakeContextCurrent(__Handle);
            }
            
            __Native = new GlfwNativeWindow(WL.GLFW.API, __Handle);

            Keyboard = new GLFW_Keyboard(this);

            WL.GLFW.API.SetKeyCallback(__Handle, (Self, Key, Code, Action, Mods) => Keyboard.__HandleCallback(Key, Action));
            
            Mouse = new GLFW_Mouse(this);

            WL.GLFW.API.SetCursorPosCallback(__Handle, (Self, X, Y) => Mouse.__HandlePositionCallback(X, Y));
            WL.GLFW.API.SetMouseButtonCallback(__Handle, (Self, Button, Action, Mods) => Mouse.__HandleButtonCallback(Button, Action));
            WL.GLFW.API.SetScrollCallback(__Handle, (Self, X, Y) => Mouse.__HandleScrollCallback(X, Y));
            
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
        
        public GLFW_Keyboard(GLFW Window){ __Owner = Window; }
        
        private readonly bool[] __Current  = new bool[512];
        private readonly bool[] __Previous = new bool[512];

        public void __UpdateStates() => Array.Copy(__Current, __Previous, 512);

        public void __HandleCallback(Keys Key, InputAction Action){
            int Key__ = (int)GLFWKeyToWLKey(Key);
            if(Key__ >= 0){ __Current[Key__] = Action != InputAction.Release; }
        }

        public bool IsKeyDown(Keyboard.Key Key) => __Current[(int)Key];
        public bool IsKeyPressed(Keyboard.Key Key) => __Current[(int)Key] && !__Previous[(int)Key];

        public Keyboard.Key GLFWKeyToWLKey(Keys Key) => Key switch{
            Keys.A => WLI_Input.Keyboard.Key.A,
            Keys.W => WLI_Input.Keyboard.Key.W,
            Keys.S => WLI_Input.Keyboard.Key.S,
            Keys.D => WLI_Input.Keyboard.Key.D,
            Keys.Up => WLI_Input.Keyboard.Key.Up,
            Keys.Down => WLI_Input.Keyboard.Key.Down,
            Keys.Left => WLI_Input.Keyboard.Key.Left,
            Keys.Right => WLI_Input.Keyboard.Key.Right,
            var _ => WLI_Input.Keyboard.Key.Unknown
        };
    }

    public class GLFW_Mouse : WLI_Input.Mouse{
        private GLFW __Owner;
        
        public GLFW_Mouse(GLFW Window){ __Owner = Window; }

        private          Vector2I __Position;
        private          Vector2I __PrevPosition;
        private          Vector2F __ScrollDelta;
        private          Vector2F __ScrollAccumulator;
        private readonly bool[]   __Buttons = new bool[8];

        public void __UpdateStates(){
            __PrevPosition = __Position;

            __ScrollDelta = __ScrollAccumulator;
            __ScrollAccumulator = new Vector2F(0, 0);
        }

        public bool IsButtonDown(Mouse.Button Button) => __Buttons[(int)Button];

        public void __HandlePositionCallback(double X, double Y){
            __Position = new Vector2I((int)X, (int)Y);
        }

        public void __HandleButtonCallback(MouseButton Button, InputAction Action){
            if((int)Button < __Buttons.Length){
                __Buttons[(int)Button] = Action != InputAction.Release;
            }
        }

        public void __HandleScrollCallback(double X, double Y){
            __ScrollAccumulator = new Vector2F(__ScrollAccumulator.X + (float)X, __ScrollAccumulator.Y + (float)Y);
        }

        public Vector2I Position => __Position;
        public Vector2I Delta => __Position - __PrevPosition;
        public Vector2F ScrollDelta => __ScrollDelta;
    }
}