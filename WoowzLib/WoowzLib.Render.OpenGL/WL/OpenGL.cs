using Silk.NET.OpenGL;
using WLO;

namespace WL;

public static partial class OpenGL{
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
    
    public static uint CompileGLSL(GL API, WLI.GPU.Shader.Type Stage, string Source){
        try{
            uint Shader = API.CreateShader(Stage switch{
                WLI.GPU.Shader.Type.Vertex   => ShaderType.VertexShader,
                WLI.GPU.Shader.Type.Fragment => ShaderType.FragmentShader,
                WLI.GPU.Shader.Type.Geometry => ShaderType.GeometryShader,
                WLI.GPU.Shader.Type.Compute  => ShaderType.ComputeShader,
                var _ => throw new ArgumentOutOfRangeException(nameof(Stage), Stage, null)
            });

            API.ShaderSource (Shader, Source);
            API.CompileShader(Shader        );

            string InfoLog = API.GetShaderInfoLog(Shader);
            if(!string.IsNullOrEmpty(InfoLog)){
                throw new ExceptionWL($"Ошибка компиляции: {InfoLog}");
            }
            
            return Shader;
        }
        catch(Exception e){
            throw new ExceptionWL($"Произошла ошибка при компиляции OpenGL шейдера!\nGL: {API}\nТип шейдера: {Stage}\nКод шейдера:\n{Source}", e);
        }
    }
}