using Silk.NET.OpenGL;
using WLO;

namespace WL;

public static class OpenGL{
    public static GL CreateAPI(Func<string, IntPtr> ProcessLoader){
        GL API = GL.GetApi(ProcessLoader);
        if(API == null!){ throw new ExceptionWL($"GL.GetApi({ProcessLoader}) вернул null!"); }
        return API;
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="API">todo</param>
    /// <param name="Synchronous">Ловить ошибку сразу, а не позже</param>
    /// <param name="UserParam">todo</param>
    public static unsafe void SetupDebugLogger(GL API, DebugProc DebugLogger, bool Synchronous = true, void* UserParam = null){
        API.Enable(EnableCap.DebugOutput);
        
        if(Synchronous){ API.Enable(EnableCap.DebugOutputSynchronous); }
        
        API.DebugMessageCallback(DebugLogger, UserParam);
        
        // todo, фильтрация
        //API.DebugMessageControl();
    }
}