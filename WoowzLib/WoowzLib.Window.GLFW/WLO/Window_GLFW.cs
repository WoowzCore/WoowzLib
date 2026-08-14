using System.Runtime.CompilerServices;
using Silk.NET.GLFW;
using WLO;

namespace WoowzLib.Window.GLFW.WLO;

public unsafe class Window_GLFW : WLI.Window{
    private static readonly Glfw          __GLFW = Glfw.GetApi();
    private                 WindowHandle* __Handle;
    private static bool                   __IsInitialized = false;

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
    
    public void Create(int W, int H, string Title){
        InitGLFW();
        
        __GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 3);
        __GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 3);
        __GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

        __Handle = __GLFW.CreateWindow(W, H, Title, null, null);

        if(__Handle == null){ throw new Exception("Произошла ошибка при создании окна!"); }
        
        __GLFW.MakeContextCurrent(__Handle);
        
        __GLFW.SwapInterval(1);
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
    public void PollEvents() => __GLFW.PollEvents();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SwapBuffers(){
        if(__Handle != null){
            __GLFW.SwapBuffers(__Handle);
        }
    }
}