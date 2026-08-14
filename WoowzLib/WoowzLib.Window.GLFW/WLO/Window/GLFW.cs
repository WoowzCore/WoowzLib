using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.GLFW;
using WLI_Input;
using WLO;
using WoowzLib.Core.WLO;
using WoowzLib.Core.WLO.Math;

namespace WLO.Window;

public unsafe class GLFW : WLI.Window{
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
    
    private static readonly Glfw             __GLFW = Glfw.GetApi();
    private                 WindowHandle*    __Handle;
    private static          bool             __IsInitialized = false;
    private                 GlfwNativeWindow __Native;

    public GLFW_Keyboard Keyboard{ get; }

    // ----------------------------------------------------------------------
    
    public static void InitGLFW(){
        if(!__IsInitialized){
            if(!__GLFW.Init()){
                throw new Exception("Не получилось инициализировать GLFW!");
            }
            __IsInitialized = true;
        }
    }

    public static void TerminateGLFW(){
        if(__IsInitialized){
            __GLFW.Terminate();
            __IsInitialized = false;
        }
    }

    public GLFW(Vector2I Size, string Title){
        InitGLFW();
        
        __GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);

        __Handle = __GLFW.CreateWindow(Size.W, Size.H, Title, null, null);

        if(__Handle == null){ throw new Exception("Произошла ошибка при создании окна!"); }

        __Native = new GlfwNativeWindow(__GLFW, __Handle);

        Keyboard = new GLFW_Keyboard();

        __GLFW.SetKeyCallback(__Handle, (Self, Key, Code, Action, Mods) => {
            Keyboard.__HandleCallback(Key, Action);
        });
        
        __GLFW.SwapInterval(1);
    }

    public static WLI.Window Create(Vector2I Size, string Title){
        return new GLFW(Size, Title);
    }
    public void Close(){
        if(__Handle != null){
            __GLFW.SetWindowShouldClose(__Handle, true);
            __GLFW.DestroyWindow(__Handle);
            __Handle = null;
        }
    }

    public bool IsClosed => __Handle == null || __GLFW.WindowShouldClose(__Handle);

    public Vector2I Size{
        get{
            if(__Handle == null){ return new Vector2I(0, 0); }
            __GLFW.GetWindowSize(__Handle, out int W, out int H);
            return new Vector2I(W, H);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PollEvents(){
        Keyboard.__UpdateStates();
        __GLFW.PollEvents();
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
            _ => WLI_Input.Keyboard.Key.Unknown
        };
    }
}