using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Web.WebView2.Core;
using WLO.Math;

namespace WLO.Web;

public class WebView2 : WLI.Web{
    #region МУСОР
        [StructLayout(LayoutKind.Sequential)]
        private struct MSG{
            public IntPtr Hwnd;
            public uint   Message;
            public IntPtr WParam;
            public IntPtr LParam;
            public uint   Time;
            public int    PtX;
            public int    PtY;
        }

        private const uint PM_REMOVE = 0x0001;

        [DllImport("user32.dll")] private static extern bool PeekMessage(out MSG Msg, IntPtr Hwnd, uint FilterMin, uint FilterMax, uint RemoveMsg);
        [DllImport("user32.dll")] private static extern bool TranslateMessage(ref MSG Msg);
        [DllImport("user32.dll")] private static extern IntPtr DispatchMessage(ref MSG Msg);
    #endregion
    
    private CoreWebView2Environment? __Environment;
    private CoreWebView2Controller?  __Controller;

    private Vector2I __Size    = Vector2I.Zero;
    private bool     __Visible = true;

    public event Action<string>? OnMessage;

    private WebView2(){}

    public static WebView2 Create() => new WebView2();

    public bool IsAttached => __Controller != null;

    public Vector2I Size{
        get => __Size;
        set{
            __Size = value;
            if(__Controller != null){
                __Controller.Bounds = new Rectangle(0, 0, value.W, value.H);
            }
        }
    }

    public bool Visible{
        get => __Visible;
        set{
            __Visible = value;
            if(__Controller != null){ __Controller.IsVisible = value; }
        }
    }

    public void __InternalAttach(IntPtr ParentHWND){
        try{
            Task<CoreWebView2Environment>? EnvironmentTask = CoreWebView2Environment.CreateAsync();
            __PumpUntilCompleted(EnvironmentTask);
            __Environment = EnvironmentTask.Result;

            Task<CoreWebView2Controller>? ControllerTask = __Environment.CreateCoreWebView2ControllerAsync(ParentHWND);
            __PumpUntilCompleted(ControllerTask);
            __Controller = ControllerTask.Result;

            __Controller.Bounds = new Rectangle(0, 0, __Size.W, __Size.H);
            __Controller.IsVisible = __Visible;

            __Controller.CoreWebView2.WebMessageReceived += (Sender, Args) => {
                OnMessage?.Invoke(Args.WebMessageAsJson);
            };
        }catch(Exception e){
            throw new ExceptionWL($"Произошла ошибка при присоединении WebView2 [{this}] к [{ParentHWND}]!", e);
        }
    }

    private void __PumpUntilCompleted(Task Task){
        while(!Task.IsCompleted){
            __InternalPollEvents();
        }
    }
    
    public void __InternalDetach(){
        __Controller?.Close();
        __Controller = null;
        
        __InternalPollEvents();
    }
    
    public void __InternalPollEvents(){
        while(PeekMessage(out MSG Msg, IntPtr.Zero, 0, 0, PM_REMOVE)){
            TranslateMessage(ref Msg);
            DispatchMessage (ref Msg);
        }
    }

    public void __InternalResize(Vector2I Size) => this.Size = Size;

    public void Navigate(string URL) => __Controller?.CoreWebView2.Navigate(URL);

    public void NavigateToString(string HTML) => __Controller?.CoreWebView2.NavigateToString(HTML);

    public void ExecuteScript(string Script){
        throw new NotImplementedException();
    }

    public void PostMessage(string JSON) => __Controller?.CoreWebView2.PostWebMessageAsJson(JSON);
}