using System.Drawing;
using Microsoft.Web.WebView2.Core;
using WLO.Math;

namespace WLO.Web;

public class WebView2 : WLI.Web{
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
            __Environment = CoreWebView2Environment.CreateAsync().GetAwaiter().GetResult();
            __Controller = __Environment.CreateCoreWebView2CompositionControllerAsync(ParentHWND).GetAwaiter().GetResult();

            __Controller.Bounds = new Rectangle(0, 0, __Size.W, __Size.H);
            __Controller.IsVisible = __Visible;

            __Controller.CoreWebView2.WebMessageReceived += (Sender, Args) => {
                OnMessage?.Invoke(Args.WebMessageAsJson);
            };
        }catch(Exception e){
            throw new ExceptionWL($"Произошла ошибка при присоединении WebView2 [{this}] к [{ParentHWND}]!", e);
        }
    }

    public void __InternalDetach(){
        __Controller?.Close();
        __Controller = null;
    }

    public void __InternalResize(Vector2I Size) => this.Size = Size;

    public void Navigate(string URL) => __Controller?.CoreWebView2.Navigate(URL);

    public void NavigateToString(string HTML) => __Controller?.CoreWebView2.NavigateToString(HTML);

    public void ExecuteScript(string Script){
        throw new NotImplementedException();
    }

    public void PostMessage(string JSON) => __Controller?.CoreWebView2.PostWebMessageAsJson(JSON);
}