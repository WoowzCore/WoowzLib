using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using WLI_Render;
using WLI.GPU;
using WLO.GPU;
using WLO.Math;
using Buffer = WLI.GPU.Buffer;
using Shader = WLI.GPU.Shader;
using Texture = WLI.GPU.Texture;

namespace WLO.Render.Hardware;

public class OpenGL : WLI_Render.Hardware{
    #region Значения

        public GL API{ get; private set; } = null!;

        public readonly Func<string, IntPtr> API_ProcessLoader;
        
        
        public RenderView CurrentRenderView{ get; private set; } = null!;

        #region Логирование

            public WLI.Logger? CurrentLogger = null!;
        
            public DebugProc API_DebugLogger{ get; private set; } = null!;
            
            public readonly bool API_HasDebugLogger;

            public uint LogType_Initialization    = (uint)WLI.Logger.Type.Info;
            public uint LogType_InitDetails       = (uint)WLI.Logger.Type.Info;
            public uint LogType_DebugLogger_Info  = (uint)WLI.Logger.Type.Info;
            public uint LogType_DebugLogger_Warn  = (uint)WLI.Logger.Type.Warning;
            public uint LogType_DebugLogger_Error = (uint)WLI.Logger.Type.Error;
            public uint LogType_DebugLogger_Fatal = (uint)WLI.Logger.Type.Fatal;

            public bool DebugLogger_ThrowExceptionOnFatalErrors = true;
            
        #endregion
        
    #endregion
    // ----------------------------------------------------------------------
    #region Запуск/Остановка
    
        public bool IsStarted{ get; private set; }
    
        public OpenGL(Func<string, IntPtr> ProcessLoader, StartParameters? Parameters = null, bool StartImmediately = false){
            StartParameters Parameters__ = new StartParameters{
                DebugLogger = Parameters.HasValue && Parameters.Value.DebugLogger.HasValue ? Parameters.Value.DebugLogger.Value : false,
                UseThisLogger = Parameters.HasValue && Parameters.Value.UseThisLogger != null ? Parameters.Value.UseThisLogger : WL.Logger.CurrentLogger
                
            };
            
            API_ProcessLoader = ProcessLoader;
            API_HasDebugLogger = Parameters__.DebugLogger.Value;

            CurrentLogger = Parameters__.UseThisLogger;

            if(StartImmediately){ Start(); }
        }
        public struct StartParameters{
            public bool?       DebugLogger;
            public WLI.Logger? UseThisLogger;
        }
        
        public unsafe void Start(){
            try{
                if(IsStarted){ throw new ExceptionWL("Рендер OpenGL уже и так был запущен!"); }

                Log(LogType_Initialization, "Запуск OpenGL...");
                
                Log(LogType_InitDetails, $"Параметры! HasDebugLogger: {API_HasDebugLogger}");
                
                API = WL.OpenGL.CreateAPI(API_ProcessLoader);
                Log(LogType_InitDetails, $"API: {API}, ProcessLoader: {API_ProcessLoader}");
                
                if(API_HasDebugLogger){
                    API_DebugLogger = __DebugLoggerCallback;
                    WL.OpenGL.SetupDebugLogger(API, API_DebugLogger);
                    Log(LogType_InitDetails, $"DebugLogger: {API_DebugLogger}");
                }
                
                CurrentRenderView = new GLRenderView(this);
                Log(LogType_InitDetails, $"CurrentRenderView: {CurrentRenderView}");
            
                Log(LogType_Initialization, "OpenGL запущен!");
                
                IsStarted = true;
            }catch(Exception e){
                throw new ExceptionWL("Произошла ошибка при запуске OpenGL!", e);
            } 
        }
        
        public void Stop(){
            try{
                if(!IsStarted){ throw new ExceptionWL("Рендер OpenGL даже не был запущен!"); }
                
                Log(LogType_Initialization, "Остановка OpenGL...");
                
                API = null!;
                
                Log(LogType_Initialization, "OpenGL остановлен!");
                
                IsStarted = false;
            }catch(Exception e){
                throw new ExceptionWL("Произошла ошибка при остановке OpenGL!", e);
            } 
        }
    
    #endregion
    // ----------------------------------------------------------------------
    #region Остальное

        public void Log(uint Type, object Message, string Prefix = "GL"){
            CurrentLogger?.PrefixPush(Prefix);
            CurrentLogger?.Log(Type, Message);
            CurrentLogger?.PrefixPop();
        }

        private void __DebugLoggerCallback(GLEnum Source, GLEnum Type, int ID, GLEnum Severity, int L, nint MessagePtr, nint UserParam){
            string Message = Marshal.PtrToStringAnsi(MessagePtr, L);

            uint LogType = Severity switch{
                GLEnum.DebugSeverityHigh when DebugLogger_ThrowExceptionOnFatalErrors => throw new ExceptionWL($"Фатальная ошибка OpenGL: {Message}"),
                GLEnum.DebugSeverityHigh   => LogType_DebugLogger_Fatal,
                GLEnum.DebugSeverityMedium => LogType_DebugLogger_Error,
                GLEnum.DebugSeverityLow    => LogType_DebugLogger_Warn,
                var _                      => LogType_DebugLogger_Info
            };

            Log(LogType, Message, "GLDL");
        }
    
    #endregion
    // ----------------------------------------------------------------------

    public void FrameStart(RenderView? Target = null){
        RenderView View = Target ?? CurrentRenderView;
        API.Viewport(0, 0, (uint)View.Viewport.X, (uint)View.Viewport.Y);
    }
    
    public void FrameStop(){
        
    }
    
    public Buffer CreateBuffer(uint Usage, uint Size) => new GLBuffer(this, BufferTargetARB.ArrayBuffer, Size);

    public Shader CreateShader(string VertexSource, string FragmentSource) => new GLShader(this, VertexSource, FragmentSource);
    
    public unsafe Mesh CreateMesh<T>(T[] Vertices, uint[]? Indices = null) where T : unmanaged{
        GLBuffer VBO = new GLBuffer(this, BufferTargetARB.ArrayBuffer, (uint)(Vertices.Length * sizeof(T)));
        VBO.Update(Vertices);

        GLBuffer EBO = null!;
        if(Indices != null){
            EBO = new GLBuffer(this, BufferTargetARB.ElementArrayBuffer, (uint)(Indices.Length * sizeof(uint)));
            EBO.Update(Indices);
        }

        return new GLMesh(this, VBO, EBO, (uint)Vertices.Length, (uint)(Indices?.Length ?? 0));
    }
    
    public Texture CreateTexture(Vector2I Size, uint Format){
        throw new NotImplementedException();
    }
}